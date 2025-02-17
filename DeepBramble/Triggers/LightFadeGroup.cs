using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DeepBramble.Triggers
{
    public class LightFadeGroup : MonoBehaviour
    {
        private List<Light> lights = new List<Light>();
        private List<float> intensities = new List<float>();
        private List<LightFadeTrigger> triggers = new List<LightFadeTrigger>();
        private int dimCount = 0;
        private float minDimTime = 0;
        private float changeStartTime = 0;
        private float changeDelay = 0; //Need this to have a grace period after exiting
        bool fading = false;

        /**
         * Adds the given light to the group
         */
        public void AddLight(Light light)
        {
            this.lights.Add(light);
            this.intensities.Add(light.intensity);
        }

        /**
         * Add the given fade trigger to the component
         * 
         * @param trigger The trigger to register
         */
        public void RegisterTrigger(LightFadeTrigger trigger)
        {
            if(triggers.SafeAdd(trigger))
            {
                trigger.fadeGroup = this;
            }
        }

        /**
         * Tells the group that the given trigger has been entered
         * 
         * @param trigger The fade trigger that was activated
         */
        public void OnTriggerActivate(LightFadeTrigger trigger)
        {
            //Make sure we have non-zero max intensities where possible
            for (int i = 0; i < intensities.Count; i++)
            {
                if (intensities[i] == 0)
                {
                    intensities[i] = lights[i].intensity;
                }
            }

            //Increment the dim count
            dimCount++;

            //Check if this should be the new minimum dim time
            if (dimCount > 1)
                minDimTime = Mathf.Min(minDimTime, trigger.fadetime);
            else
            {
                minDimTime = trigger.fadetime;
                if (changeDelay <= 0) //Only update the change time if we're outside of the delay
                {
                    changeStartTime = Time.time;
                    fading = true;
                    DeepBramble.debugPrint("Light time changed by entering");
                }
            }
        }

        /**
         * Tells the group that the given trigger has been exited
         * 
         * @param trigger The fade trigger that was deactivated
         */
        public void OnTriggerDeactivate(LightFadeTrigger trigger)
        {
            //Decrement the dim count
            dimCount--;

            //Figure out which trigger should set the fade time
            if (dimCount > 0)
            {
                float min = Mathf.Infinity;
                foreach (LightFadeTrigger currentTrigger in triggers)
                {
                    min = Mathf.Min(min, currentTrigger.fadetime);
                }
                minDimTime = min;
            }
            else
                changeDelay = 5;
        }

        /**
         * Update the light intensity every frame
         */
        private void LateUpdate()
        {
            if (changeDelay > 0 && dimCount <= 0)
            {
                changeDelay--;
            }
            if(changeDelay <= 0 && fading && dimCount <= 0)
            {
                changeStartTime = Time.time;
                fading = false;
                DeepBramble.debugPrint("Light time changed by exiting");
            }

            float changeAmount = (Time.time - changeStartTime) / minDimTime;

            //Dim them if we're fading
            if (fading)
            {
                for (int i = 0; i < this.lights.Count(); i++)
                {
                    this.lights[i].intensity = Mathf.Lerp(this.intensities[i], 0, changeAmount);
                }
            }

            //Bring them back otherwise
            else if(changeDelay <= 0)
            {
                for (int i = 0; i < this.lights.Count(); i++)
                {
                    this.lights[i].intensity = Mathf.Lerp(0, this.intensities[i], changeAmount);
                }
            }
        }
    }
}
