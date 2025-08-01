﻿using DeepBramble.BaseInheritors;
using DeepBramble.Ditylum;
using DeepBramble.MiscBehaviours;
using DeepBramble.Triggers;
using UnityEngine;

namespace DeepBramble.Helpers
{
    public static class DBSystemHelper
    {
        public static bool ensureStarterLoad = false;

        /**
         * Do overall fixes for the system
         */
        public static void FixDeepBramble()
        {
            //Override the default system
            if (!ForgottenLocator.vanishShip)
                DeepBramble.instance.NewHorizonsAPI.SetDefaultSystem("DeepBramble");

            //Set the condition for finding the bramble
            PlayerData._currentGameSave.SetPersistentCondition("DeepBrambleFound", true);

            //Fix the parents of all of the signals
            SignalHelper.PrepDictionary();
            SignalHelper.FixSignalParents();

            //Do some things to each astro object
            foreach (AstroObject i in Component.FindObjectsOfType<AstroObject>())
            {
                //If it has a gravity volume, do some stuff to it
                FixPlanet(i.gameObject);

                //If it's a dimension, do some fixes
                FixDimension(i.gameObject);
            }

            //Fix the fog warps for every ship log entry location
            EntryLocationHelper.PrepDictionary();
            EntryLocationHelper.FixEntryOuterWarps();

            //Fix the campfires
            CampFireHelper.PrepFires();

            //Fix all of the nomai text
            foreach(NomaiTextLine line in GameObject.FindObjectsOfType<NomaiTextLine>())
            {
                OWRenderer rend = line.GetComponent<OWRenderer>();
                if(rend != null)
                {
                    rend.GetRenderer().enabled = rend._gameplayActive && rend._lodActive;
                }
            }

            
        }

        /**
         * If this object has a child with a gravity field, modify it as a planet
         * 
         * @param body The body to fix
         */
        private static void FixPlanet(GameObject body)
        {
            //Check if it has a gravity volume and a child named "Sector" to find if it's a custom planet
            GravityVolume volume = body.GetComponentInChildren<GravityVolume>();
            Transform sectorTransform = body.transform.Find("Sector");
            if ((volume != null || body.name.Equals("TheFirstDimension_Body")) && sectorTransform != null)
            {
                DeepBramble.debugPrint("Applying fixes for " + body.name);

                //Increase the priority of the gravity volume
                if(volume != null)
                    volume._priority = 2;

                //Disable the supernova controller (won't be needing it & it messes stuff up)
                sectorTransform.Find("SupernovaController").gameObject.SetActive(false);

                //Do some extra stuff for specific planets
                if (body.GetComponent<AstroObject>().GetAstroObjectName() == AstroObject.Name.CustomString)
                {
                    switch (body.GetComponent<AstroObject>().GetCustomName())
                    {
                        case "The First Dimension":
                            //If needed, vanish the ship
                            if (ForgottenLocator.vanishShip)
                            {
                                DeepBramble.debugPrint("Vanishing the ship");
                                body.transform.Find("ShipSpawnPoint").position = new Vector3(0, 0, -999999f);
                                ForgottenLocator.vanishShip = false;
                            }
                            break;

                        case "Lover's Rock":
                            //Make the broken crystal
                            GravCrystalItem crystal = sectorTransform.Find("large_planetoid/soil_lab_building/crystal").gameObject.AddComponent<GravCrystalItem>();
                            crystal.SetIntact(false);

                            //Update the arrival distance of the signal
                            body.GetComponentInChildren<SignalBody>().GetReferenceFrame()._autopilotArrivalDistance = 250;
                            break;

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

                            //Override the autopilot arrival distance
                            body.GetComponentInChildren<ReferenceFrameVolume>().GetReferenceFrame()._autopilotArrivalDistance = 450;

                            //Update the arrival distance of the signal
                            body.GetComponentInChildren<SignalBody>().GetReferenceFrame()._autopilotArrivalDistance = 450;
                            break;

                        case "Shattered Hearth":
                            //Grab the mat from the tree
                            ForgottenLocator.greenTreeMat = sectorTransform.Find("shattered_planet/park_area/GEO_NomaiTree_1_Trunk").gameObject.GetComponent<MeshRenderer>().material;
                            if (ForgottenLocator.heartDimensionSector != null)
                                ReskinHeartDimension(ForgottenLocator.greenTreeMat, ForgottenLocator.heartDimensionSector);

                            //Set up the gravity triggers for the concert hall
                            sectorTransform.Find("shattered_planet/concert_hall_area/concert_hall_gravity").gameObject.AddComponent<EntranceGravTrigger>();

                            //Set up the chime puzzle
                            sectorTransform.Find("shattered_planet/concert_hall_area/chime_puzzle").gameObject.AddComponent<ChimePuzzleController>();
                            break;

                        case "Magma's Recursion":
                            //Register the hazard with patches
                            ForgottenLocator.hotNodeHazard = sectorTransform.Find("lava_planet/heat_hazard").gameObject.GetComponent<HazardVolume>();

                            //Make all of the cool zones
                            foreach (OWTriggerVolume trig in sectorTransform.gameObject.GetComponentsInChildren<OWTriggerVolume>())
                            {
                                if (trig.name.Contains("coolzone"))
                                {
                                    trig.gameObject.AddComponent<CoolZone>();
                                }
                            }

                            //Complete the cave zone triggers
                            Transform caveTriggerRoot = body.transform.Find("Sector/lava_planet/cave_triggers");
                            Light dimensionLight = GameObject.Find("HotDimension_Body/Sector/Atmosphere/AmbientLight_DB_Interior").GetComponent<Light>();
                            Light planetLight = body.transform.Find("Sector/AmbientLight").GetComponent<Light>();

                            //Make the fade group
                            ForgottenLocator.lavaLightFadeGroup = caveTriggerRoot.gameObject.AddComponent<LightFadeGroup>();
                            ForgottenLocator.lavaLightFadeGroup.AddLight(dimensionLight);
                            ForgottenLocator.lavaLightFadeGroup.AddLight(planetLight);
                            if (ForgottenLocator.heartLightFadeTrigger != null)
                                ForgottenLocator.lavaLightFadeGroup.RegisterTrigger(ForgottenLocator.heartLightFadeTrigger);

                            //Quantum Cave trigger
                            LightFadeTrigger quantumFadeTrigger = caveTriggerRoot.Find("quantumcavetrigger").gameObject.AddComponent<LightFadeTrigger>();
                            ForgottenLocator.lavaLightFadeGroup.RegisterTrigger(quantumFadeTrigger);

                            //Gas Cave trigger
                            LightFadeTrigger gasFadeTrigger = caveTriggerRoot.Find("gascavetrigger").gameObject.AddComponent<LightFadeTrigger>();
                            gasFadeTrigger.fadetime = 2.0f;
                            ForgottenLocator.lavaLightFadeGroup.RegisterTrigger(gasFadeTrigger);
                            LavaDisableTrigger gasLavaDisable = caveTriggerRoot.Find("gascavetrigger").gameObject.AddComponent<LavaDisableTrigger>();
                            gasLavaDisable.RegisterLavaSphere(sectorTransform.Find("MoltenCore").gameObject);

                            //Make the gas hazardous
                            sectorTransform.Find("lava_planet/crystal_cave/explosion_trigger").gameObject.AddComponent<GasVolume>();
                            GasVolume.baseExplosion = sectorTransform.Find("player_explosion").gameObject;

                            //Do stuff to all of the gravity crystals
                            Transform crystalRoot = sectorTransform.Find("lava_planet/crystal_cave/grav_crystals");
                            foreach (Transform socket in crystalRoot)
                            {
                                if (socket.name.Contains("full"))
                                    GravCrystalItem.MakeItem(socket.Find("crystal"));

                                socket.gameObject.AddComponent<GravCrystalSocket>();
                            }

                            //Add the special quantum stuff to the quantum cave
                            Transform quantumCaveRoot = sectorTransform.Find("lava_planet/quantum_cave");

                            //First, the sockets
                            OuterFogWarpVolume hotOuterWarp = GameObject.Find("HotDimension_Body/Sector/OuterWarp").GetComponent<OuterFogWarpVolume>();
                            foreach (Transform socket in quantumCaveRoot)
                            {
                                if (socket.name.Contains("socket"))
                                {
                                    BlockableQuantumSocket sock = socket.gameObject.AddComponent<BlockableQuantumSocket>();
                                    sock.outerFogWarp = hotOuterWarp;
                                }
                            }

                            //Then the rock
                            Transform rockTF = quantumCaveRoot.Find("quantum_rock");
                            ForgottenLocator.quantumRock = rockTF.gameObject.AddComponent<BlockableQuantumObject>();
                            ForgottenLocator.quantumRock._randomYRotation = false;
                            ForgottenLocator.quantumRock.SetSector(null);
                            if (ForgottenLocator.specialSocket != null)
                                ForgottenLocator.quantumRock.specialSocket = ForgottenLocator.specialSocket;

                            //If available, register the signal with the rock
                            if(ForgottenLocator.rockSignal != null)
                                ForgottenLocator.quantumRock.RegisterSignal(ForgottenLocator.rockSignal);

                            //Grab the special things for Ditylum's sequence
                            ForgottenLocator.permaBlockRock = quantumCaveRoot.Find("perma_block_rock").gameObject;
                            ForgottenLocator.permaBlockRock.SetActive(false);
                            ForgottenLocator.permaBlockableSocket = quantumCaveRoot.Find("quantum_socket (1)").GetComponent<BlockableQuantumSocket>();

                            //Set up the swapping text on the scroll
                            ScrollTextSwitcher scrollSwitcher = sectorTransform.Find("quantumTrickScroll").gameObject.AddComponent<ScrollTextSwitcher>();
                            scrollSwitcher.RegisterTrigger(quantumFadeTrigger.gameObject.GetComponent<OWTriggerVolume>());

                            //Make the exit beam toggleable
                            Lever outLever = sectorTransform.Find("lava_planet/entry_zone/tractor_out/lever").gameObject.AddComponent<Lever>();
                            outLever.RegisterBeam(sectorTransform.Find("lava_planet/entry_zone/tractor_out/BeamVolume").gameObject);

                            //Set up the geyser achievement
                            OWTriggerVolume[] geyserTriggers = sectorTransform.Find("lava_planet/geysers").GetComponentsInChildren<OWTriggerVolume>();
                            foreach(OWTriggerVolume trigger in geyserTriggers)
                            {
                                DeepBramble.debugPrint("Doing ach on geyser");
                                trigger.OnExit += DeepBramble.OnGeyserExit;
                            }

                            break;

                        case "Heart Planet":
                            //Make the light fade trigger
                            ForgottenLocator.heartLightFadeTrigger = sectorTransform.Find("final_lab/quantum_room/grav").gameObject.AddComponent<LightFadeTrigger>();
                            if (ForgottenLocator.lavaLightFadeGroup != null)
                                ForgottenLocator.lavaLightFadeGroup.RegisterTrigger(ForgottenLocator.heartLightFadeTrigger);

                            //Activate the doors
                            DoorButtonGroup quantumDoorGroup = DoorButtonGroup.MakeOnDoor(sectorTransform.Find("final_lab/quantum_room/room/walls/functional_doorway").gameObject);
                            quantumDoorGroup.SetLightControl(true);
                            DoorButtonGroup.MakeOnDoor(sectorTransform.Find("final_lab/final_lab_room/room/walls/functional_doorway").gameObject);

                            //Find and activate the quantum socket
                            OuterFogWarpVolume heartFogWarp = GameObject.Find("BramblesHeart_Body/Sector/OuterWarp").GetComponent<OuterFogWarpVolume>();
                            ForgottenLocator.specialSocket = sectorTransform.Find("final_lab/quantum_room/quantum_socket").gameObject.AddComponent<BlockableQuantumSocket>();
                            ForgottenLocator.specialSocket.outerFogWarp = heartFogWarp;
                            Light[] lightArray = new Light[] { quantumDoorGroup.doorLights[0], quantumDoorGroup.doorLights[1] };
                            ForgottenLocator.specialSocket._visibilityObject.SetLightSources(lightArray);
                            if (ForgottenLocator.quantumRock != null)
                                ForgottenLocator.quantumRock.specialSocket = ForgottenLocator.specialSocket;

                            //Hide Ditylum's text
                            ForgottenLocator.griefText = sectorTransform.Find("grief_text").gameObject.GetComponent<NomaiWallText>();
                            ForgottenLocator.griefText.HideTextOnStart();
                            ForgottenLocator.griefText.HideImmediate();

                            //Add the controller to ditylum
                            ForgottenLocator.sadDitylum = sectorTransform.Find("final_lab/final_lab_room/sad_ditylum").gameObject.AddComponent<SadDitylumManager>();

                            //Make the music switch trigger
                            SadMusicSwitch musicSwitch = sectorTransform.Find("final_lab/final_lab_room/sad_music_trigger").gameObject.AddComponent<SadMusicSwitch>();

                            //Give the music switcher the sad music
                            musicSwitch.audioVol = sectorTransform.Find("AmbientAudio").GetComponent<AudioVolume>();

                            break;

                        case "The Venomous Reject":
                            sectorTransform.Find("poison_planet/injector").gameObject.AddComponent<InjectorItem>();
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
        private static void FixDimension(GameObject body)
        {
            //Only do anything if it's a bramble dimension added by a mod
            if (body.GetComponentInChildren<DarkBrambleRepelVolume>() != null && body.transform.Find("Sector") != null)
            {
                DeepBramble.debugPrint("Applying fixes for " + body.name);

                //Disable lock-on for the dimension body
                body.GetComponent<OWRigidbody>()._isTargetable = false;

                //Disable the thrust and drag limits
                body.GetComponentInChildren<ThrustRuleset>().enabled = false;
                body.GetComponentInChildren<SimpleFluidVolume>()._density = 0;

                //Remove the ambient light from the dimension
                if (body.GetComponent<AstroObject>()._customName != "Hot Dimension")
                    body.transform.Find("Sector/Atmosphere/AmbientLight_DB_Interior").gameObject.SetActive(false);

                //Special actions for specific dimensions
                switch (body.GetComponent<AstroObject>()._customName)
                {
                    case "Bramble's Doorstep":
                        ForgottenLocator.startDimensionObject = body;
                        ensureStarterLoad = true;
                        break;

                    case "Briar's Hollow":
                        //Turn off the stacking recursive signals
                        foreach(AudioSignal sig in body.transform.Find("Sector/Loop Node").gameObject.GetComponentsInChildren<AudioSignal>())
                            sig.gameObject.SetActive(false);

                        //Add the swim controller to outer ditylum
                        GameObject.Find("outerditylum").gameObject.AddComponent<SwimmingDitylumManager>();

                        //Make the toxin injector
                        body.transform.Find("Sector/Dilation Node/injector_socket").gameObject.AddComponent<InjectorSocket>();

                        //Grab the dilated signal
                        ForgottenLocator.dilatedSignal = body.transform.Find("Sector/Dilation Node/dilation_signal")
                            .gameObject.GetComponent<AudioSignal>();
                        break;

                    case "The Nursery":
                        //Set up Kevin
                        GameObject kevin = body.transform.Find("Sector/nursery_tube/kevin").gameObject;
                        kevin.AddComponent<KevinBody>();
                        kevin.AddComponent<KevinController>();
                        body.GetComponentInChildren<SimpleFluidVolume>()._density = 8;

                        //Save the drag volume and the outer warp
                        Patches.nurseryDragVol = body.transform.Find("Sector/Volumes/ZeroG_Fluid_Audio_Volume").gameObject.GetComponent<SimpleFluidVolume>();
                        ForgottenLocator.nurseryOuterWarp = body.transform.Find("Sector/OuterWarp").GetComponent<OuterFogWarpVolume>();
                        break;

                    case "Bramble's Heart":
                        ForgottenLocator.heartDimensionSector = body.transform.Find("Sector");
                        if (ForgottenLocator.greenTreeMat != null)
                            ReskinHeartDimension(ForgottenLocator.greenTreeMat, ForgottenLocator.heartDimensionSector);
                        break;

                    case "Bright Hollow":
                        //Make the fish have some brains
                        foreach (Transform fish in body.transform.Find("Sector/observation_lab/fish"))
                        {
                            fish.gameObject.AddComponent<DomesticFishController>();
                        }

                        //Set up the audio switch trigger
                        body.transform.Find("Sector/observation_lab/audio_switcher").gameObject.AddComponent<AudioSwitchTrigger>();
                        body.transform.Find("Sector/observation_lab/audio_switcher").gameObject.SetActive(false);
                        body.transform.Find("Sector/observation_lab/domestic_ambience_calm").gameObject.SetActive(false);
                        break;

                    case "Dilation Dimension":
                        //Register this as the dilation dimension
                        ForgottenLocator.dilationOuterWarp = body.transform.Find("Sector/OuterWarp").GetComponent<OuterFogWarpVolume>();

                        //Add the killer to the dilation node
                        ForgottenLocator.dilationNodeKiller = ForgottenLocator.dilationOuterWarp._linkedInnerWarpVolume.gameObject.AddComponent<NodeKiller>();

                        //Add the rotation controller to dilated Ditylum
                        ForgottenLocator.dilatedDitylum = body.transform.Find("Sector/dilated_ditylum").gameObject.AddComponent<DilatedDitylumManager>();
                        break;

                    case "Dree Dimension":
                        //Suppress the fog override of the hot node
                        body.transform.Find("Sector/Main Hot Node/Effects/FogOverrideVolume").gameObject.SetActive(false);
                        break;

                    case "Hot Dimension":
                        //Grab the original quantum rock signal
                        ForgottenLocator.rockSignal = body.transform.Find("Sector/rock_signal").GetComponent<AudioSignal>();
                        if (ForgottenLocator.quantumRock != null)
                            ForgottenLocator.quantumRock.RegisterSignal(ForgottenLocator.rockSignal);
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
        private static void ReskinHeartDimension(Material mat, Transform sectorRoot)
        {
            //The vines & walls
            Transform geometryRoot = sectorRoot.Find("Geometry/OtherComponentsGroup");
            foreach (MeshRenderer i in geometryRoot.GetComponentsInChildren<MeshRenderer>())
                i.material = mat;

            //The central node
            foreach (MeshRenderer i in sectorRoot.Find("Heart Node/Terrain_DB_BrambleSphere_Inner_v2").gameObject.GetComponentsInChildren<MeshRenderer>())
                i.material = mat;
        }
    }
}
