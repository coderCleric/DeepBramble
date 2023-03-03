using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.MiscBehaviours
{
    class DoorButtonGroup : MonoBehaviour
    {
        private List<InteractReceiver> buttonReceivers = new List<InteractReceiver>();
        private Animator doorAnimator = null;
        private OWAudioSource audio = null;
        private bool doorOpen = false;
        private bool audioPlaying = false;
        private float audioPlayTime = 0;

        /**
         * Toggle the door's state
         */
        private void ToggleDoor()
        {
            doorOpen = !doorOpen;
            if (doorAnimator != null)
            {
                doorAnimator.SetBool("isOpen", doorOpen);
                audioPlaying = true;
                audioPlayTime = 2;
                audio.Play();
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
            buttonReceivers.Add(button);
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
         * Check whether to continue or stop the audio
         */
        private void Update()
        {
            if(audioPlaying)
            {
                audioPlayTime -= Time.deltaTime;
                if(audioPlayTime <= 0)
                {
                    audioPlaying = false;
                    audio.Stop();
                    audio.PlayOneShot(AudioType.NomaiDoorStop);
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
            DoorButtonGroup buttonGroup = obj.transform.Find("buttons").gameObject.AddComponent<DoorButtonGroup>();

            //Find the door animator
            buttonGroup.doorAnimator = obj.transform.Find("door").GetComponent<Animator>();
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
            foreach (InteractReceiver receiver in buttonGroup.GetComponentsInChildren<InteractReceiver>())
            {
                receiver.gameObject.AddComponent<DoorButton>();
                buttonGroup.RegisterButton(receiver);
            }

            return buttonGroup;
        }
    }
}
