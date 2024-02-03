using DeepBramble.Triggers;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.Helpers
{
    public static class EyeSystemHelper
    {
        public static bool doEyeStuff = false;

        /**
         * Does the fixes for the eye system
         */
        public static void FixEyeSystem()
        {
            //Find the campsite root transform
            Transform campRoot = Component.FindObjectOfType<QuantumCampsiteController>().transform;

            //If Ditylum isn't here, delete our additions and exit early
            if(!PlayerData.GetPersistentCondition("MET_DITYLUM"))
            {
                DeepBramble.debugPrint("Ditylum isn't coming");
                doEyeStuff = false;
                GameObject.Destroy(campRoot.Find("Terrain_Campfire/Terrain_EYE_ForestFloor_Tomb/forest_new_ground").gameObject);
                GameObject.Destroy(campRoot.Find("Campsite/Ditylum").gameObject);
                GameObject.Destroy(campRoot.Find("Campsite/FCAltTravelerSockets").gameObject);
                GameObject.Destroy(campRoot.Find("InstrumentZones/DitylumZone").gameObject);
                GameObject.Destroy(campRoot.parent.Find("Sector_Observatory/Geo_Observatory/ObservatoryPivot/dree_exhibit").gameObject);
                return;
            }

            doEyeStuff = true;

            //Disable the original ground
            campRoot.Find("Terrain_Campfire/Terrain_EYE_ForestFloor_Tomb/ForestOfGalaxies_Center_new").gameObject.SetActive(false);

            //Do stuff for the campsite
            FixCampsite(campRoot);

            //Do stuff for Ditylum
            FixDitylum(campRoot);

            //Do stuff for the zone
            FixZone(campRoot);

            //Do stuff to the inflation controller
            FixInflationController(campRoot);
        }

        /**
         * Fixes things associated with the campsite controller
         * 
         * @param campRoot the root of the campsite
         */
        private static void FixCampsite(Transform campRoot)
        {
            //Give information to the campsite controller
            QuantumCampsiteController campController = campRoot.GetComponent<QuantumCampsiteController>();

            //Ditylum's traveler script
            campController._travelerControllers = AddToArray<TravelerEyeController>(campRoot.Find("Campsite/Ditylum").GetComponent<TravelerEyeController>(), 
                campController._travelerControllers);
            campController._travelerControllers[campController._travelerControllers.Length - 1].OnStartPlaying += campController.OnTravelerStartPlaying;

            //Ditylum's zone
            campController._instrumentZones = AddToArray<GameObject>(campRoot.Find("InstrumentZones/DitylumZone").gameObject,
                campController._instrumentZones);

            //Ditylum's object
            campController._travelerRoots = AddToArray<Transform>(campRoot.Find("Campsite/Ditylum"),
                campController._travelerRoots);

            //The alternate sockets
            //Check to see if someone's missing
            Transform missingTravelerSocket = null;
            int missingCount = 0;
            if (!PlayerData.GetPersistentCondition("MET_SOLANUM"))
            {
                missingTravelerSocket = campRoot.Find("Campsite/AltTravelerSockets/Solanum_Alt");
                missingCount++;
            }
            if (!PlayerData.GetPersistentCondition("MET_PRISONER"))
            {
                missingTravelerSocket = campRoot.Find("Campsite/AltTravelerSockets/Prisoner_Alt");
                missingCount++;
            }

            //If someone's missing, Ditylum will take their place (can just add missing socket to end of alternate list and reposition all)
            if (missingCount == 1)
            {
                //Make new socket list
                campController._altTravelerSockets = AddToArray<Transform>(missingTravelerSocket,
                campController._altTravelerSockets);

                //Do repositioning
                for (int i = 0; i < campController._travelerRoots.Length; i++)
                {
                    campController._travelerRoots[i].SetPositionAndRotation(campController._altTravelerSockets[i].position, campController._altTravelerSockets[i].rotation);
                }
            }

            //If everyone's here, should just need to give the controller the new alternate list
            else if (missingCount == 0)
            {
                int i = 0;
                Transform[] alts = new Transform[campController._altTravelerSockets.Length + 1];
                foreach (Transform sock in campRoot.Find("Campsite/FCAltTravelerSockets"))
                {
                    alts[i] = sock;
                    i++;
                }
                campController._altTravelerSockets = alts;
            }

            //If both are missing do nothing, Ditylum's default position will work
        }

        /**
         * Fixes things relating directly to ditylum
         * 
         * @param campRoot the root of the campsite
         */
        private static void FixDitylum(Transform campRoot)
        {
            //Give Ditylum the stuff that he needs to work
            TravelerEyeController dityController = campRoot.Find("Campsite/Ditylum").GetComponent<TravelerEyeController>();

            //Give him the dialogue
            dityController._dialogueTree = dityController.gameObject.GetComponentInChildren<CharacterDialogueTree>();
            dityController._dialogueTree.OnStartConversation += dityController.OnStartConversation;
            dityController._dialogueTree.OnEndConversation += dityController.OnEndConversation;

            //Give him the signal
            dityController._signal = dityController.gameObject.GetComponentInChildren<AudioSignal>();
            dityController._signal._startActive = false;
            dityController._signal.GetOWAudioSource().playOnAwake = false;
            dityController._signal.SetSector(campRoot.GetComponent<Sector>());
            dityController._signal.SetSignalActivation(false, 0);

            //Finally, turn him off for now
            dityController.gameObject.SetActive(false);
        }

        /**
         * Fixes things relating to ditylum's zone
         * 
         * @param campRoot the root of the campsite
         */
        private static void FixZone(Transform campRoot)
        {
            //Give the quantum instrument the things to enable
            QuantumInstrument instrument = campRoot.Find("InstrumentZones/DitylumZone/poem").GetComponent<QuantumInstrument>();
            instrument._activateObjects[0] = campRoot.Find("Campsite/Ditylum").gameObject;
            instrument._activateObjects[1] = campRoot.Find("Terrain_Campfire/Terrain_EYE_ForestFloor_Tomb/forest_new_ground/actual_ground/ditylum_patch").gameObject;

            //Add the zone trigger to the necessary component
            DitylumZoneTrigger zoneTrigger = campRoot.Find("InstrumentZones/DitylumZone/warp_override_trigger").gameObject.AddComponent<DitylumZoneTrigger>();
            zoneTrigger.warpCylinder = campRoot.Find("Volumes_Campfire/EndlessCylinder_Forest").GetComponent<EndlessCylinder>();

            //Set up the gather logic
            instrument.OnFinishGather += OnFinishGather;

            //Set up the signal
            AudioSignal signal = campRoot.Find("InstrumentZones/DitylumZone").GetComponentInChildren<AudioSignal>();
            signal.SetSector(campRoot.GetComponent<Sector>());
            SignalSwitchTrigger switchTrigger = campRoot.Find("InstrumentZones/DitylumZone/signal_move_trigger").gameObject.AddComponent<SignalSwitchTrigger>();
            switchTrigger.signalTransform = signal.transform;
            switchTrigger.originalTransform = campRoot.Find("InstrumentZones/DitylumZone/signal_start_socket");
            switchTrigger.poemTransform = campRoot.Find("InstrumentZones/DitylumZone/poem");
        }

        /**
         * When the player gathers the instrument, teleport them back and re-enable the teleport field
         */
        private static void OnFinishGather()
        {
            //Teleport the player
            Transform campRoot = Component.FindObjectOfType<QuantumCampsiteController>().transform;
            Transform returnSocket = campRoot.Find("InstrumentZones/DitylumZone/return_socket");
            Locator.GetPlayerBody().SetPosition(returnSocket.position);
            Locator.GetPlayerBody().SetRotation(returnSocket.rotation);
            Locator.GetPlayerBody().SetVelocity(Vector3.zero);

            //Re-enable the distance thing
            campRoot.Find("Volumes_Campfire/EndlessCylinder_Forest").GetComponent<EndlessCylinder>().SetActivation(true);
        }

        /**
         * Fixes the inflation controller
         * 
         * @param campRoot The root of the campsite sector
         */
        private static void FixInflationController(Transform campRoot)
        {
            CosmicInflationController inflator = campRoot.GetComponentInChildren<CosmicInflationController>();

            //First, give it Ditylum's traveler controller
            inflator._travelers = AddToArray<TravelerEyeController>(campRoot.Find("Campsite/Ditylum").GetComponent<TravelerEyeController>(),
                inflator._travelers);
            inflator._travelers[inflator._travelers.Length - 1].OnStartPlaying += inflator.OnTravelerStartPlaying;

            //Next, give it his transform
            inflator._inflationObjects = AddToArray<Transform>(campRoot.Find("Campsite/Ditylum"),
                inflator._inflationObjects);

            //Finally, give it the ground and the patch
            inflator._groundRenderers = AddToArray<OWRenderer>(campRoot.Find("Terrain_Campfire/Terrain_EYE_ForestFloor_Tomb/forest_new_ground/actual_ground")
                .GetComponent<OWRenderer>(), inflator._groundRenderers);
            inflator._groundRenderers = AddToArray<OWRenderer>(campRoot.Find("Terrain_Campfire/Terrain_EYE_ForestFloor_Tomb/forest_new_ground/actual_ground/ditylum_patch")
                .GetComponent<OWRenderer>(), inflator._groundRenderers);
        }

        /**
         * Adds the given element to the given array
         * 
         * @param element The thing to add
         * @param arr The array to add to
         * @return A copy of arr with the added element
         */
        private static T[] AddToArray<T>(T element, T[] arr)
        {
            T[] temp = new T[arr.Length + 1];
            arr.CopyTo(temp, 0);
            temp[temp.Length - 1] = element;
            return temp;
        }
    }
}
