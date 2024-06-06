using UnityEngine;

namespace DeepBramble.Triggers
{
    public class EntranceGravTrigger : MonoBehaviour
    {
        private GameObject gravObject = null;
        private OWTriggerVolume outerTrigger = null;
        private OWTriggerVolume innerTrigger = null;
        private bool inInner = false;

        /**
         * On awake, grab necessary components
         */
        private void Awake()
        {
            //Grab the objects
            gravObject = transform.Find("actual_gravity").gameObject;
            outerTrigger = transform.Find("grav_outer_trigger").gameObject.GetComponent<OWTriggerVolume>();
            innerTrigger = transform.Find("grav_inner_trigger").gameObject.GetComponent<OWTriggerVolume>();

            //Link to the triggers
            outerTrigger.OnEntry += OnEnterOuter;
            outerTrigger.OnExit += OnExitOuter;
            innerTrigger.OnEntry += OnEnterInner;
            innerTrigger.OnExit += OnExitInner;
        }

        /**
         * On start, disable the gravity object
         */
        private void Start()
        {
            gravObject.SetActive(false);
        }

        /**
         * Handles entering the outer trigger
         */
        private void OnEnterOuter(GameObject other)
        {
            if(other.CompareTag("PlayerDetector"))
            {
                gravObject.SetActive(true);
            }
        }

        /**
         * Handles exiting the outer trigger
         */
        private void OnExitOuter(GameObject other)
        {
            if (other.CompareTag("PlayerDetector"))
            {
                if(!inInner)
                    gravObject.SetActive(false);
            }
        }

        /**
         * Handles entering the inner trigger
         */
        private void OnEnterInner(GameObject other)
        {
            if (other.CompareTag("PlayerDetector"))
            {
                inInner = true;
            }
        }

        /**
         * Handles exiting the inner trigger
         */
        private void OnExitInner(GameObject other)
        {
            if (other.CompareTag("PlayerDetector"))
            {
                inInner = false;
            }
        }

        /**
         * If destroyed, unlink
         */
        private void OnDestroy()
        {
            outerTrigger.OnEntry -= OnEnterOuter;
            outerTrigger.OnExit -= OnExitOuter;
            innerTrigger.OnEntry -= OnEnterInner;
            innerTrigger.OnExit -= OnExitInner;
        }
    }
}
