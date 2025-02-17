using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.Triggers
{
    public class QuantumBlockTrigger : MonoBehaviour
    {
        QuantumObject quantumObject = null;
        OWTriggerVolume trigger = null;

        /**
         * On awake, grab components and do setup
         */
        private void Awake()
        {
            quantumObject = GetComponentInParent<QuantumObject>();
            trigger = GetComponent<OWTriggerVolume>();
            trigger.OnEntry += OnEntry;
            trigger.OnExit += OnExit;
        }

        /**
         * When the player enters, disable the quantum nature
         */
        private void OnEntry(GameObject other)
        {
            if (other.CompareTag("PlayerDetector"))
            {
                quantumObject.SetIsQuantum(isQuantum: false);
            }
        }

        /**
         * When the player leaves, enable the quantum nature
         */
        private void OnExit(GameObject other)
        {
            if (other.CompareTag("PlayerDetector"))
            {
                quantumObject.SetIsQuantum(isQuantum: true);
            }
        }

        /**
         * Make sure nothing explodes if we're destroyed
         */
        private void OnDestroy()
        {
            trigger.OnEntry -= OnEntry;
            trigger.OnExit -= OnExit;
        }
    }
}
