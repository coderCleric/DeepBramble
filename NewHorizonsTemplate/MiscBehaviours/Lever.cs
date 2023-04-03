using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.MiscBehaviours
{
    class Lever : MonoBehaviour
    {
        public GameObject beamObject = null;
        private InteractReceiver interactor = null;
        private Animator animator = null;
        public bool permaDisable = false;

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
         * Activate the animator
         */
        private void OnFlip()
        {
            animator.SetTrigger("flip");
            if(!permaDisable)
                beamObject.SetActive(!beamObject.activeSelf);
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
            levComponents[0].beamObject = beams[0].gameObject;
            levComponents[1].beamObject = beams[4].gameObject;
            levComponents[2].beamObject = beams[1].gameObject;
            levComponents[3].beamObject = beams[2].gameObject;
            levComponents[4].beamObject = beams[5].gameObject;
            levComponents[5].beamObject = beams[3].gameObject;
        }
    }
}
