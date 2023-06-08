using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.MiscBehaviours
{
    public class Lever : MonoBehaviour
    {
        private GameObject beamObject = null;
        private GameObject spikeObject = null;
        private InteractReceiver interactor = null;
        private Animator animator = null;
        private bool permaDisable = false;

        /**
         * Grab needed components on awake
         */
        private void Awake()
        {
            interactor = GetComponentInChildren<InteractReceiver>();
            animator = GetComponent<Animator>();

            //Make sure needed components were found
            if(interactor == null || animator == null)
            {
                DeepBramble.debugPrint("Lever " + name + " failed to find interactor or animator!");
                return;
            }

            //Set the interact receiver to activate the animator
            interactor.OnPressInteract += OnFlip;
        }

        /**
         * Change the prompt
         */
        private void Start()
        {
            if (interactor != null)
                interactor.ChangePrompt("Flip Lever");
        }

        /**
         * Remove the action if this is destroyed
         */
        private void OnDestroy()
        {
            if (interactor != null)
                interactor.OnPressInteract -= OnFlip;
        }

        /**
         * Registers the given transform as the beam, requiring a very specific structure
         * 
         * @param beamObj The object to register
         */
        public void RegisterBeam(GameObject beamObject)
        {
            //Grab the beam, then the spikes
            this.beamObject = beamObject;
            if(beamObject.transform.parent.Find("spikes") != null)
                this.spikeObject = beamObject.transform.parent.Find("spikes").gameObject;
        }

        /**
         * Activate the animator
         */
        private void OnFlip()
        {
            animator.SetTrigger("flip");
            bool flipBool = !beamObject.activeSelf;
            if (permaDisable)
                flipBool = false;
            beamObject.SetActive(flipBool);
            if (spikeObject != null)
            {
                spikeObject.transform.Find("collider").gameObject.SetActive(!flipBool);
                spikeObject.transform.Find("killzone").gameObject.SetActive(flipBool);
            }
        }

        /**
         * Permanently disables the beam that the lever is attached to
         */
        public void PermaDisable()
        {
            permaDisable = true;
            beamObject.SetActive(false);
            if (spikeObject != null)
            {
                spikeObject.transform.Find("collider").gameObject.SetActive(true);
                spikeObject.transform.Find("killzone").gameObject.SetActive(false);
            }
        }

        /**
         * Creates levers on the given beam root, which must have a very specific structure
         * 
         * @param rootTransform The root transform to create from
         */
        public static void MakeLevers(Transform rootTransform)
        {
            //Get the lists of beams and levers
            TractorBeamFluid[] beams = rootTransform.GetComponentsInChildren<TractorBeamFluid>(true);
            InteractReceiver[] levers = rootTransform.GetComponentsInChildren<InteractReceiver>();

            //Make the actual lever components
            Lever[] levComponents = new Lever[6];
            for(int i = 0; i < 6; i++)
            {
                levComponents[i] = levers[i].transform.parent.gameObject.AddComponent<Lever>();
            }

            //Map them to the beams
            levComponents[0].RegisterBeam(beams[0].gameObject);
            levComponents[1].RegisterBeam(beams[4].gameObject);
            levComponents[2].RegisterBeam(beams[1].gameObject);
            levComponents[3].RegisterBeam(beams[2].gameObject);
            levComponents[4].RegisterBeam(beams[5].gameObject);
            levComponents[5].RegisterBeam(beams[3].gameObject);
        }
    }
}
