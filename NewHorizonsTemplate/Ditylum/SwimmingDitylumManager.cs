using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.Ditylum
{
    public enum SwimState
    {
        TUMBLE, STEADY, SWIMMING, IMPULSE, GONE
    }

    public class SwimmingDitylumManager : MonoBehaviour
    {
        private OWRigidbody thisBody = null;
        private OWRigidbody dimensionBody = null;
        private PlayerLockOnTargeting lockOnThing = null;
        private float swimAccel = 7f;
        private float disableTime = -1;
        private SwimState swimState = SwimState.TUMBLE;

        /**
         * Grab the player lock on thingy when we start up
         */
        private void Start()
        {
            //Basic initialization
            lockOnThing = Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>();
            thisBody = gameObject.GetComponent<OWRigidbody>();
            ForgottenLocator.swimmingDitylum = this;
            dimensionBody = thisBody.GetOrigParentBody();

            //Adjust position
            transform.position = Vector3.MoveTowards(transform.position, dimensionBody.transform.position, 15);

            //Prime starting velocity
            Vector3 move = dimensionBody.transform.position - transform.position;
            move = move.normalized * 6;
            thisBody.SetVelocity(move);

            //Disable the object
            thisBody.Suspend();
            GetComponent<Animator>().speed = 0;
            foreach(SkinnedMeshRenderer rend in GetComponentsInChildren<SkinnedMeshRenderer>())
                rend.gameObject.SetActive(false);
        }

        /**
         * Forces the player to look at this object
         */
        public void LockPlayerOn()
        {
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
            OWInput.ChangeInputMode(InputMode.Character);
            Locator.GetPauseCommandListener().RemovePauseCommandLock();
            lockOnThing.BreakLock();
            Locator.GetPlayerCameraController().SnapToDegrees(0, 0, 60, true);
        }

        /**
         * Begins the processes of Ditylum
         */
        public void BeginSequence()
        {
            foreach (SkinnedMeshRenderer rend in GetComponentsInChildren<SkinnedMeshRenderer>(true))
                rend.gameObject.SetActive(true);
            thisBody.Unsuspend(true);
            GetComponent<Animator>().speed = 1;
            transform.LookAt(dimensionBody.transform, Locator.GetPlayerTransform().up);
        }

        /**
         * Used by the animations to set the state
         */
        public void SetState(int state)
        {
            if(swimState != SwimState.GONE)
                swimState = (SwimState)state;
        }

        /**
         * Apply some acceleration every frame, and despawn when far enough away
         */
        private void Update()
        {
            //Have it move towards original parent body from rigidbody
            switch(swimState)
            {
                case SwimState.STEADY:
                    thisBody.SetVelocity(Vector3.MoveTowards(thisBody.GetVelocity(), Vector3.zero, 3 * Time.deltaTime));
                    break;

                case SwimState.IMPULSE:
                    thisBody.AddAcceleration(swimAccel * transform.forward);
                    break;
            }

            //If he's swimming and gets too far away, fade him out
            if((swimState == SwimState.IMPULSE || swimState == SwimState.SWIMMING) && Vector3.Distance(transform.position, Locator.GetPlayerBody().transform.position) > 200)
            {
                disableTime = Time.time + 5.0f;
                swimState = SwimState.GONE;
                UnlockPlayer();
                GetComponent<SectorCullGroup>().SetVisible(false);
            }

            //If it's time to disable him, do so
            if (disableTime > 0 && Time.time > disableTime)
            {
                thisBody.Suspend();
                GetComponent<Animator>().speed = 0;
                foreach (SkinnedMeshRenderer rend in GetComponentsInChildren<SkinnedMeshRenderer>())
                    rend.gameObject.SetActive(false);
            }
        }
    }
}
