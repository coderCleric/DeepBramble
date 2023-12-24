using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.BaseInheritors
{
    public class BlockableQuantumSocket : QuantumSocket
    {
        public OWItem blockingDrop = null;
        private bool scoutIsBlocking = false;
        public OuterFogWarpVolume outerFogWarp = null;
        public bool isPermaBlocked = false;

        /**
         * Need to do stuff to prevent a null ref in the standard awake
         */
        public new void Awake()
        {
            _occupiedByPlayerVolume = GetComponentInChildren<OWTriggerVolume>();
            _occupiedByPlayerVolume.OnEntry += OnScoutEnter;
            _occupiedByPlayerVolume.OnExit+= OnScoutExit;
            _lightSources = new Light[0];
            base.Awake();
            ForgottenLocator.blockableSockets.Add(this);
        }

        /**
         * Mark the socket as blocked when the scout enters
         */
        private void OnScoutEnter(GameObject other)
        {
            if(other.CompareTag("ProbeDetector"))
            {
                scoutIsBlocking = true;
            }
        }

        /**
         * Set the flag when the scout leaves
         */
        private void OnScoutExit(GameObject other)
        {
            if (other.CompareTag("ProbeDetector"))
            {
                scoutIsBlocking = false;
            }
        }

        /**
         * Has additional blocking conditions
         */
        public override bool IsOccupied()
        {
            bool baseOccupied = base.IsOccupied();
            return baseOccupied || blockingDrop != null || scoutIsBlocking || isPermaBlocked;
        }

        /**
         * Tells whether or not the socket is blocked by the scout only
         */
        public bool OccupiedByScoutOnly()
        {
            return scoutIsBlocking && !base.IsOccupied() && blockingDrop == null && !isPermaBlocked;
        }

        /**
         * Remove events when destroyed
         */
        private new void OnDestroy()
        {
            base.OnDestroy();
            _occupiedByPlayerVolume.OnEntry -= OnScoutEnter;
            _occupiedByPlayerVolume.OnExit -= OnScoutExit;
        }
    }
}
