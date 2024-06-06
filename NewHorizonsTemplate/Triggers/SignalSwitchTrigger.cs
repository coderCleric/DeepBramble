using UnityEngine;

namespace DeepBramble.Triggers
{
    public class SignalSwitchTrigger : MonoBehaviour
    {
        public Transform signalTransform;
        public Transform originalTransform;
        public Transform poemTransform;

        /**
         * On awake, set up the triggers
         */
        private void Awake()
        {
            OWTriggerVolume trigger = gameObject.GetComponent<OWTriggerVolume>();
            trigger.OnEntry += OnEnter;
            trigger.OnExit += OnExit;
        }

        /**
         * When the player enters from above, move the signal down
         * 
         * @param other The triggering object
         */
        private void OnEnter(GameObject other)
        {
            if (other.CompareTag("PlayerDetector") && Locator.GetPlayerBody().transform.position.y > transform.position.y)
            {
                signalTransform.position = poemTransform.position;
            }
        }

        /**
         * When the player exits from above, move the signal up
         * 
         * @param other The triggering object
         */
        private void OnExit(GameObject other)
        {
            if (other.CompareTag("PlayerDetector") && Locator.GetPlayerBody().transform.position.y > transform.position.y)
            {
                signalTransform.position = originalTransform.position;
            }
        }
    }
}
