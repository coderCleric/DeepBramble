using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.MiscBehaviours
{
    internal class ScrollTextSwitcher : MonoBehaviour
    {
        private ScrollItem scroll = null;
        private NomaiWallText normalText = null;
        private NomaiWallText altText = null;
        private OWTriggerVolume trigger = null;

        /**
         * On awake, grab the texts the scroll may display
         */
        private void Awake()
        {
            scroll = GetComponent<ScrollItem>();
            NomaiWallText[] textList = GetComponentsInChildren<NomaiWallText>();
            normalText = textList[1];
            altText = textList[2];
            altText.InitializeAsWhiteboardText();
        }

        private void Start()
        {
            altText.Hide();
        }

        /**
         * Register the given trigger
         * 
         * @param trigger The trigger to register
         */
        public void RegisterTrigger(OWTriggerVolume trigger)
        {
            if(this.trigger != null)
            {
                trigger.OnEntry -= UseAltText;
                trigger.OnExit -= UseNormalText;
            }

            this.trigger = trigger;
            this.trigger.OnEntry += UseAltText;
            this.trigger.OnExit += UseNormalText;
        }

        /**
         * Switch to the alt text when the trigger is entered
         */
        private void UseAltText(GameObject other)
        {
            if (other.GetAttachedOWRigidbody().CompareTag("Player"))
            {
                scroll._nomaiWallText = altText;
            }
        }

        /**
         * Switch to the normal text when the trigger is exited
         */
        private void UseNormalText(GameObject other)
        {
            if (other.GetAttachedOWRigidbody().CompareTag("Player"))
            {
                scroll._nomaiWallText = normalText;
            }
        }

        /**
         * Remove method references if this object is destroyed
         */
        private void OnDestroy()
        {
            trigger.OnEntry -= UseAltText;
            trigger.OnExit -= UseNormalText;
        }
    }
}
