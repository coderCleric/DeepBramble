using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.Triggers
{
    class LavaDisableTrigger : MonoBehaviour
    {
        private GameObject lavaSphere = null;

        /**
         * Registers the given lava sphere as being under control of this trigger
         * 
         * @param sphere The lava sphere
         */
        public void RegisterLavaSphere(GameObject sphere)
        {
            this.lavaSphere = sphere;
        }

        /**
         * Disable the sphere when the player enters the trigger
         * 
         * @param other The other collider involved
         */
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetAttachedOWRigidbody().CompareTag("Player"))
            {
                lavaSphere.SetActive(false);
            }
        }

        /**
         * Enable the sphere when the player enters the trigger
         * 
         * @param other The other collider involved
         */
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.GetAttachedOWRigidbody().CompareTag("Player"))
            {
                lavaSphere.SetActive(true);
            }
        }
    }
}
