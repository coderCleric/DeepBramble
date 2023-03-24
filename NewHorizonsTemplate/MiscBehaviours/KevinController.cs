using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.MiscBehaviours
{
    enum KevinState
    {
        HIDDEN, ATSTART, ATEND, MOVING
    }

    class KevinController : MonoBehaviour
    {
        //Animation change times
        private float lookTime = 1;
        private float accelTime = 1;

        //Animation controlling variables
        //Looking
        private float lookStartTime = 0;
        private float lookStartX = 0;
        private float lookTargetX = 0;
        private float lookX = 0;
        //Speed
        private float accelStartTime = 0;
        private float startSpeed = 0;
        private float targetSpeed = 0;
        private float speed = 0;

        //Other things
        public KevinState state = KevinState.HIDDEN;
        private Animator travelAnimator = null;
        private Animator bodyAnimator = null;
        private OWAudioSource longRangeSource = null;
        private PlayerAttachPoint attachPoint = null;
        private InteractReceiver handleReceiver = null;
        private OWTriggerVolume[] eyeTriggers;

        /**
         * Need to grab necessary components and initialize a proper starting state
         */
        private void Start()
        {
            //Set up animator stuff
            travelAnimator = GetComponent<Animator>();
            bodyAnimator = transform.Find("Beast_Anglerfish").gameObject.GetComponent<Animator>();
            bodyAnimator.runtimeAnimatorController = Patches.anglerAnimator.runtimeAnimatorController;
            bodyAnimator.SetFloat("MoveSpeed", 0);

            //Set up the handle stuff
            attachPoint = GetComponentInChildren<PlayerAttachPoint>();
            attachPoint.SetAttachOffset(new Vector3(1, 0, 0));
            handleReceiver = attachPoint.gameObject.GetComponent<InteractReceiver>();
            handleReceiver.OnPressInteract += MoveToEnd;
            handleReceiver.ChangePrompt("Grab");
            handleReceiver.DisableInteraction();

            //Grab & set up the eye triggers
            eyeTriggers = transform.Find("Beast_Anglerfish/B_angler_root/B_angler_body01/B_angler_body02/eye_triggers").gameObject.GetComponentsInChildren<OWTriggerVolume>();
            foreach(OWTriggerVolume trigger in eyeTriggers)
            {
                trigger.OnEntry += EyeHitDetected;
            }

            //Grab the audio sources
            longRangeSource = transform.Find("AudioController/OneShotSource_LongRange").gameObject.GetComponent<OWAudioSource>();
        }
        
        /**
         * Does everything needed for Kevin to come to the start from being hidden
         */
        private void EyeHitDetected(GameObject other)
        {
            if(state == KevinState.HIDDEN && other.CompareTag("ProbeDetector"))
            {
                state = KevinState.MOVING;
                longRangeSource.pitch = UnityEngine.Random.Range(0.8f, 1f);
                longRangeSource.PlayOneShot(AudioType.DBAnglerfishDetectDisturbance);

                //Get Kevin moving
                travelAnimator.SetTrigger("eye_hit");
            }
        }

        /**
         * Does everything needed for Kevin to start moving from the start to the end. Should be called when the handles are grabbed
         */
        private void MoveToEnd()
        {
            if (state == KevinState.ATSTART)
            {
                state = KevinState.MOVING;

                //Connect the player and disable the interaction
                attachPoint.AttachPlayer();
                handleReceiver.DisableInteraction();
                Locator.GetPlayerBody().GetComponent<PlayerCharacterController>().EnableZeroGMovement(); //This allows the player to look horizontally, for some reason
                Patches.playerAttachedToKevin = true;

                //Then, get Kevin moving
                travelAnimator.SetTrigger("begin_travel");
            }
        }

        /**
         * Does the stuff necessary for Kevin to warp back to the start
         */
        public void TeleportBack()
        {
            if(state == KevinState.ATEND)
            {
                state = KevinState.ATSTART;
                travelAnimator.SetTrigger("teleport_back");
                handleReceiver.EnableInteraction();
            }
        }

        /**
         * Does everything needed when Kevin reaches the end
         */
        public void ArriveAtEnd()
        {
            state = KevinState.ATEND;
            Patches.playerAttachedToKevin = false;
            attachPoint.DetachPlayer();
            Locator.GetPlayerBody().GetComponent<PlayerCharacterController>().DisableZeroGMovement();
        }

        /**
         * Does everything needed when Kevin reaches the start
         */
        public void ArriveAtStart()
        {
            state = KevinState.ATSTART;
            handleReceiver.EnableInteraction();
        }

        /**
         * Used by animation events to set horizontal look
         * 
         * @param look The x look amount
         */
        public void SetLookX(float x)
        {
            x = Mathf.Clamp(x, -1, 1);

            //Can't set it directly, need to tell update how to do it
            lookStartTime = Time.time;
            lookStartX = lookX;
            lookTargetX = x;
        }

        /**
         * Used by animation events to control apparent speed
         * 
         * @param speed The speed to go to
         */
        public void SetSpeed(float speed)
        {
            speed = Mathf.Clamp(speed, 0, 1);

            //Can't set it directly, need to tell update how to do it
            accelStartTime = Time.time;
            startSpeed = this.speed;
            targetSpeed = speed;
        }

        /**
         * Some animation things need updated every frame
         */
        private void Update()
        {
            //Update the horizontal look
            float lookAmount = Mathf.Abs((lookTargetX - lookStartX));
            float lerpTime = (Time.time - lookStartTime) / (lookTime * lookAmount);
            lerpTime = Mathf.Min(lerpTime, 1);
            lookX = Mathf.Lerp(lookStartX, lookTargetX, lerpTime);
            bodyAnimator.SetFloat("LookX", lookX);

            //Update the apparent speed
            float speedAmount = Mathf.Abs((targetSpeed - startSpeed));
            lerpTime = (Time.time - accelStartTime) / (accelTime * speedAmount);
            lerpTime = Mathf.Min(lerpTime, 1);
            speed = Mathf.Lerp(startSpeed, targetSpeed, lerpTime);
            bodyAnimator.SetFloat("MoveSpeed", speed);
        }

        /**
         * Remove events if this is destroyed
         */
        private void OnDestroy()
        {
            foreach (OWTriggerVolume trigger in eyeTriggers)
            {
                trigger.OnEntry -= EyeHitDetected;
            }
        }
    }
}
