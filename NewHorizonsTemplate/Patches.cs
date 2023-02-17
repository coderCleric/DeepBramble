using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace DeepBramble
{
    /**
     * This class just contains all of the patches that are used by the mod
     */
    [HarmonyPatch]
    public static class Patches
    {
        //Flags
        public static bool inBrambleSystem = false;
        public static bool forbidUnlock = false;

        //Other variables
        private static GameObject brambleHole = null;
        private static GameObject eyeHologram = null;
        private static InnerFogWarpVolume largeLabEntrance = null;
        private static InnerFogWarpVolume smallLabEntrance = null;
        private static OuterFogWarpVolume languageOuterWarp = null;
        public static Dictionary<string, bool> startupFlags = null;

        //Needed for the baby angler
        private static Animator anglerAnimator = null;

        /**
         * Initialize the dictionary of startup flags
         */
        public static void initFlags()
        {
            startupFlags = new Dictionary<string, bool>();
            startupFlags.Add("vanishShip", false);
            startupFlags.Add("revealStartingRumor", false);
        }

        //################################# Miscellanious patches #################################
        /**
         * When the locator finishes loading, do a bunch of stuff to prep the game
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Locator), nameof(Locator.LocateSceneObjects))]
        public static void LocatorStartup()
        {
            DeepBramble.debugPrint("Running locator startup");

            //If needed, vanish the ship
            if (startupFlags["vanishShip"])
            {
                DeepBramble.debugPrint("Vanishing the ship");
                Locator.GetShipBody().SetPosition(new Vector3(0, 0, -999999f));
                startupFlags["vanishShip"] = false;
            }

            //If needed, check if we need to reveal the starting rumor of the mod
            if(startupFlags["revealStartingRumor"])
            {
                ShipLogManager logManager = Locator.GetShipLogManager();
                if (logManager.IsFactRevealed("DB_VESSEL_X1"))
                {
                    DeepBramble.debugPrint("Revealing starting rumor");
                    logManager.RevealFact("WHY_TWO_PODS_RUMOR");
                }
                startupFlags["revealStartingRumor"] = false;
            }

            //If we're in deep bramble, disable the bramble audio player
            if (inBrambleSystem)
                Locator._globalMusicController._darkBrambleSource.gameObject.SetActive(false);
        }

        /**
         * When the player enters a bramble dimension, wake up the attached rigidbodies
         * 
         * @param detector The warp detector of the object being warped
         * @param __instance The instance of the warp volume the method is being called from
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FogWarpVolume), nameof(FogWarpVolume.ReceiveWarpedDetector))]
        public static void WakeOnEnter(ref FogWarpDetector detector, FogWarpVolume __instance)
        {
            //Find the outer warp volume or, if there isn't one, do nothing
            OuterFogWarpVolume outerVolume = null;
            if ((__instance as OuterFogWarpVolume) != null)
                outerVolume = __instance as OuterFogWarpVolume;
            else if (((__instance as InnerFogWarpVolume) != null))
            {
                InnerFogWarpVolume innerVolume = __instance as InnerFogWarpVolume;
                outerVolume = innerVolume.GetContainerWarpVolume();
            }
            else
                return;

            //Only do stuff if we're in the bramble system
            if (inBrambleSystem)
            {
                //Figure out if it's the player that entered
                bool isPlayer = detector.CompareName(FogWarpDetector.Name.Player) || detector.CompareName(FogWarpDetector.Name.Ship) && PlayerState.IsInsideShip();

                //If it is the player, activate the dimension they entered
                GameObject bodyObject = outerVolume.transform.parent.parent.gameObject;
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
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NomaiTranslatorProp), nameof(NomaiTranslatorProp.DisplayTextNode))]
        public static bool HideDreeText(NomaiTranslatorProp __instance)
        {
            //This flag checks if the targeted text is Dree text
            bool flag = __instance._scanBeams[0]._nomaiTextLine != null && __instance._scanBeams[0]._nomaiTextLine.gameObject.GetComponent<OWRenderer>().sharedMaterial.name.Contains("IP");

            //If the text is dree, and the player lacks the upgrade, hide the text
            if (flag && inBrambleSystem && !Locator.GetShipLogManager().IsFactRevealed("TRANSLATOR_DREE_UPGRADE"))
            {
                __instance._textField.text = UITextLibrary.GetString(UITextType.TranslatorUntranslatableWarning);
                return false;
            }

            //Otherwise, run normally
            return true;
        }

        //################################# Language Lab Hotswap Patches #################################
        /**
         * Set up the hotswapper for the two language lab nodes when they wake up
         * 
         * @param __instance The instance of the fog warp volume
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SphericalFogWarpVolume), nameof(SphericalFogWarpVolume.OnAwake))]
        public static void PrimeNodeHotswap(SphericalFogWarpVolume __instance)
        {
            if (inBrambleSystem)
            {
                //If it's the smaller node, just save it
                if (__instance.name.Equals("Language Node"))
                {
                    smallLabEntrance = __instance as InnerFogWarpVolume;
                    DeepBramble.debugPrint("Saving small language node inner warp.");
                }

                //If it's the dimension, just save it
                if (__instance.transform.parent.parent.name.Equals("LanguageDimension_Body"))
                {
                    languageOuterWarp = __instance as OuterFogWarpVolume;
                    DeepBramble.debugPrint("Saving language outer warp.");
                }

                //If it's the larger node, save it & do some cosmetic edits
                if (__instance.name.Equals("Large Language Node"))
                {
                    largeLabEntrance = __instance as InnerFogWarpVolume;
                    Transform nodeEffects = __instance.transform.Find("Effects");
                    nodeEffects.Find("PointLight_DB_FogLight").gameObject.SetActive(false);
                    nodeEffects.Find("DB_BrambleLightShafts").gameObject.SetActive(false);
                    nodeEffects.Find("InnerWarpFogGlow").gameObject.SetActive(false);
                    nodeEffects.Find("FogOverrideVolume").gameObject.SetActive(false);
                    DeepBramble.debugPrint("Saving large language node inner warp.");
                }
            }
        }

        /**
         * Swap the language lab exit depending on the actions of the ship
         * 
         * @param __instance The instance of the fog warp
         * @param body The body that was warped
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SphericalFogWarpVolume), nameof(SphericalFogWarpVolume.RepositionWarpedBody))]
        public static void SwapLanguageExits(SphericalFogWarpExit __instance, OWRigidbody body)
        {
            if(inBrambleSystem && body.CompareTag("Ship"))
            {
                //If the dimension received the ship, swap the exit to the big one
                if(__instance == languageOuterWarp)
                {
                    languageOuterWarp._linkedInnerWarpVolume = largeLabEntrance;
                    DeepBramble.debugPrint("Swapping language entrance to the larger node.");
                }

                //If the big exit received the ship, swap the dimension exit to the small one
                if(__instance == largeLabEntrance)
                {
                    languageOuterWarp._linkedInnerWarpVolume = smallLabEntrance;
                    DeepBramble.debugPrint("Swapping language entrance to the smaller node.");
                }
            }
        }

        //################################# Black hole things #################################
        /**
         * When the player sockets a warp core, check if we need to activate the black hole
         * 
         * @param __instance The warp core socket instance the method is being called from
         * @param item The item being placed into the socket
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(OWItemSocket), nameof(OWItemSocket.PlaceIntoSocket))]
        public static void WarpPlaceListener(OWItemSocket __instance, ref OWItem item)
        {
            //Only do stuff if the slotting was successful and this is the right slot
            if ((__instance as WarpCoreSocket) != null && __instance.AcceptsItem(item) && __instance.name.Equals("BrambleSystemWarpSocket"))
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
        [HarmonyPostfix]
        [HarmonyPatch(typeof(OWItemSocket), nameof(OWItemSocket.RemoveFromSocket))]
        public static void WarpRemoveListener(OWItemSocket __instance)
        {
            if ((__instance as WarpCoreSocket) != null && __instance.name.Equals("BrambleSystemWarpSocket"))
            {
                brambleHole.SetActive(false);
            }
        }

        /**
         * When the vanish volume wakes up, save it if it's the one we want
         * 
         * @param __instance The vanish volume instance the method is being called from
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(VanishVolume), nameof(VanishVolume.Awake))]
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
        [HarmonyPostfix]
        [HarmonyPatch(typeof(VanishVolume), nameof(VanishVolume.OnTriggerEnter))]
        public static void ShipRemover(VanishVolume __instance, ref Collider hitCollider)
        {
            if(__instance.transform.parent.gameObject == brambleHole && hitCollider.attachedRigidbody.CompareTag("Player"))
            {
                startupFlags["vanishShip"] = true;
            }
        }

        //################################# Signal lock patches #################################
        /**
         * Suppress lock-on while the player has their signalscope out
         * 
         * @return True if lock-on should behave normally, false otherwise
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ReferenceFrameTracker), nameof(ReferenceFrameTracker.FindReferenceFrameInLineOfSight))]
        public static bool LockOnSuppressor()
        {
            //Supress normal operation if the signalscope is equipped
            if ((Locator.GetToolModeSwapper()._equippedTool as Signalscope) != null)
            {
                return false;
            }

            //Otherwise, return true
            return true;
        }

        /**
         * Suppress unlocks if they are forbidden
         * 
         * @return True if unlocks are allowed, false otherwise
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ReferenceFrameTracker), nameof(ReferenceFrameTracker.UntargetReferenceFrame), new Type[] { typeof(bool) })]
        public static bool UnlockSuppressor()
        {
            return !forbidUnlock;
        }

        //################################# AudioSignalDetectionTrigger stuff, so the player can pick up signals while in their ship #################################
        /**
         * If the AudioSignalDetectionTrigger asks whether the player is in the ship, say no
         * 
         * @param __result The return value of the original call
         * @return False if the signal is asking, true otherwise
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerState), nameof(PlayerState.IsInsideShip))]
        public static bool ShipLieToSignal(ref bool __result)
        {
            //Get the name of the calling function & class
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace();
            String callerMethodName = trace.GetFrame(2).GetMethod().Name;
            String callerClassName = trace.GetFrame(2).GetMethod().DeclaringType.FullName;

            //Always say we're not in the ship if the signal trigger asks
            if(callerClassName.Contains("AudioSignalDetectionTrigger") && callerMethodName.Contains("Update"))
            {
                __result = false;
                return false;
            }

            //Otherwise, let the actual method run
            return true;
        }

        /**
         * If the AudioSignalDetectionTrigger asks whether the player has their helmet on, say yes
         * 
         * @param __result The return value of the original call
         * @return False if the signal is asking, true otherwise
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerSpacesuit), nameof(PlayerSpacesuit.IsWearingHelmet))]
        public static bool SuitLieToSignal(ref bool __result)
        {
            //Get the name of the calling function & class
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace();
            String callerMethodName = trace.GetFrame(2).GetMethod().Name;
            String callerClassName = trace.GetFrame(2).GetMethod().DeclaringType.FullName;

            //Always say we're not in the ship if the signal trigger asks
            if (callerClassName.Contains("AudioSignalDetectionTrigger") && callerMethodName.Contains("Update"))
            {
                __result = true;
                return false;
            }

            //Otherwise, let the actual method run
            return true;
        }

        //################################# Baby angler stuff #################################
        /**
         * Before the angleraudiocontroller wakes up, see if we need to add a babyfishcontroller
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AnglerfishAudioController), nameof(AnglerfishAudioController.Awake))]
        public static void BabyBrainMaker(AnglerfishAudioController __instance)
        {
            Transform fishTransform = __instance.transform.parent;

            //If it's an adult fish, save a reference to the animator
            if (anglerAnimator == null && fishTransform != null && fishTransform.name.Equals("Anglerfish_Body"))
            {
                anglerAnimator = fishTransform.gameObject.GetComponentInChildren<Animator>(true);
            }

            //If it's a baby fish, prepare it
            else if (fishTransform != null && fishTransform.name.Equals("baby_fish") && fishTransform.GetComponent<BabyFishController>() == null)
            {
                //Copy properties of the saved animator to this one
                Animator babyAnimator = fishTransform.gameObject.GetComponentInChildren<Animator>();
                babyAnimator.runtimeAnimatorController = anglerAnimator.runtimeAnimatorController;
                babyAnimator.avatar = anglerAnimator.avatar;
                babyAnimator.cullingMode = anglerAnimator.cullingMode;

                //Add the controller for the audio controller to find
                BabyFishController.AddBabyController(fishTransform.gameObject);
                BabyFishController babyController = fishTransform.gameObject.GetComponent<BabyFishController>();

                //Manually feed the controller to the anim controller
                AnglerfishAnimController animController = fishTransform.GetComponentInChildren<AnglerfishAnimController>();
                animController._anglerfishController = babyController;

                //Delete the NH animation fixer from existence (Have to find it by string since it's private)
                MonoBehaviour[] comps = animController.gameObject.GetComponents<MonoBehaviour>();
                for(int i = 0; i < comps.Length; i++)
                {
                    if(comps[i].ToString().Contains("NewHorizons.Builder.Props.DetailBuilder/AnglerAnimFixer"))
                    {
                        Component.Destroy(comps[i]);
                        break;
                    }
                }

                //Add the baby biter to the bite trigger
                Transform biteTrigger = fishTransform.Find("BiteTrigger");
                if (biteTrigger != null)
                    biteTrigger.gameObject.AddComponent<BabyBiter>();
            }
        }

        /**
         * Prevent baby fish from entering the investigate state
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AnglerfishController), nameof(AnglerfishController.ChangeState))]
        public static bool PreventBabyInvestigate(AnglerfishController __instance, AnglerfishController.AnglerState newState)
        {
            if (newState == AnglerfishController.AnglerState.Investigating && __instance is BabyFishController)
                return false;
            else
                return true;
        }

        //################################# Debug Things #################################

        /**
         * When the player enters a bramble dimension, set it to be the relative body for the position printout
         * 
         * @param __instance The instance of the warp volume the method is being called from
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FogWarpVolume), nameof(FogWarpVolume.ReceiveWarpedDetector))]
        public static void DimensionUpdater(FogWarpVolume __instance)
        {
            //Find the outer warp volume or, if there isn't one, do nothing
            OuterFogWarpVolume outerVolume = null;
            if ((__instance as OuterFogWarpVolume) != null)
                outerVolume = __instance as OuterFogWarpVolume;
            else if (((__instance as InnerFogWarpVolume) != null))
            {
                InnerFogWarpVolume innerVolume = __instance as InnerFogWarpVolume;
                outerVolume = innerVolume.GetContainerWarpVolume();
            }
            else
                return;

            //Set the dimension to be the relative body
            Transform tf = outerVolume.transform;
            while (tf != null)
            {
                if (tf.gameObject.GetComponent<AstroObject>() != null)
                {
                    DeepBramble.relBody = tf;
                    return;
                }
                tf = tf.parent;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SphericalFogWarpVolume), nameof(SphericalFogWarpVolume.RepositionWarpedBody))]
        public static void PrintUsedPassage(SphericalFogWarpVolume __instance, Vector3 localPos)
        {
            Vector3 localPoint = __instance.transform.TransformPoint(localPos.normalized * __instance._exitRadius);
            SphericalFogWarpExit usedExit = __instance.FindClosestWarpExit(localPoint);
            for(int i = 0; i < __instance._exits.Length; i++)
            {
                if(__instance._exits[i] == usedExit)
                {
                    DeepBramble.debugPrint("Something was received in passage " + i);
                }
            }
        }
    }
}
