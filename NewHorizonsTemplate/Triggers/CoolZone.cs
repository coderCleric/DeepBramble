using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.Triggers
{
    internal class CoolZone : MonoBehaviour
    {
        private static int insideCount = 0;

        /**
         * Put our events on the trigger volume
         */
        private void Awake()
        {
            OWTriggerVolume trig = gameObject.GetComponent<OWTriggerVolume>();
            trig.OnEntry += DisableHeat;
            trig.OnExit += EnableHeat;
        }

        /**
         * When the player enters, up the count and disable the heat
         * 
         * @param other The entering collider
         */
        private void DisableHeat(GameObject other)
        {
            if (other.GetAttachedOWRigidbody().CompareTag("Player"))
            {
                insideCount++;
                Patches.hotNodeHazard.gameObject.SetActive(false);
            }
        }

        /**
         * When the player leaves, lower the count and enable the heat
         * 
         * @param other The entering collider
         */
        private void EnableHeat(GameObject other)
        {
            if (other.GetAttachedOWRigidbody().CompareTag("Player"))
            {
                insideCount--;
                if(insideCount == 0)
                    Patches.hotNodeHazard.gameObject.SetActive(true);
            }
        }

        /**
         * Remove our events if we're destroyed
         */
        private void OnDestroy()
        {
            OWTriggerVolume trig = gameObject.GetComponent<OWTriggerVolume>();
            trig.OnEntry -= DisableHeat;
            trig.OnExit -= EnableHeat;
        }
    }
}
