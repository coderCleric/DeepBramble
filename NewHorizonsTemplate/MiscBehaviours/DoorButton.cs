using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.MiscBehaviours
{
    class DoorButton : MonoBehaviour
    {
        private InteractReceiver interactor = null;
        private Animator animator = null;

        /**
         * Grab needed components on awake
         */
        private void Awake()
        {
            interactor = GetComponent<InteractReceiver>();
            animator = GetComponent<Animator>();

            //Make sure needed components were found
            if(interactor == null || animator == null)
            {
                DeepBramble.debugPrint("Door Button " + name + " failed to find interactor or animator!");
                return;
            }

            //Set the interact receiver to activate the animator
            interactor.OnPressInteract += OnPress;
        }

        /**
         * Change the prompt
         */
        private void Start()
        {
            if (interactor != null)
                interactor.ChangePrompt("Press");
        }

        /**
         * Remove the action if this is destroyed
         */
        private void OnDestroy()
        {
            if (interactor != null)
                interactor.OnPressInteract -= OnPress;
        }

        /**
         * Activate the animator
         */
        private void OnPress()
        {
            animator.SetTrigger("press");
        }
    }
}
