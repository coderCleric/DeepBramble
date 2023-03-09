using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.Triggers
{
    class LightFadeGroup : MonoBehaviour
    {
        private List<Light> lights = new List<Light>();
        private List<float> intensities = new List<float>();
        private List<LightFadeTrigger> triggers = new List<LightFadeTrigger>();
        private int dimCount = 0;
        private float minDimTime = Mathf.Infinity;

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
                minDimTime = trigger.fadetime;
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
        }

        /**
         * Update the light intensity every frame
         */
        private void Update()
        {
            //Dim them if we're fading
            if (dimCount > 0)
            {
                for (int i = 0; i < this.lights.Count(); i++)
                {
                    this.lights[i].intensity -= this.intensities[i] * (Time.deltaTime / minDimTime);
                    this.lights[i].intensity = Mathf.Max(0, this.lights[i].intensity);
                }
            }

            //Bring them back otherwise
            else
            {
                for (int i = 0; i < this.lights.Count(); i++)
                {
                    this.lights[i].intensity += this.intensities[i] * (Time.deltaTime / minDimTime);
                    this.lights[i].intensity = Mathf.Min(this.intensities[i], this.lights[i].intensity);
                }
            }
        }
    }
}
