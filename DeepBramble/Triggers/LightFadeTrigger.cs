using UnityEngine;


namespace DeepBramble.Triggers
{
    public class LightFadeTrigger : MonoBehaviour
    {
        //Variables
        public float fadetime = 0.5f;
        public LightFadeGroup fadeGroup = null;

        /**
         * Tell the group when the player enters the trigger
         * 
         * @param other The entering collider
         */
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("PlayerDetector"))
            {
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
            if (other.CompareTag("PlayerDetector"))
            {
                if (fadeGroup != null)
                    fadeGroup.OnTriggerDeactivate(this);
            }
        }
    }
}
