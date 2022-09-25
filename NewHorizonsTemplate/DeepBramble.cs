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
    public class DeepBramble : ModBehaviour
    {
        public INewHorizons NewHorizonsAPI;
        public static DeepBramble instance;
        private SignalHelper signalHelper;
        private bool shipDriftFixPrimed = false;

        //Only needed for debug
        public static Transform relBody = null;

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

            //Make patches
            //Wake up dimensions as we enter them
            ModHelper.HarmonyHelper.AddPrefix<OuterFogWarpVolume>(
                "ReceiveWarpedDetector",
                typeof(Patches),
                nameof(Patches.WakeOnEnter));

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

            //Save and deactivate the black hole into the vessel dimension
            ModHelper.HarmonyHelper.AddPostfix<VanishVolume>(
                "Awake",
                typeof(Patches),
                nameof(Patches.VanishVolumeListener));

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
         * The s is not used in this function
         */
        private void PrepBrambleSystem(String s)
        {
            //Only do stuff if we're in the bramble system now
            if (NewHorizonsAPI.GetCurrentStarSystem().Equals("BrambleSystem"))
            {
                //Do some things to each astro object
                foreach (AstroObject i in Component.FindObjectsOfType<AstroObject>())
                {
                    //If it has a gravity volume, increase the priority to 2
                    this.FixGravity(i.gameObject);

                    //If it's a dimension, do some fixes
                    this.FixDimension(i.gameObject);
                }

                //Fix the parents of all of the signals
                this.signalHelper.PrepDictionary();
                this.signalHelper.FixSignalParents();

                //Prime the ship drift fix
                this.shipDriftFixPrimed = true;
            }

            //If we're not in the bramble system, clear the bramble containers
            else
                BrambleContainer.clear();
        }

        /**
         * If this object has a child with a gravity field, make it a higher priority
         */
        private void FixGravity(GameObject body)
        {
            //Increase the priority
            GravityVolume volume = body.GetComponentInChildren<GravityVolume>();
            if (volume != null)
                volume._priority = 2;
        }

        /**
         * If this object is a bramble dimension, remove the speed limit
         */
        private void FixDimension(GameObject body)
        {
            //Disable the thrust and drag
            if(body.GetComponentInChildren<DarkBrambleRepelVolume>() != null)
            {
                body.GetComponentInChildren<ThrustRuleset>().enabled = false;
                body.GetComponentInChildren<SimpleFluidVolume>()._density = 0;
            }

            //Set up each dimension with the things it needs to grab
            if(body.GetComponent<AstroObject>()._name == AstroObject.Name.CustomString)
            {
                switch(body.GetComponent<AstroObject>()._customName)
                {
                    case "Start Dimension":
                        BrambleContainer.containers.Add(new BrambleContainer(body, new string[] { "StartDimensionFlare", "StartDimensionRecorderContainer" }, true));
                        break;
                }
            }
        }

        /**
         * Do certain things every frame (mostly debug key presses)
         */
        private void Update()
        {
            //If it's primed, fix the ship drift
            if (this.shipDriftFixPrimed && Locator.GetShipBody() != null)
            {
                Locator.GetShipBody().SetVelocity(Vector3.zero);
                this.shipDriftFixPrimed = false;
            }

            //Print the player's absolute and relative positions when k is pressed
            if (Keyboard.current[Key.K].wasPressedThisFrame)
            {
                Transform absCenter = null;
                foreach (AstroObject i in Component.FindObjectsOfType<AstroObject>())
                {
                    //Find the center
                    if (i._name == AstroObject.Name.CustomString && i._customName.Equals("The Center"))
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
        }

        public static void debugPrint(string str)
        {
            instance.ModHelper.Console.WriteLine(str);
        }
    }
}
