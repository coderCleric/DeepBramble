using OWML.ModHelper;
using OWML.Common;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;

namespace DeepBramble
{
    public class DeepBramble : ModBehaviour
    {
        public INewHorizons NewHorizonsAPI;
        public static DeepBramble instance;
        private SignalHelper signalHelper;

        /**
         * Do NH setup stuff and patch certain methods
         */
        private void Start()
        {
            //NH setup stuff
            NewHorizonsAPI = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NewHorizonsAPI.LoadConfigs(this);
            UnityEvent<string> loadEvent = NewHorizonsAPI.GetStarSystemLoadedEvent();
            loadEvent.AddListener(PrepBrambleSystem);

            //Make our signal helper
            this.signalHelper = new SignalHelper();

            instance = this;
        }

        /**
         * When a star system finishes loading, check if it's the bramble system
         * If it is the bramble system, set every non-dimension astro object to have a higher priority gravity field
         * 
         * The s is not used in this function
         */
        private void PrepBrambleSystem(String s)
        {
            //Only do stuff if we're in the bramble system now
            if(NewHorizonsAPI.GetCurrentStarSystem().Equals("BrambleSystem"))
            {
                //Do some things to each astro object
                foreach(AstroObject i in Component.FindObjectsOfType<AstroObject>())
                {
                    //If it has a gravity volume, increase the priority to 2
                    this.FixGravity(i.gameObject);

                    //If it's a dimension, disable the speed limit
                    this.RemoveSpeedLimit(i.gameObject);
                }

                //Fix the parents of all of the signals
                this.signalHelper.PrepDictionary();
                this.signalHelper.FixSignalParents();
            }
        }

        /**
         * If this object has a child with a gravity field, make it a higher priority
         */
        private void FixGravity(GameObject body)
        {
            GravityVolume volume = body.GetComponentInChildren<GravityVolume>();
            if (volume != null)
                volume._priority = 2;
        }

        /**
         * If this object is a bramble dimension, remove the speed limit
         */
        private void RemoveSpeedLimit(GameObject body)
        {
            if(body.GetComponentInChildren<DarkBrambleRepelVolume>() != null)
            {
                //body.GetComponentInChildren<ThrustRuleset>()._thrustLimit = 9999999;
                body.GetComponentInChildren<ThrustRuleset>().enabled = false;
                //body.GetComponentInChildren<ThrustRuleset>();
            }
        }

        private void Update()
        {
            if (Keyboard.current[Key.K].wasPressedThisFrame)
            {
                ModHelper.Console.WriteLine("Velocity: " + Locator.GetShipBody().GetVelocity().magnitude);
                ModHelper.Console.WriteLine("Drag: " + Locator.GetShipBody().GetRigidbody().drag);
            }
        }

        public static void debugPrint(string str)
        {
            instance.ModHelper.Console.WriteLine(str);
        }
    }
}
