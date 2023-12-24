using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DeepBramble.BaseInheritors;
using DeepBramble.MiscBehaviours;
using HarmonyLib;
using DeepBramble.Ditylum;
using NewHorizons;
using DeepBramble.Triggers;
using DeepBramble.Helpers;

namespace DeepBramble
{
    /**
     * This class just contains all of the patches that are used by the mod
     */
    [HarmonyPatch]
    public static class Patches
    {
        //Flags that are immediatelly used
        public static bool forbidUnlock = false;
        public static bool fogRepositionHandled = false;
        private static bool hideFogEffect = false;

        //Other variables
        public static Vector3 lastCOTUCachedVel = Vector3.zero;

        //Needed for the baby angler & kevin
        public static Animator anglerAnimator = null;

        //Needed for hot node hazard
        private static bool heatNotifPosted = false;
        private static NotificationData heatNotification = new NotificationData(NotificationTarget.Player, "WARNING: EXCESSIVE HEAT DETECTED");

        //################################# Miscellanious patches #################################
        /**
         * When the locator finishes loading, do a bunch of stuff to prep the game
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Locator), nameof(Locator.LocateSceneObjects))]
        public static void LocatorStartup()
        {
            DeepBramble.debugPrint("Running locator startup");

            //Reset a couple of flags
            ForgottenLocator.playerAttachedToKevin = false;
            ForgottenLocator.probeDilated = false;
            heatNotifPosted = false;

            //If needed, vanish the ship
            if (ForgottenLocator.vanishShip)
            {
                DeepBramble.debugPrint("Vanishing the ship");
                Locator.GetShipBody().SetPosition(new Vector3(0, 0, -999999f));
                ForgottenLocator.vanishShip = false;
            }

            //If needed, check if we need to reveal the starting rumor of the mod
            if(ForgottenLocator.revealStartingRumor)
            {
                ShipLogManager logManager = Locator.GetShipLogManager();
                if (logManager.IsFactRevealed("DB_VESSEL_X1"))
                {
                    DeepBramble.debugPrint("Revealing starting rumor");
                    logManager.RevealFact("WHY_TWO_PODS_RUMOR");
                }
                ForgottenLocator.revealStartingRumor = false;
            }

            //If we're in deep bramble, do stuff
            if (ForgottenLocator.inBrambleSystem)
            {
                //Disable bramble music
                Locator._globalMusicController._darkBrambleSource.gameObject.SetActive(false);

                //Stop that weird "ship landed" bug
                foreach(LandingPadSensor sensor in Locator.GetShipBody().GetComponentsInChildren<LandingPadSensor>())
                    sensor._contactBody = null;
            }

            //Give the main class the player damage audio
            ForgottenLocator.playerAudioController = Locator.GetPlayerAudioController();
        }

        /**
         * Brake the player as they come out of the hot dimension
         * 
         * @param detector The warp detector being received
         * @param __instance The calling warp volume
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FogWarpVolume), nameof(FogWarpVolume.ReceiveWarpedDetector))]
        public static void HotNodeBrakes(ref FogWarpDetector detector, FogWarpVolume __instance)
        {
            if (__instance.gameObject.name.Equals("Main Hot Node") && detector.CompareName(FogWarpDetector.Name.Player))
            {
                OWRigidbody playerBody = Locator.GetPlayerBody();
                Vector3 wantedVel = playerBody.GetVelocity().normalized * Mathf.Min(playerBody.GetVelocity().magnitude, 5);
                playerBody.SetVelocity(wantedVel);
            }
        }

        /**
         * Prevent the player from reading text if they're not grounded
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NomaiWallText), nameof(NomaiWallText.CheckAllowFocus))]
        public static bool AirReadCancel(ref bool __result)
        {
            //Don't alter behaviour out of mod
            if (!ForgottenLocator.inBrambleSystem)
                return true;

            //If player is ungrounded and isn't floating, no reading for them
            PlayerCharacterController charController = Locator.GetPlayerBody().GetComponent<PlayerCharacterController>();
            if(!charController.IsGrounded() && charController._isAlignedToForce)
            {
                __result = false;
                return false;
            }

            //By default, run normal stuff
            return true;
        }

        /**
         * Stop the ship from randomly thinking it's on TH
         * 
         * @param __instance The calling sensor
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LandingPadSensor), nameof(LandingPadSensor.GetContactBody))]
        public static void ShipGroundedCorrection(LandingPadSensor __instance)
        {
            if(ForgottenLocator.inBrambleSystem && __instance._contactBody != null && __instance._contactBody.name.Equals("TimberHearth_Body"))
                __instance._contactBody = null;
        }

        /**
         * Listen for when the player finishes reading Ditylum's stuff
         * 
         * @param id The id of the learned fact
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipLogManager), nameof(ShipLogManager.RevealFact))]
        public static bool ListenForReveal(string id)
        {
            //If it's the dummy fact, do things and stop
            if(id.Equals("READ_DITYLUM_STUFF"))
            {
                ForgottenLocator.sadDitylum.EnableSitting();
                return false;
            }

            return true;
        }

        //################################# Slate & Respawning #################################
        /**
         * Slate needs dialogue conditions set up properly at the start of the loop
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(DialogueConditionManager), nameof(DialogueConditionManager.ReadPlayerData))]
        public static void PrepSlateDialogue(DialogueConditionManager __instance)
        {
            if (PlayerData._currentGameSave.GetPersistentCondition("DeepBrambleFound"))
                __instance.SetConditionState("ArgonDeepBrambleFound", true);
            if (PlayerData._currentGameSave.GetPersistentCondition("LockableSignalFound"))
                __instance.SetConditionState("ArgonLockableSignalFound", true);
        }

        /**
         * If the player is finding certain signals for the first time, set them to go back and tell Slate to yap
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(AudioSignal), nameof(AudioSignal.IdentifySignal))]
        public static void TriggerSignalHint(AudioSignal __instance) 
        {
            if (!PlayerData._currentGameSave.GetPersistentCondition("LockableSignalFound") && 
                (__instance.name.Equals("Camp_Marker_Signal") || __instance.name.Equals("Gravitation_Anomaly_Signal")))
            {
                DeepBramble.debugPrint("default system about to change");
                PlayerData._currentGameSave.SetPersistentCondition("LockableSignalFound", true);
                DeepBramble.instance.NewHorizonsAPI.SetDefaultSystem("SolarSystem");
                DeepBramble.debugPrint("default system should be changed");
            }
        }

        //################################# Do funky time dilation node stuff #################################
        /**
         * If the player enters the dilation dimension, kill them. If the probe enters, set the lock
         * 
         * @param detector The warp detector being received
         * @param __instance The calling warp volume
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FogWarpVolume), nameof(FogWarpVolume.ReceiveWarpedDetector))]
        public static void DilationEntryManager(ref FogWarpDetector detector, FogWarpVolume __instance)
        {
            //Only do anything if this is the dilation dimension
            OuterFogWarpVolume outerWarp = __instance as OuterFogWarpVolume;
            if(outerWarp != null && outerWarp == ForgottenLocator.dilationOuterWarp)
            {
                //If it's the player, kill them
                if (detector.CompareName(FogWarpDetector.Name.Player) || (detector.CompareName(FogWarpDetector.Name.Ship) && PlayerState.IsInsideShip()))
                {
                    Locator.GetDeathManager().KillPlayer(DeathType.TimeLoop);
                    OWRigidbody playerBody = Locator.GetPlayerBody();
                    Vector3 wantedVel = playerBody.GetVelocity().normalized * 3;
                    playerBody.SetVelocity(wantedVel);
                    ForgottenLocator.dilatedDitylum.LookAtPlayer();
                }

                //If it's the probe, engage the lock
                else if (detector.CompareTag("ProbeDetector"))
                {
                    ForgottenLocator.probeDilated = true;
                }
            }
        }

        /**
         * Make interference if the probe is in the dilation dimension
         * 
         * @param __result Whether or not the probe should be interfered with
         * @return Suppress the original if the probe is locked by the dilation dimension
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProbeCamera), nameof(ProbeCamera.HasInterference))]
        public static bool DilationInterference(ref bool __result)
        {
            //Mark true and suppress if the probe is locked in
            if(ForgottenLocator.probeDilated)
            {
                __result = true;
                return false;
            }

            return true;
        }

        /**
         * Delay the recall if the probe is locked
         * 
         * @param forcedRetrieval Whether or not the retrieval was forced
         * @return If the probe is locked and isn't being forced, stop it from recalling
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.RetrieveProbe))]
        public static bool DelayRetrieve(bool playEffects, bool forcedRetrieval, ProbeLauncher __instance)
        {
            //Request the recall if it's locked in
            if(!forcedRetrieval && ForgottenLocator.probeDilated)
            {
                if (DeepBramble.recallTimer == -999)
                    DeepBramble.recallTimer = 3;
                NotificationData data = new NotificationData(NotificationTarget.All, "RECALL REQUEST UNACKNOWLEDGED");
                NotificationManager.SharedInstance.PostNotification(data);
                return false;
            }

            //Play the recall effects manually if it was forced but still locked
            else if(DeepBramble.recallTimer > -999 && !playEffects && 
                Locator.GetToolModeSwapper().GetProbeLauncher() == __instance && Locator.GetToolModeSwapper().GetToolMode() == ToolMode.Probe)
            {
                __instance._effects.PlayRetrievalClip();
                __instance._probeRetrievalEffect.WarpObjectIn(__instance._probeRetrievalLength);
            }

            ForgottenLocator.probeDilated = false; //If it's recalled, it's no longer dilated
            return true;
        }

        //################################# Suppress hot node damage effects #################################
        /**
         * Helper function, checks if the player is in only the hot node hazard volume
         * 
         * @return True if the player is only being damaged by the hot node, false otherwise
         */
        public static bool DamagedByAmbientHeatOnly()
        {
            if(ForgottenLocator.hotNodeHazard == null)
                return false;

            HazardDetector playerDetector = Locator.GetPlayerDetector().GetComponent<HazardDetector>();
            bool otherFound = false;
            bool nodeHazFound = false;
            foreach(EffectVolume i in playerDetector._activeVolumes)
            {
                HazardVolume hazVolume = i as HazardVolume;
                if (hazVolume == ForgottenLocator.hotNodeHazard)
                    nodeHazFound = true;
                else
                    otherFound = true;
            }

            return !otherFound && nodeHazFound;
        }

        /**
         * Disable the phosphenes if they're only made by the node hazard
         * 
         * @return False if the effects should be suppressed, true otherwise
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerCameraEffectController), nameof(PlayerCameraEffectController.ApplyExposureDamage))]
        public static bool SuppressPhosphenes()
        {
            if(DamagedByAmbientHeatOnly())
            {
                return false;
            }
            return true;
        }

        /**
         * Cool the detector if it's in a coolzone
         * 
         * @param __instance The detector being checked
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HazardDetector), nameof(HazardDetector.GetNetDamagePerSecond))]
        public static void CoolDetector(HazardDetector __instance, ref float __result)
        {
            //Set some flags
            bool inCoolZone = CoolZone.cooledDetectors.Contains(__instance);
            bool isPlayerDetector = __instance.CompareTag("PlayerDetector");
            bool inSpecialHazard = __instance._activeVolumes.Contains(ForgottenLocator.hotNodeHazard as EffectVolume);

            //Make the cooling work
            if (inCoolZone)
                __result = Mathf.Max(0, __result - ForgottenLocator.hotNodeHazard._damagePerSecond);

            //Do the notification if needed
            else if(!heatNotifPosted && isPlayerDetector && inSpecialHazard)
            {
                heatNotifPosted = true;
                NotificationManager.SharedInstance.PostNotification(heatNotification, true);
            }

            //Unpost the notif when needed
            if(heatNotifPosted && (inCoolZone || !inSpecialHazard))
            {
                heatNotifPosted = false;
                NotificationManager.SharedInstance.UnpinNotification(heatNotification);
            }
        }

        //################################# Dree text stuff #################################
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
            if (flag && ForgottenLocator.inBrambleSystem && !Locator.GetShipLogManager().IsFactRevealed("TRANSLATOR_UPGRADE_FACT_FC"))
            {
                __instance._textField.text = UITextLibrary.GetString(UITextType.TranslatorUntranslatableWarning);
                return false;
            }

            //Otherwise, run normally
            return true;
        }

        /**
         * Recolor all of the Dree text
         * 
         * @param __instance The instance of text being investigated
         * @param state The state of the text
         * @param __result The returned color
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NomaiTextLine), nameof(NomaiTextLine.DetermineTextLineColor))]
        public static void RecolorDreeText(NomaiTextLine __instance, NomaiTextLine.VisualState state, ref Color __result)
        {
            //Only recolor if it's active, in the bramble system, and is Dree
            if(ForgottenLocator.inBrambleSystem && __instance._active && __instance.gameObject.GetComponent<OWRenderer>().sharedMaterial.name.Contains("IP"))
            {
                switch(state)
                {
                    case NomaiTextLine.VisualState.UNREAD:
                        __result = new Color(0.25f, 0.3f, 0.25f, 7f);
                        break;
                    case NomaiTextLine.VisualState.TRANSLATED:
                        __result = new Color(0.25f, 0.3f, 0.25f, 1f);
                        break;
                }
            }
        }

        //################################# Kevin, my hated #################################
        /**
         * Fixes a bug where attach points are able to carry the player away from the world origin
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerAttachPoint), nameof(PlayerAttachPoint.UpdatePlayerAttach))]
        public static void FixAttachBug()
        {
            GlobalMessenger.FireEvent("PlayerRepositioned");
        }

        /**
         * Stop the center of the universe from flying away if the player is attached to a moving Kevin
         * 
         * @param __instance The center of the universe
         * @return False if we should override, true otherwise
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CenterOfTheUniverse), nameof(CenterOfTheUniverse.FixedUpdate))]
        public static bool FixCOTUSpeedup(CenterOfTheUniverse __instance)
        {
            if (ForgottenLocator.playerAttachedToKevin)
            {
                Vector3 newCachedVel = (__instance.enabled ? (-__instance._centerBody.GetRigidbody().velocity) : Vector3.zero); //Get vel directly from player's rigidbody
                Vector3 origCachedVel = newCachedVel;

                //Calculate what it should end up being
                newCachedVel = newCachedVel - lastCOTUCachedVel;
                lastCOTUCachedVel = origCachedVel;
                __instance._cachedOffsetVelocity = newCachedVel;
                return false;
            }
            else
                return true;
        }

        /**
         * Prevent other fish from hearing the player if they're attached to Kevin
         * 
         * @return False if the player is attached to kevin, true otherwise
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AnglerfishController), nameof(AnglerfishController.OnClosestAudibleNoise))]
        public static bool MufflePlayer()
        {
            return !ForgottenLocator.playerAttachedToKevin;
        }

        /**
         * Warp Kevin back to the start when the player leaves the nursery
         * 
         * @param __instance The fog warp volume the player is leaving
         * @param detector The detector being warped
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FogWarpVolume), nameof(FogWarpVolume.WarpDetector))]
        public static void ResetKevin(FogWarpVolume __instance, FogWarpDetector detector)
        {
            if (ForgottenLocator.inBrambleSystem && (detector.CompareName(FogWarpDetector.Name.Player) || (detector.CompareName(FogWarpDetector.Name.Ship) && PlayerState.IsInsideShip())))
            {
                ForgottenLocator.registeredKevin.TeleportBack();
            }
        }

        //################################# Between dimension teleportation #################################
        /**
         * Ignores the request to reposition a body if we have been told to do so
         * 
         * @param body The body being warped
         * @return False if it should interrupt, and true otherwise
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SphericalFogWarpVolume), nameof(SphericalFogWarpVolume.RepositionWarpedBody))]
        public static bool InterruptFogReposition(OWRigidbody body)
        {
            if (fogRepositionHandled)
            {
                fogRepositionHandled = false;
                hideFogEffect = body.CompareTag("Player");
                DeepBramble.debugPrint("Blocked a fog warp reposition");
                return false;
            }
            else
                return true;
        }

        /**
         * Hide the visor fog effect when the player uses direct teleportation
         * 
         * @param __instance The calling instance
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerFogWarpDetector), nameof(PlayerFogWarpDetector.OnFogWarp))]
        public static void HideThickFog(PlayerFogWarpDetector __instance)
        {
            if(hideFogEffect)
            {
                hideFogEffect = false;
                __instance._fogFraction = 0;
            }
        }

        /**
         * When the object moves, check if we need to handle the probe warp
         * 
         * @param __instance The instance of the object that is calling
         * @socket The socket to move to
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SocketedQuantumObject), nameof(SocketedQuantumObject.MoveToSocket))]
        public static void HandleProbeEntanglement(SocketedQuantumObject __instance, QuantumSocket socket)
        {
            //Only do this for our special rocks
            BlockableQuantumObject obj = __instance as BlockableQuantumObject;
            BlockableQuantumSocket targetSocket = socket as BlockableQuantumSocket;
            if(obj != null && targetSocket != null)
            {
                //Then, only do this if the probe is anchored and is both in and going to a bramble dimension
                FogWarpDetector probeDetector = obj.gameObject.GetComponentInChildren<FogWarpDetector>();
                if(probeDetector != null && probeDetector.GetOuterFogWarpVolume() != null && targetSocket.outerFogWarp != null)
                {
                    fogRepositionHandled = true;
                    probeDetector.GetOuterFogWarpVolume().WarpDetector(probeDetector, targetSocket.outerFogWarp);
                }
            }
        }

        //################################# Dropping object blocking logic #################################
        /**
         * When an item is placed down, check if it's in-bounds of any of the saved blockable sockets
         * 
         * @param position The position the object was placed at
         * @param __instance The instance of the item
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(OWItem), nameof(OWItem.DropItem))]
        public static void BlockOnDrop(Vector3 position, OWItem __instance)
        {
            DeepBramble.debugPrint(position.ToString());
            foreach(BlockableQuantumSocket i in ForgottenLocator.blockableSockets)
            {
                if (i._occupiedByPlayerVolume != null && i._occupiedByPlayerVolume.GetOWCollider().GetCollider().bounds.Contains(position))
                {
                    i.blockingDrop = __instance;
                }
            }
        }

        /**
         * When an item is picked up, unblock any sockets it was blocking
         * 
         * @param __instance The instance of the item
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(OWItem), nameof(OWItem.PickUpItem))]
        public static void UnblockOnPickup(OWItem __instance)
        {
            foreach(BlockableQuantumSocket i in ForgottenLocator.blockableSockets)
            {
                if (i.blockingDrop == __instance)
                    i.blockingDrop = null;
            }
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
            if (ForgottenLocator.inBrambleSystem)
            {
                //If it's the smaller node, just save it
                if (__instance.name.Equals("Language Node"))
                {
                    ForgottenLocator.smallLabEntrance = __instance as InnerFogWarpVolume;
                    DeepBramble.debugPrint("Saving small language node inner warp.");
                }

                //If it's the dimension, just save it
                if (__instance.transform.parent.parent.name.Equals("LanguageDimension_Body"))
                {
                    ForgottenLocator.languageOuterWarp = __instance as OuterFogWarpVolume;
                    DeepBramble.debugPrint("Saving language outer warp.");
                }

                //If it's the larger node, save it & do some cosmetic edits
                if (__instance.name.Equals("Large Language Node"))
                {
                    ForgottenLocator.largeLabEntrance = __instance as InnerFogWarpVolume;
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
            if(ForgottenLocator.inBrambleSystem && body.CompareTag("Ship"))
            {
                //If the dimension received the ship, swap the exit to the big one
                if(__instance == ForgottenLocator.languageOuterWarp)
                {
                    ForgottenLocator.languageOuterWarp._linkedInnerWarpVolume = ForgottenLocator.largeLabEntrance;
                    DeepBramble.debugPrint("Swapping language entrance to the larger node.");
                }

                //If the big exit received the ship, swap the dimension exit to the small one
                if(__instance == ForgottenLocator.largeLabEntrance)
                {
                    ForgottenLocator.languageOuterWarp._linkedInnerWarpVolume = ForgottenLocator.smallLabEntrance;
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
            if (__instance is WarpCoreSocket && __instance.AcceptsItem(item) && __instance.name.Equals("BrambleSystemWarpSocket"))
            {
                //Try to cast the item as a warp core
                WarpCoreItem core = item as WarpCoreItem;

                //If it's a black hole core, activate the singularity
                if (core.GetWarpCoreType() == WarpCoreType.Black)
                {
                    ForgottenLocator.brambleSingularity.SetActive(true);
                    ForgottenLocator.eyeHologram = GameObject.Find("VesselHologram_EyeSignal");
                    if (ForgottenLocator.eyeHologram != null)
                        ForgottenLocator.eyeHologram.SetActive(false);
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
            if (__instance is WarpCoreSocket && __instance.name.Equals("BrambleSystemWarpSocket"))
            {
                ForgottenLocator.brambleSingularity.SetActive(false);
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
            if(__instance.transform.parent.parent.name.Equals("Sector_VesselDimension") && ForgottenLocator.brambleSingularity == null)
            {
                ForgottenLocator.brambleSingularity = __instance.transform.parent.gameObject;
                ForgottenLocator.brambleSingularity.SetActive(false);
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
            if(__instance.transform.parent.gameObject == ForgottenLocator.brambleSingularity && hitCollider.attachedRigidbody.CompareTag("Player"))
            {
                ForgottenLocator.vanishShip = true;
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
                    biteTrigger.gameObject.AddComponent<Triggers.BabyBiter>();
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

        //################################# Eye Things #################################
        /**
         * When other instrument zone's are activated, also activate Ditylum's
         * 
         * @param __instance The calling campsite controller
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuantumCampsiteController), nameof(QuantumCampsiteController.ActivateRemainingInstrumentZones))]
        public static void DityZoneFix(QuantumCampsiteController __instance)
        {
            if(EyeSystemHelper.doEyeStuff)
            {
                __instance._instrumentZones[6].SetActive(true);
                __instance.transform.Find("Terrain_Campfire/Terrain_EYE_ForestFloor_Tomb/forest_new_ground/actual_ground/ditylum_patch")
                    .gameObject.SetActive(false); //Need to also deactivate the hole cover
            }
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
            if (__instance is OuterFogWarpVolume)
                outerVolume = __instance as OuterFogWarpVolume;
            else if (__instance is InnerFogWarpVolume)
            {
                InnerFogWarpVolume innerVolume = __instance as InnerFogWarpVolume;
                outerVolume = innerVolume.GetContainerWarpVolume();
            }
            else
                return;

            //Don't break things if there was no container
            if(outerVolume == null)
            {
                DeepBramble.relBody = null;
                return;
            }

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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DestructionVolume), nameof(DestructionVolume.Vanish), new Type[] { typeof(OWRigidbody), typeof(RelativeLocationData) })]
        public static void PrintVanishedThing(DestructionVolume __instance, OWRigidbody bodyToVanish)
        {
            DeepBramble.debugPrint("Object " + bodyToVanish.gameObject.name + " was vanished by " + __instance.gameObject.name);
        }
    }
}
