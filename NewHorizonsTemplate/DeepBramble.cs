using OWML.ModHelper;
using OWML.Common;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using HarmonyLib;
using UnityEngine.Events;
using System;
using System.Reflection;
using DeepBramble.BaseInheritors;
using DeepBramble.Triggers;
using DeepBramble.MiscBehaviours;
using System.Linq;

namespace DeepBramble
{
    /**
     * The main class of the mod, everything used in the mod branches from this class.
     */
    public class DeepBramble : ModBehaviour
    {
        //Flags
        private bool fixShipDrift = false;
        private bool ensureStarterLoad = false;

        //Miscellanious variables
        public INewHorizons NewHorizonsAPI;
        private SignalHelper signalHelper;
        private EntryLocationHelper entryHelper;
        private DecorHelper decorHelper;
        private GameObject startDimensionObject;
        private AssetBundle titleBundle;
        public static PlayerAudioController playerAudioController = null;

        //Paired things that need each other but may load at different times
        private BlockableQuantumObject quantumRock = null;
        private BlockableQuantumSocket specialSocket = null;

        private LightFadeGroup lavaLightFadeGroup = null;
        private LightFadeTrigger heartLightFadeTrigger = null;

        private Material greenTreeMat = null;
        private Transform heartDimensionSector = null;

        //Only needed for debug
        public static Transform relBody = null;
        public static DeepBramble instance;

        /**
         * Do NH setup stuff and patch certain methods
         */
        private void Start()
        {
            instance = this;

            //NH setup stuff
            NewHorizonsAPI = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NewHorizonsAPI.LoadConfigs(this);

            //Do stuff when the title screen loads
            this.titleBundle = ModHelper.Assets.LoadBundle("assetbundles/titlescreeneffects");
            this.MakeTitleEdits();
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                debugPrint("Detecting scene load");
                if (loadScene == OWScene.TitleScreen)
                    this.MakeTitleEdits();
            };

            //Do stuff when the system starts to load
            UnityEvent<String> startLoadEvent = NewHorizonsAPI.GetChangeStarSystemEvent();
            startLoadEvent.AddListener(UpdateSystemFlag);

            //Do stuff when the system finishes loading
            UnityEvent<string> loadCompleteEvent = NewHorizonsAPI.GetStarSystemLoadedEvent();
            loadCompleteEvent.AddListener(PrepSystem);

            //Make our helpers
            this.signalHelper = new SignalHelper();
            this.entryHelper = new EntryLocationHelper();
            this.decorHelper = new DecorHelper();

            //Initialize the startup flag dictionary
            Patches.initFlags();

            //Make all of the patches
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        /**
         * Updates the inBrambleSystem flag
         * 
         * @param s The name of the loading system
         */
        private void UpdateSystemFlag(String s)
        {
            Patches.inBrambleSystem = s.Equals("DeepBramble");
        }

        /**
         * When a star system finishes loading, check if it's the bramble system
         * If it is the bramble system, do a series of setup steps
         * 
         * @param s The string that's the name of the loaded system? I think? Not used in this method.
         */
        private void PrepSystem(String s)
        {
            //Do this stuff no matter where we are
            Patches.blockableSockets = new List<BlockableQuantumSocket>();
            DomesticFishController.Reset();

            //Do this stuff if we're in the bramble system
            if (NewHorizonsAPI.GetCurrentStarSystem().Equals("DeepBramble"))
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

            //Debug thing, take out
            if (NewHorizonsAPI.GetCurrentStarSystem().Equals("WorkSystem"))
            {
            }
        }

        /**
         * If this object has a child with a gravity field, modify it as a planet
         * 
         * @param body The body to fix
         */
        private void FixPlanet(GameObject body)
        {
            //Check if it has a gravity volume and a child named "Sector" to find if it's a custom planet
            GravityVolume volume = body.GetComponentInChildren<GravityVolume>();
            Transform sectorTransform = body.transform.Find("Sector");
            if (volume != null && sectorTransform != null)
            {
                //Increase the priority of the gravity volume
                volume._priority = 2;

                //Disable the supernova controller (won't be needing it & it messes stuff up)
                sectorTransform.Find("SupernovaController").gameObject.SetActive(false);

                //Do some extra stuff for specific planets
                if(body.GetComponent<AstroObject>().GetAstroObjectName() == AstroObject.Name.CustomString)
                {
                    switch(body.GetComponent<AstroObject>().GetCustomName())
                    {
                        case "Graviton's Folly":
                            //Reverse the gravity since it doesn't work in the config
                            volume._gravitationalMass *= -1;
                            volume._surfaceAcceleration *= -1;

                            //Add the camera inverter
                            body.transform.Find("Sector/hollowplanet/planet/LandingInverseTrigger").gameObject.AddComponent<LandingCamInverter>();

                            //Add the gravity controllers to the pillar fields
                            Transform gravityParent = body.transform.Find("Sector/hollowplanet/planet/pillargravity");
                            gravityParent.Find("Side 1").gameObject.AddComponent<PillarGravityController>();
                            gravityParent.Find("Side 2").gameObject.AddComponent<PillarGravityController>();
                            gravityParent.Find("Side 3").gameObject.AddComponent<PillarGravityController>();
                            gravityParent.Find("Side 4").gameObject.AddComponent<PillarGravityController>();

                            //Make the levers
                            Lever.MakeLevers(sectorTransform.Find("hollowplanet/planet/crystal_core/beams"));

                            //Make the core
                            sectorTransform.Find("hollowplanet/planet/crystal_core/grav_core").gameObject.AddComponent<GravCore>();
                            break;

                        case "Shattered Hearth":
                            //Grab the mat from the tree
                            greenTreeMat = sectorTransform.Find("shattered_planet/park_area/GEO_NomaiTree_1_Trunk").gameObject.GetComponent<MeshRenderer>().material;
                            if(heartDimensionSector != null)
                                ReskinHeartDimension(greenTreeMat, heartDimensionSector);
                            break;

                        case "Magma's Recursion":
                            //Register the hazard with patches
                            Patches.hotNodeHazard = sectorTransform.Find("lava_planet/heat_hazard").gameObject.GetComponent<HazardVolume>();
                            Patches.hotNodeHazard.gameObject.AddComponent<HeatWarningTrigger>();

                            //Make all of the cool zones
                            foreach(OWTriggerVolume trig in sectorTransform.gameObject.GetComponentsInChildren<OWTriggerVolume>())
                            {
                                if(trig.name.Contains("coolzone"))
                                {
                                    trig.gameObject.AddComponent<CoolZone>();
                                }
                            }

                            //Complete the cave zone triggers
                            Transform caveTriggerRoot = body.transform.Find("Sector/lava_planet/cave_triggers");
                            Light dimensionLight = GameObject.Find("HotDimension_Body/Sector/Atmosphere/AmbientLight_DB_Interior").GetComponent<Light>();
                            Light planetLight = body.transform.Find("Sector/AmbientLight").GetComponent<Light>();

                            //Make the fade group
                            lavaLightFadeGroup = caveTriggerRoot.gameObject.AddComponent<LightFadeGroup>();
                            lavaLightFadeGroup.AddLight(dimensionLight);
                            lavaLightFadeGroup.AddLight(planetLight);
                            if (heartLightFadeTrigger != null)
                                lavaLightFadeGroup.RegisterTrigger(heartLightFadeTrigger);

                            //Quantum Cave trigger
                            LightFadeTrigger quantumFadeTrigger = caveTriggerRoot.Find("quantumcavetrigger").gameObject.AddComponent<LightFadeTrigger>();
                            lavaLightFadeGroup.RegisterTrigger(quantumFadeTrigger);

                            //Gas Cave trigger
                            LightFadeTrigger gasFadeTrigger = caveTriggerRoot.Find("gascavetrigger").gameObject.AddComponent<LightFadeTrigger>();
                            gasFadeTrigger.fadetime = 2.0f;
                            lavaLightFadeGroup.RegisterTrigger(gasFadeTrigger);
                            LavaDisableTrigger gasLavaDisable = caveTriggerRoot.Find("gascavetrigger").gameObject.AddComponent<LavaDisableTrigger>();
                            gasLavaDisable.RegisterLavaSphere(sectorTransform.Find("MoltenCore").gameObject);

                            //Make the gas hazardous
                            sectorTransform.Find("lava_planet/crystal_cave/explosion_trigger").gameObject.AddComponent<GasVolume>();
                            GasVolume.baseExplosion = sectorTransform.Find("player_explosion").gameObject;

                            //Do stuff to all of the gravity crystals
                            Transform crystalRoot = sectorTransform.Find("lava_planet/crystal_cave/grav_crystals");
                            foreach(Transform socket in crystalRoot)
                            {
                                if(socket.name.Contains("full"))
                                    GravCrystalItem.MakeItem(socket.Find("crystal"));

                                socket.gameObject.AddComponent<GravCrystalSocket>();
                            }

                            //Add the special quantum stuff to the quantum cave
                            Transform quantumCaveRoot = sectorTransform.Find("lava_planet/quantum_cave");

                            //First, the sockets
                            OuterFogWarpVolume hotOuterWarp = GameObject.Find("HotDimension_Body/Sector/OuterWarp").GetComponent<OuterFogWarpVolume>();
                            foreach(Transform socket in quantumCaveRoot)
                            {
                                if(socket.name.Contains("socket"))
                                {
                                    BlockableQuantumSocket sock = socket.gameObject.AddComponent<BlockableQuantumSocket>();
                                    sock.outerFogWarp = hotOuterWarp;
                                }
                            }

                            //Then the rock
                            Transform rockTF = quantumCaveRoot.Find("quantum_rock");
                            quantumRock = rockTF.gameObject.AddComponent<BlockableQuantumObject>();
                            quantumRock._randomYRotation = false;
                            quantumRock.SetSector(null);
                            if(specialSocket != null)
                                quantumRock.specialSocket = specialSocket;

                            //Set up the swapping text on the scroll
                            ScrollTextSwitcher scrollSwitcher = sectorTransform.Find("quantumTrickScroll").gameObject.AddComponent<ScrollTextSwitcher>();
                            scrollSwitcher.RegisterTrigger(quantumFadeTrigger.gameObject.GetComponent<OWTriggerVolume>());

                            break;

                        case "Heart Planet":
                            //Make the light fade trigger
                            heartLightFadeTrigger = sectorTransform.Find("final_lab/quantum_room/grav").gameObject.AddComponent<LightFadeTrigger>();
                            if (lavaLightFadeGroup != null)
                                lavaLightFadeGroup.RegisterTrigger(heartLightFadeTrigger);

                            //Activate the doors
                            DoorButtonGroup quantumDoorGroup = DoorButtonGroup.MakeOnDoor(sectorTransform.Find("final_lab/quantum_room/room/walls/functional_doorway").gameObject);
                            quantumDoorGroup.SetLightControl(true);
                            DoorButtonGroup.MakeOnDoor(sectorTransform.Find("final_lab/final_lab_room/room/walls/functional_doorway").gameObject);

                            //Find and activate the quantum socket
                            OuterFogWarpVolume heartFogWarp = GameObject.Find("HeartDimension_Body/Sector/OuterWarp").GetComponent<OuterFogWarpVolume>();
                            specialSocket = sectorTransform.Find("final_lab/quantum_room/quantum_socket").gameObject.AddComponent<BlockableQuantumSocket>();
                            specialSocket.outerFogWarp = heartFogWarp;
                            Light[] lightArray = new Light[] {quantumDoorGroup.doorLights[0], quantumDoorGroup.doorLights[1]};
                            specialSocket._visibilityObject.SetLightSources(lightArray);
                            if (quantumRock != null)
                                quantumRock.specialSocket = specialSocket;

                            break;
                    }
                }
            }
        }

        /**
         * If this object is a bramble dimension, remove the speed limit
         * 
         * @param body The body to remove the speed limit from
         */
        private void FixDimension(GameObject body)
        {
            //Only do anything if it's a bramble dimension added by a mod
            if (body.GetComponentInChildren<DarkBrambleRepelVolume>() != null && body.transform.Find("Sector") != null) {

                //Disable lock-on for the dimension body
                body.GetComponent<OWRigidbody>()._isTargetable = false;

                //Disable the thrust and drag limits
                body.GetComponentInChildren<ThrustRuleset>().enabled = false;
                body.GetComponentInChildren<SimpleFluidVolume>()._density = 0;

                //Remove the ambient light from the dimension
                if(body.GetComponent<AstroObject>()._customName != "Hot Dimension")
                    body.transform.Find("Sector/Atmosphere/AmbientLight_DB_Interior").gameObject.SetActive(false);

                //Set up each dimension with the things it needs to grab
                /*
                if (body.GetComponent<AstroObject>()._name == AstroObject.Name.CustomString)
                {
                    switch (body.GetComponent<AstroObject>()._customName)
                    {
                        case "Start Dimension":
                            BrambleContainer.containers.Add(new BrambleContainer(body, new string[] { "StartDimensionFlare", "StartDimensionRecorderContainer" }, true));
                            break;
                        case "Large Dimension":
                            BrambleContainer.containers.Add(new BrambleContainer(body, new string[] { "RecursiveNodeRecorderContainer" }, false));
                            break;
                        case "Dree Dimension":
                            BrambleContainer.containers.Add(new BrambleContainer(body, new string[] { "CommunionRecorderContainer", "ReinvigorationRecorderContainer" }, false));
                            break;
                    }
                }
                */

                //Special actions for specific dimensions
                switch(body.GetComponent<AstroObject>()._customName)
                {
                    case "Start Dimension":
                        this.startDimensionObject = body;
                        this.ensureStarterLoad = true;
                        break;

                    case "The Nursery":
                        GameObject kevin = body.transform.Find("Sector/nursery_tube/kevin").gameObject;
                        kevin.AddComponent<KevinBody>();
                        kevin.AddComponent<KevinController>();
                        body.GetComponentInChildren<SimpleFluidVolume>()._density = 8;
                        break;

                    case "Heart Dimension":
                        heartDimensionSector = body.transform.Find("Sector");
                        if(greenTreeMat != null)
                            ReskinHeartDimension(greenTreeMat, heartDimensionSector);
                        break;

                    case "Domestic Dimension":
                        foreach(Transform fish in body.transform.Find("Sector/observation_lab/fish"))
                        {
                            fish.gameObject.AddComponent<DomesticFishController>();
                        }
                        break;
                }
            }
        }

        /**
         * Re-material the vines in the heart dimension
         * 
         * @param mat The mat to replace with
         * @param sectorRoot the root of the heart dimension's sector
         */
        private void ReskinHeartDimension(Material mat, Transform sectorRoot)
        {
            //The vines & walls
            Transform geometryRoot = sectorRoot.Find("Geometry/OtherComponentsGroup");
            foreach(MeshRenderer i in geometryRoot.GetComponentsInChildren<MeshRenderer>())
                i.material = mat;
        }

        /**
         * Mess with the title screen
         */
        private void MakeTitleEdits()
        {
            debugPrint("Making title edits");

            //Find the background object
            GameObject backgroundObject = GameObject.Find("Scene/Background");
            if(backgroundObject == null)
            {
                debugPrint("Couldn't find background object");
                return;
            }

            //Load the custom effects bundle, make it a child of the background object
            GameObject titleEffectsObject = this.titleBundle.LoadAsset<GameObject>("Assets/Prefabs/titlescreeneffects.prefab");
            if(titleEffectsObject == null)
            {
                debugPrint("Couldn't load title effects object");
                return;
            }
            titleEffectsObject = GameObject.Instantiate(titleEffectsObject, backgroundObject.transform);
            titleEffectsObject.name = "DB Title Effects Object";
            titleEffectsObject.transform.position = new Vector3(116.455f, 368.8177f, -47.0909f);
            titleEffectsObject.transform.rotation = Quaternion.Euler(327.4284f, 1.9997f, 340.8541f);
            debugPrint("Title edits complete");

            //Change the campfire appearance
            CampFireHelper.ChangeFireAppearance(backgroundObject.transform.Find("PlanetPivot/Prefab_HEA_Campfire/Controller_Campfire").GetComponent<Campfire>());

            //Disable the cricket noises
            GameObject.Find("Scene/AudioSource_Ambience").SetActive(false);

            //Find the animator & set it to play at a specific point
            Animator animator = GameObject.Find("Scene").GetComponent<Animator>();
            animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0.6f);
        }

        /**
         * Do certain things every frame:
         * -Any debug presses
         * -Fix the ship drift
         * -Send the ship far, far away
         */
        private void Update()
        {
            //Stop forbidding unlocks this frame
            Patches.forbidUnlock = false;

            //Fix the ship drift, if possible and necessary
            if(fixShipDrift && Locator.GetShipBody() != null)
            {
                Locator.GetShipBody().SetVelocity(Vector3.zero);
                fixShipDrift = false;
            }

            //Ensure that the starting dimension gets rendered
            if(this.ensureStarterLoad && this.startDimensionObject.GetComponent<NewHorizons.Components.Sectored.BrambleSectorController>() != null)
            {
                this.startDimensionObject.GetComponent<NewHorizons.Components.Sectored.BrambleSectorController>().Invoke("EnableRenderers", 0);
                debugPrint("Start dimension renderers manually enabled");
                ensureStarterLoad = false;
            }

            //Lower the damage audio if the player is hurt only by the hot node
            if(playerAudioController != null)
            {
                if (Patches.DamagedByAmbientHeatOnly())
                    playerAudioController._damageAudioSource.SetMaxVolume(0.15f);
                else
                    playerAudioController._damageAudioSource.SetMaxVolume(0.7f);
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
            if (OWInput.IsNewlyPressed(InputLibrary.lockOn))
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
                                Patches.forbidUnlock = true;
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
                //Vector3 point = new Vector3(18.1f, -108.8f, 28770.3f); //Graviton's Folly
                //Vector3 point = new Vector3(9968.0f, -7.1f, -158.7f); //Dree planet
                //Vector3 point = new Vector3(9559.7f, 9920.6f, -99.4f); //Language Dimension
                //Vector3 point = new Vector3(-24.7f, 10043.4f, -244.6f); //Lava planet start
                //Vector3 point = new Vector3(-257.3f, 9950.4f, 39.4f); //Quantum cave
                Vector3 point = new Vector3(85.7f, -3.3f, -9960.4f); //Poison planet
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

                FogWarpDetector shipDetector = Locator.GetShipDetector().GetComponent<FogWarpDetector>();
                if (shipDetector.GetOuterFogWarpVolume() != null) {
                    Patches.fogRepositionHandled = true;
                    shipDetector.GetOuterFogWarpVolume().WarpDetector(shipDetector, GameObject.Find("AntrumoftheCyst_Body/Sector/OuterWarp").GetComponent<OuterFogWarpVolume>());
                }

                Locator._shipBody.SetPosition(point);
            }

            if (Keyboard.current[Key.P].wasPressedThisFrame)
            {
                foreach (ReferenceFrameGUI i in Component.FindObjectsOfType<ReferenceFrameGUI>())
                {
                    i._reticule1.EnableTextReadout(true);
                    i._reticule2.EnableTextReadout(true);
                }
            }
        }

        public static void debugPrint(string str)
        {
            instance.ModHelper.Console.WriteLine(str);
        }
    }
}
