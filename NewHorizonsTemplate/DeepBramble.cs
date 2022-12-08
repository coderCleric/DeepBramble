using OWML.ModHelper;
using OWML.Common;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;

namespace DeepBramble
{
    /**
     * The main class of the mod, everything used in the mod branches from this class.
     */
    public class DeepBramble : ModBehaviour
    {
        //Flags
        private bool fixShipDrift = false;

        //Miscellanious variables
        public INewHorizons NewHorizonsAPI;
        private SignalHelper signalHelper;
        private EntryLocationHelper entryHelper;

        //Only needed for debug
        public static Transform relBody = null;
        public static DeepBramble instance;

        /**
         * Do NH setup stuff and patch certain methods
         */
        private void Start()
        {
            //NH setup stuff
            NewHorizonsAPI = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NewHorizonsAPI.LoadConfigs(this);

            //Do stuff when the system loads
            UnityEvent<string> loadEvent = NewHorizonsAPI.GetStarSystemLoadedEvent();
            loadEvent.AddListener(PrepSystem);

            //Make our helpers
            this.signalHelper = new SignalHelper();
            this.entryHelper = new EntryLocationHelper();

            //Initialize the startup flag dictionary
            Patches.initFlags();

            //Miscellanious patches
            //Do locator-specific startup stuff
            ModHelper.HarmonyHelper.AddPostfix<Locator>(
                "LocateSceneObjects",
                typeof(Patches),
                nameof(Patches.LocatorStartup));

            //Wake up dimensions as we enter them
            ModHelper.HarmonyHelper.AddPrefix<OuterFogWarpVolume>(
                "ReceiveWarpedDetector",
                typeof(Patches),
                nameof(Patches.WakeOnEnter));

            //Hide the text of dree writing
            ModHelper.HarmonyHelper.AddPrefix<NomaiTranslatorProp>(
                "DisplayTextNode",
                typeof(Patches),
                nameof(Patches.HideDreeText));

            //Black Hole Patches
            //Activate the vessel black hole when the warp core is placed in the correct spot
            ModHelper.HarmonyHelper.AddPostfix<WarpCoreSocket>(
                "PlaceIntoSocket",
                typeof(Patches),
                nameof(Patches.WarpPlaceListener));

            //Deactivate the vessel black hole when the warp core is removed from the slot
            ModHelper.HarmonyHelper.AddPostfix<WarpCoreSocket>(
                "RemoveFromSocket",
                typeof(Patches),
                nameof(Patches.WarpRemoveListener));

            //Save and deactivate the black hole in the vessel dimension
            ModHelper.HarmonyHelper.AddPostfix<VanishVolume>(
                "Awake",
                typeof(Patches),
                nameof(Patches.VanishVolumeListener));

            //Patch black holes so that we remove the player ship if they use the hole to get to the bramble system
            ModHelper.HarmonyHelper.AddPostfix<VanishVolume>(
                "OnTriggerEnter",
                typeof(Patches),
                nameof(Patches.ShipRemover));

            //Debug patches (don't include in release)
            //Update the relative body
            ModHelper.HarmonyHelper.AddPrefix<OuterFogWarpVolume>(
                "ReceiveWarpedDetector",
                typeof(Patches),
                nameof(Patches.DimensionUpdater));

            instance = this;
        }

        /**
         * When a star system finishes loading, check if it's the bramble system
         * If it is the bramble system, do a series of setup steps
         * 
         * @param s The string that's the name of the loaded system? I think? Not used in this method.
         */
        private void PrepSystem(String s)
        {
            //Do this stuff if we're in the bramble system
            if (NewHorizonsAPI.GetCurrentStarSystem().Equals("BrambleSystem"))
            {
                //Tell patches that we're in the bramble system
                Patches.inBrambleSystem = true;

                //Do some things to each astro object
                foreach (AstroObject i in Component.FindObjectsOfType<AstroObject>())
                {
                    //If it has a gravity volume, do some stuff to it
                    this.FixPlanet(i.gameObject);

                    //If it's a dimension, do some fixes
                    this.FixDimension(i.gameObject);
                }

                //Fix the parents of all of the signals
                this.signalHelper.PrepDictionary();
                this.signalHelper.FixSignalParents();

                //Fix the fog warps for every ship log entry location
                this.entryHelper.PrepDictionary();
                this.entryHelper.FixEntryOuterWarps();

                //Fix the campfires
                CampFireHelper cfHelper = new CampFireHelper();
                cfHelper.PrepFires();

                //Prime the ship drift fix
                this.fixShipDrift = true;
            }

            //Do other stuff if we're not in the bramble system
            else
            {
                //Tell patches we're not in the bramble system
                Patches.inBrambleSystem = false;

                //Clear the bramble containers
                BrambleContainer.clear();
            }

            //Do this stuff if we're in the hearthian system
            if(NewHorizonsAPI.GetCurrentStarSystem().Equals("SolarSystem"))
            {
                //If the player knows about the vessel, give them the first fact for our mod
                Patches.startupFlags["revealStartingRumor"] = true;
            }
        }

        /**
         * If this object has a child with a gravity field, modify it as a planet
         * 
         * @param body The body to fix
         */
        private void FixPlanet(GameObject body)
        {
            //Check if it has a gravity volume and a child named "Sector"
            GravityVolume volume = body.GetComponentInChildren<GravityVolume>();
            Transform sectorTransform = body.transform.Find("Sector");
            if (volume != null && sectorTransform != null)
            {
                //Increase the priority of the gravity volume
                volume._priority = 2;

                //Remove any heightmap-generated ground
                Transform fakeGround = sectorTransform.Find("CubeSphere");
                if (fakeGround != null)
                    fakeGround.gameObject.SetActive(false);
            }
        }

        /**
         * If this object is a bramble dimension, remove the speed limit
         * 
         * @param body The body to remove the speed limit from
         */
        private void FixDimension(GameObject body)
        {
            //Only do anything if it's a bramble dimension
            if (body.GetComponentInChildren<DarkBrambleRepelVolume>() != null) {

                //Disable lock-on for the dimension body
                body.GetComponent<OWRigidbody>()._isTargetable = false;

                //Disable the thrust and drag limits
                body.GetComponentInChildren<ThrustRuleset>().enabled = false;
                body.GetComponentInChildren<SimpleFluidVolume>()._density = 0;

                //Set up each dimension with the things it needs to grab
                if (body.GetComponent<AstroObject>()._name == AstroObject.Name.CustomString)
                {
                    switch (body.GetComponent<AstroObject>()._customName)
                    {
                        case "Start Dimension":
                            BrambleContainer.containers.Add(new BrambleContainer(body, new string[] { "StartDimensionFlare", "StartDimensionRecorderContainer" }, true));
                            break;
                    }
                }
            }
        }

        /**
         * Do certain things every frame:
         * -Any debug presses
         * -Fix the ship drift
         * -Send the ship far, far away
         */
        private void Update()
        {
            //Fix the ship drift, if possible and necessary
            if(fixShipDrift && Locator.GetShipBody() != null)
            {
                Locator.GetShipBody().SetVelocity(Vector3.zero);
                fixShipDrift = false;
            }

            //Print the player's absolute and relative positions when k is pressed
            if (Keyboard.current[Key.K].wasPressedThisFrame)
            {
                Transform absCenter = null;
                foreach (AstroObject i in Component.FindObjectsOfType<AstroObject>())
                {
                    //Find the center
                    if (i._name == AstroObject.Name.CustomString && i._customName.Equals("The First Dimension"))
                    {
                        absCenter = i.transform;
                    }
                }
                //Finish calculating the absolute position
                GameObject absObject = new GameObject("noname1");
                absObject.transform.position = new Vector3(Locator.GetPlayerCamera().transform.position.x, Locator.GetPlayerCamera().transform.position.y, Locator.GetPlayerCamera().transform.position.z);
                absObject.transform.SetParent(absCenter, true);

                //Finish calculating the relative position
                if (relBody != null) { 
                    GameObject relObject = new GameObject("noname2");
                    relObject.transform.position = new Vector3(Locator.GetPlayerCamera().transform.position.x, Locator.GetPlayerCamera().transform.position.y, Locator.GetPlayerCamera().transform.position.z);
                    relObject.transform.SetParent(relBody, true);
                    debugPrint("Relative position to " + relBody.name + ": " + relObject.transform.localPosition);
                    Destroy(relObject);
                }

                //Print stuff
                debugPrint("Absolute position: " + absObject.transform.localPosition);
            }

            //Lock onto the body that a signal is attached to
            if (Keyboard.current[Key.O].wasPressedThisFrame)
            {
                //Go through each audio signal
                foreach(AudioSignal i in Component.FindObjectsOfType<AudioSignal>())
                {
                    //If it's known and strong enough, try to lock onto it's parent body
                    if(i.GetSignalStrength() == 1 && PlayerData.KnowsSignal(i._name))
                    {
                        //Loop through each parent
                        Transform tf = i.transform;
                        while(tf != null)
                        {
                            OWRigidbody body = tf.gameObject.GetComponent<OWRigidbody>();

                            //If this parent is lockable, lock onto it
                            if(body != null && body.IsTargetable()) {
                                Locator.GetPlayerBody().gameObject.GetComponent<ReferenceFrameTracker>().TargetReferenceFrame(body.GetReferenceFrame());
                                break;
                            }

                            //Otherwise, move another level up
                            tf = tf.parent;
                        }
                    }
                }
            }

            //Tell the distance from the player to the thing they're looking at
            if (Keyboard.current[Key.L].wasPressedThisFrame)
            {
                RaycastHit hit;
                OWCamera cam = Locator.GetPlayerCamera();
                if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit))
                {
                    debugPrint("Distance to object is " + hit.distance);
                }
                else
                    debugPrint("Raycast hit nothing");
            }

            //Teleport to a specific point when n is pressed
            if (Keyboard.current[Key.N].wasPressedThisFrame)
            {
                Vector3 point = new Vector3(10002.1f, 151.1f, -66.3f);
                Transform absCenter = null;
                foreach (AstroObject i in Component.FindObjectsOfType<AstroObject>())
                {
                    //Find the center
                    if (i._name == AstroObject.Name.CustomString && i._customName.Equals("The First Dimension"))
                    {
                        absCenter = i.transform;
                    }
                }

                point = point + absCenter.position;

                Locator._shipBody.SetPosition(point);
            }
        }

        public static void debugPrint(string str)
        {
            instance.ModHelper.Console.WriteLine(str);
        }
    }
}
