using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.Triggers
{
    public class ChimeTrigger : MonoBehaviour
    {
        public int id = 0;
        private OWTriggerVolume trigger;

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
