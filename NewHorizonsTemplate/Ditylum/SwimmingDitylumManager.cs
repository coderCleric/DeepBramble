using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.Ditylum
{
    public class SwimmingDitylumManager : MonoBehaviour
    {
        private OWRigidbody body = null;
        private PlayerLockOnTargeting lockOnThing = null;
        private bool activated = false;
        private float swimSpeed = 7f;
        private bool isSwimming = false;
        private float disableTime = -1;

        /**
         * Grab the player lock on thingy when we start up
         */
        private void Start()
        {
            lockOnThing = Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>();
        }

        /**
         * Forces the player to look at this object
         */
        public void LockPlayerOn()
        {
            activated = true;

            //Grab the body, if we don't have it already
            if(body == null)
            {
                body = transform.parent.gameObject.GetComponent<OWRigidbody>();
                body.SetIsTargetable(false);
            }

            //Actually lock the player on
            OWInput.ChangeInputMode(InputMode.None);
            Locator.GetPauseCommandListener().AddPauseCommandLock();
            Locator.GetToolModeSwapper().UnequipTool();
            Locator.GetFlashlight().TurnOff(playAudio: false);
            Locator.GetPlayerBody().SetVelocity(Vector3.zero);
            lockOnThing.LockOn(transform);
        }

        /**
         * Unlocks the player
         */
        public void UnlockPlayer()
        {
            activated = false;
            OWInput.ChangeInputMode(InputMode.Character);
            Locator.GetPauseCommandListener().RemovePauseCommandLock();
            lockOnThing.BreakLock();
        }

        /**
         * Starts it moving towards the center of its sector
         */
        public void StartMoving()
        {
            isSwimming = true;
        }

        /**
         * Apply some acceleration every frame, and despawn when far enough away
         */
        private void Update()
        {
            //Schmoovin' time
            if (isSwimming)
            {
                if (body != null)
                {
                    Vector3 move = body._origParent.transform.position - transform.position;
                    move = move.normalized;
                    move *= swimSpeed;
                    body.AddAcceleration(move);
                }

                //If we get too far away, fade away and unlock the player
                if(Vector3.Distance(transform.position, Locator.GetPlayerBody().transform.position) > 200)
                {
                    isSwimming = false;
                    disableTime = Time.time + 5.0f;
                    UnlockPlayer();
                    GetComponent<SectorCullGroup>().SetVisible(false);
                }
            }

            //If we should disable, do so
            if(disableTime > 0 && Time.time  > disableTime)
                transform.parent.gameObject.SetActive(false);
        }
    }
}
