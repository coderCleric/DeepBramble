using NewHorizons.Utility.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.Triggers
{
    public class ChimeTrigger : MonoBehaviour
    {
        public int id = 0;

        //Needed components
        private OWTriggerVolume trigger;
        private OWAudioSource audioSource;

        //Event that can be listened to
        public delegate void OnChimeEvent(int id);
        public event OnChimeEvent onChime;

        /**
         * On awake, link to the trigger
         */
        private void Awake()
        {
            trigger = GetComponent<OWTriggerVolume>();
            trigger.OnEntry += Chime;

            audioSource = transform.parent.GetComponentInChildren<OWAudioSource>();
        }

        /**
         * When the player or probe enters, chime
         */
        private void Chime(GameObject other)
        {
            //Check for player or scout
            if (other.CompareTag("PlayerDetector") || other.CompareTag("ProbeDetector"))
            {
                //First and foremost, fire the event
                if (onChime != null)
                    onChime(id);

                //Play the chime audio
                audioSource.Play();
            }
        }

        /**
         * Unlink if destroyed
         */
        private void OnDestroy()
        {
            trigger.OnEntry -= Chime;
        }
    }
}
