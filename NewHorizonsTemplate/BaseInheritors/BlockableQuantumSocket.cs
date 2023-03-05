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
        public OuterFogWarpVolume outerFogWarp = null;

        /**
         * Need to do stuff to prevent a null ref in the standard awake
         */
        public new void Awake()
        {
            _occupiedByPlayerVolume = GetComponentInChildren<OWTriggerVolume>();
            _lightSources = new Light[0];
            base.Awake();
            Patches.blockableSockets.Add(this);
        }

        /**
         * Has additional blocking conditions
         */
        public override bool IsOccupied()
        {
            bool baseOccupied = base.IsOccupied();
            return baseOccupied || blockingDrop != null;
        }
    }
}
