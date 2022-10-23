using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble
{
    /**
     * This class just contains all of the patches that are used by the mod
     */
    public static class Patches
    {
        private static GameObject brambleHole = null;
        private static GameObject eyeHologram = null;
        public static bool inBrambleSystem = false;

        /**
         * When the player enters a bramble dimension, wake up the attached rigidbodies
         * 
         * @param detector The warp detector of the object being warped
         * @param __instance The instance of the warp volume the method is being called from
         */
        public static void WakeOnEnter(ref FogWarpDetector detector, OuterFogWarpVolume __instance)
        {
            //Only do stuff if we're in the bramble system
            if (inBrambleSystem)
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

        /**
         * Hide Dree text, unless the player has the translator upgrade
         * 
         * @param __instance The actual translator prop
         * @return True if the text shouldn't be hidden, false otherwise
         */
        public static bool HideDreeText(NomaiTranslatorProp __instance)
        {
            //This flag checks if the targeted text is Dree text
            bool flag = !(__instance._scanBeams[0]._nomaiTextLine == null) && __instance._scanBeams[0]._nomaiTextLine.gameObject.GetComponent<OWRenderer>().sharedMaterial.name == "Effects_IP_Text_mat";

            //If the text is dree, and the player lacks the upgrade, hide the text
            if (flag && inBrambleSystem && !Locator.GetShipLogManager().IsFactRevealed("TRANSLATOR_DREE_UPGRADE"))
            {
                __instance._textField.text = UITextLibrary.GetString(UITextType.TranslatorUntranslatableWarning);
                return false;
            }

            //Otherwise, run normally
            return true;
        }

        /**
         * When the player enters a bramble dimension, set it to be the relative body for the position printout
         * 
         * @param __instance The instance of the warp volume the method is being called from
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
         * 
         * @param __instance The warp core socket instance the method is being called from
         * @param item The item being placed into the socket
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
         * 
         * @param __instance The warp core socket instance the method is being called from
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
         * 
         * @param __instance The vanish volume instance the method is being called from
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

        /**
         * If we detect the player passing through the system black hole, set the flag to remove their ship
         * 
         * @param __instance The instance of the vanish volume
         * @param hitCollider The collider that entered the volume
         */
        public static void ShipRemover(VanishVolume __instance, ref Collider hitCollider)
        {
            if(__instance.transform.parent.gameObject == brambleHole && hitCollider.attachedRigidbody.CompareTag("Player"))
            {
                DeepBramble.removeShip = true;
            }
        }
    }
}
