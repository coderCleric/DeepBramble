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
        private static GameObject brambleHole = null;
        private static GameObject eyeHologram = null;

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

        /**
         * When the player enters a bramble dimension, set it to be the relative body for the position printout
         */
        public static void DimensionUpdater(OuterFogWarpVolume __instance)
        {
            Transform tf = __instance.transform;
            while(tf != null)
            {
                if(tf.gameObject.GetComponent<AstroObject>() != null)
                {
                    DeepBramble.relBody = tf;
                    return;
                }
                tf = tf.parent;
            }
        }

        /**
         * When the player sockets a warp core, check if we need to activate the black hole
         */
        public static void WarpPlaceListener(WarpCoreSocket __instance, ref OWItem item)
        {
            //Only do stuff if the slotting was successful and this is the right slot
            if (__instance.AcceptsItem(item) && __instance.name.Equals("BrambleSystemWarpSocket"))
            {
                //Try to cast the item as a warp core
                WarpCoreItem core = item as WarpCoreItem;

                //If it's a black hole core, activate the singularity
                if (core.GetWarpCoreType() == WarpCoreType.Black)
                {
                    brambleHole.SetActive(true);
                    eyeHologram = GameObject.Find("VesselHologram_EyeSignal");
                    if (eyeHologram != null)
                        eyeHologram.SetActive(false);
                }
            }
        }

        /**
         * When the player removes the warp core, deactivate the black hole
         */
        public static void WarpRemoveListener(WarpCoreSocket __instance)
        {
            if (__instance.name.Equals("BrambleSystemWarpSocket"))
            {
                brambleHole.SetActive(false);
            }
        }

        /**
         * When the vanish volume wakes up, save it if it's the one we want
         */
        public static void VanishVolumeListener(VanishVolume __instance)
        {
            //Figure out if this is the one we want, save it if it is
            if(__instance.transform.parent.parent.name.Equals("Sector_VesselDimension") && brambleHole == null)
            {
                brambleHole = __instance.transform.parent.gameObject;
                brambleHole.SetActive(false);
            }
        }

        public static void printTrace()
        {
            DeepBramble.debugPrint(System.Environment.StackTrace);
        }
    }
}
