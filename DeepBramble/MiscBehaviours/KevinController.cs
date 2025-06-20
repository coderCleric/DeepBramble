using DeepBramble.Helpers;
using NewHorizons.Handlers;
using UnityEngine;

namespace DeepBramble.MiscBehaviours
{
    public enum KevinState
    {
        HIDDEN, ATSTART, ATEND, MOVING
    }

    public class KevinController : MonoBehaviour
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
        private bool alreadyPet = false;
        private InteractReceiver[] eyeInteractors;

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
            handleReceiver.OnPressInteract += HandleGrab;
            handleReceiver.ChangePrompt(TranslationHandler.GetTranslation("Grab", TranslationHandler.TextType.UI));
            handleReceiver.DisableInteraction();

            //Grab & set up the eye triggers
            eyeTriggers = transform.Find("Beast_Anglerfish/B_angler_root/B_angler_body01/B_angler_body02/eye_triggers").gameObject.GetComponentsInChildren<OWTriggerVolume>();
            foreach(OWTriggerVolume trigger in eyeTriggers)
            {
                trigger.OnEntry += EyeHitDetected;
            }

            //Grab & set up the eye pet interactors
            eyeInteractors = transform.Find("Beast_Anglerfish/B_angler_root/B_angler_body01/B_angler_body02/eye_interacts").gameObject.GetComponentsInChildren<InteractReceiver>();
            foreach (InteractReceiver interactor in eyeInteractors)
            {
                interactor.OnPressInteract += OnPet;
                interactor.ChangePrompt(TranslationHandler.GetTranslation("Pet", TranslationHandler.TextType.UI));
            }

            //Grab the audio source
            longRangeSource = transform.Find("AudioController/OneShotSource_LongRange").gameObject.GetComponent<OWAudioSource>();

            //Register with patches
            ForgottenLocator.registeredKevin = this;
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
         * Handles what happens when the player grabs on
         */
        private void HandleGrab()
        {
            if (state == KevinState.ATSTART)
                MoveToEnd();
            else if(state == KevinState.ATEND)
                MoveToStart();
        }

        /**
         * When the player pets the eye, make a noise to make them feel better
         */
        private void OnPet()
        {
            longRangeSource.pitch = UnityEngine.Random.Range(0.8f, 1f);
            longRangeSource.PlayOneShot(AudioType.DBAnglerfishDetectDisturbance);
            if (!alreadyPet)
            {
                alreadyPet = true;
                AchievementHelper.FishPet();
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
                ForgottenLocator.playerAttachedToKevin = true;

                //Then, get Kevin moving
                travelAnimator.SetTrigger("begin_travel");
            }
        }

        /**
         * Does everything needed for Kevin to start moving from the end to the start. Should be called when the handles are grabbed
         */
        private void MoveToStart()
        {
            if (state == KevinState.ATEND)
            {
                state = KevinState.MOVING;

                //Connect the player and disable the interaction
                attachPoint.AttachPlayer();
                handleReceiver.DisableInteraction();
                Locator.GetPlayerBody().GetComponent<PlayerCharacterController>().EnableZeroGMovement(); //This allows the player to look horizontally, for some reason
                ForgottenLocator.playerAttachedToKevin = true;

                //Then, get Kevin moving
                travelAnimator.SetTrigger("begin_travel_back");
            }
        }

        /**
         * Does everything needed when Kevin reaches the end
         */
        public void ArriveAtEnd()
        {
            state = KevinState.ATEND;
            ForgottenLocator.playerAttachedToKevin = false;
            attachPoint.DetachPlayer();
            Locator.GetPlayerBody().GetComponent<PlayerCharacterController>().DisableZeroGMovement();
            handleReceiver.EnableInteraction();
        }

        /**
         * Does everything needed when Kevin reaches the start
         */
        public void ArriveAtStart()
        {
            //Need to account for if the player rode him here
            if(ForgottenLocator.playerAttachedToKevin)
            {
                ForgottenLocator.playerAttachedToKevin = false;
                attachPoint.DetachPlayer();
                Locator.GetPlayerBody().GetComponent<PlayerCharacterController>().DisableZeroGMovement();
            }

            //These happen regardless
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
            foreach (InteractReceiver interactor in eyeInteractors)
            {
                interactor.OnPressInteract -= OnPet;
            }
        }
    }
}
