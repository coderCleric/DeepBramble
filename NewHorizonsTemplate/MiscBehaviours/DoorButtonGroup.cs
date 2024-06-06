using System.Collections.Generic;
using UnityEngine;

namespace DeepBramble.MiscBehaviours
{
    class DoorButtonGroup : MonoBehaviour
    {
        private List<InteractReceiver> buttonReceivers = new List<InteractReceiver>();
        public List<Light> doorLights = new List<Light>();
        private Animator doorAnimator = null;
        private OWAudioSource audio = null;
        private bool doorOpen = false;
        private bool controlsLights = false;
        public float lightFadeTime = 2;

        /**
         * Sets whether or not the door should control the lights
         * 
         * @param shouldControl True if it should, false otherwise
         */
        public void SetLightControl(bool shouldControl)
        {
            controlsLights = shouldControl;
            foreach(Light light in doorLights)
            {
                if (shouldControl)
                    light.intensity = 0;
                else
                    light.intensity = 1;
            }
        }
        
        /**
         * Toggle the door's state
         */
        private void ToggleDoor()
        {
            doorOpen = !doorOpen;
            if (doorAnimator != null)
            {
                doorAnimator.SetBool("isOpen", doorOpen);
            }
        }

        /**
         * Register an interact receiver as a button
         * 
         * @param The receiver to register
         */
        public void RegisterButton(InteractReceiver button)
        {
            button.OnPressInteract += ToggleDoor;
            buttonReceivers.SafeAdd(button);
        }

        /**
         * Register a light
         * 
         * @param light The light to register
         */
        public void RegisterLight(Light light)
        {
            doorLights.SafeAdd(light);
        }

        /**
         * Remove the event from the buttons if this is destroyed
         */
        private void OnDestroy()
        {
            foreach(InteractReceiver receiver in buttonReceivers)
            {
                if (receiver != null)
                    receiver.OnPressInteract -= ToggleDoor;
            }
        }

        /**
         * Start playing the looping audio
         */
        public void StartAudio()
        {
            audio.Play();
        }

        /**
         * Start playing the looping audio
         */
        public void StopAudio()
        {
            audio.Stop();
            audio.PlayOneShot(AudioType.NomaiDoorStop);
        }

        /**
         * Check whether to continue or stop the audio
         */
        private void Update()
        {
            //Control the lights
            if(controlsLights)
            {
                foreach (Light light in doorLights)
                {
                    if (doorOpen)
                    {
                        light.intensity += Time.deltaTime / lightFadeTime;
                        light.intensity = Mathf.Min(light.intensity, 1);
                    }
                    else
                    {
                        light.intensity -= Time.deltaTime / lightFadeTime;
                        light.intensity = Mathf.Max(light.intensity, 0);
                    }
                }
            }
        }

        /**
         * Makes a button group on the given object (object must have expected architecture)
         * 
         * @param obj The object to make the group on
         * @return The created button group
         */
        public static DoorButtonGroup MakeOnDoor(GameObject obj)
        {
            DoorButtonGroup buttonGroup = obj.AddComponent<DoorButtonGroup>();

            //Find the door animator
            buttonGroup.doorAnimator = obj.GetComponent<Animator>();
            if(buttonGroup.doorAnimator == null)
            {
                DeepBramble.debugPrint("Door animator was not found when looked for!");
                return null;
            }

            //Find the audio source
            buttonGroup.audio = obj.transform.Find("door_audio").gameObject.GetComponent<OWAudioSource>();
            if (buttonGroup.audio == null)
            {
                DeepBramble.debugPrint("Door audio was not found when looked for!");
                return null;
            }

            //Make and store all of the buttons
            foreach (InteractReceiver receiver in obj.transform.Find("buttons").gameObject.GetComponentsInChildren<InteractReceiver>())
            {
                receiver.gameObject.AddComponent<DoorButton>();
                buttonGroup.RegisterButton(receiver);
            }

            //Find and store the lights
            foreach (Light light in obj.transform.Find("Doorway_Lamp").gameObject.GetComponentsInChildren<Light>())
                buttonGroup.RegisterLight(light);

            return buttonGroup;
        }
    }
}
