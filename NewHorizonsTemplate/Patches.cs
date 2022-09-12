using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble
{
    public static class Patches
    {
        /**
         * When the player enters a bramble dimension, wake up the attached rigidbodies
         */
        public static void WakeOnEnter(ref FogWarpDetector detector, OuterFogWarpVolume __instance)
        {
            //Figure out if it's the player that entered
            bool isPlayer = detector.CompareName(FogWarpDetector.Name.Player) || detector.CompareName(FogWarpDetector.Name.Ship) && PlayerState.IsInsideShip();

            //If it is the player, activate the dimension they entered
            GameObject bodyObject = __instance.transform.parent.parent.gameObject;
            if (isPlayer)
            {
                BrambleContainer.setActiveDimension(bodyObject);
            }
        }
    }
}
