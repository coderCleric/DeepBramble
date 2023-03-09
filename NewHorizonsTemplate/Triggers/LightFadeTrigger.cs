using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace DeepBramble.Triggers
{
    class LightFadeTrigger : MonoBehaviour
    {
        //Variables
        public float fadetime = 0.5f;
        public LightFadeGroup fadeGroup = null;
        public bool active = false;

        /**
         * Tell the group when the player enters the trigger
         * 
         * @param other The entering collider
         */
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetAttachedOWRigidbody().CompareTag("Player") && !active)
            {
                active = true;
                if (fadeGroup != null)
                    fadeGroup.OnTriggerActivate(this);
            }
        }

        /**
         * Tell the group when the player leaves the trigger
         * 
         * @param other The entering collider
         */
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.GetAttachedOWRigidbody().CompareTag("Player") && active)
            {
                active = false;
                if (fadeGroup != null)
                    fadeGroup.OnTriggerDeactivate(this);
            }
        }
    }
}
