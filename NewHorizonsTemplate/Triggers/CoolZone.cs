using OWML.ModHelper;
using System;
using System.Collections;
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
        public static List<HazardDetector> cooledDetectors = new List<HazardDetector> ();

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
         * When a hazard detector enters, put them in the list
         * 
         * @param other The entering collider
         */
        private void DisableHeat(GameObject other)
        {
            DeepBramble.debugPrint(other.name + " entered the cool zone");

            //Add the detector
            HazardDetector detector = other.GetComponent<HazardDetector>();
            if(detector != null)
                cooledDetectors.Add(detector);
        }

        /**
         * When a hazard detector enters, remove them from the list
         * 
         * @param other The entering collider
         */
        private void EnableHeat(GameObject other)
        {
            DeepBramble.debugPrint(other.name + " left the cool zone");

            //Remove the detector
            HazardDetector detector = other.GetComponent<HazardDetector>();
            if (detector != null)
                cooledDetectors.Remove(detector);
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
