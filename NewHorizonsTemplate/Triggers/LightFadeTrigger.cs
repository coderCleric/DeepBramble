using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace DeepBramble.Triggers
{
    class LightFadeTrigger : MonoBehaviour
    {
        //Constant for easy edits
        public float fadetime = 0.5f;

        //Variables
        private List<Light> lights = new List<Light>();
        private List<float> intensities = new List<float>();
        private bool isDimming = false;
        private float changeTime = 0;

        /**
         * Adds the given light to the trigger
         */
        public void AddLight(Light light)
        {
            this.lights.Add(light);
            this.intensities.Add(light.intensity);
        }

        /**
         * Dim the lights when the player enters the trigger
         * 
         * @param other The entering collider
         */
        private void OnTriggerEnter(Collider other)
        {
            for(int i = 0; i < intensities.Count; i++)
            {
                if(intensities[i] == 0)
                {
                    intensities[i] = lights[i].intensity;
                }
            }

            if(other.gameObject.GetAttachedOWRigidbody().CompareTag("Player"))
            {
                this.changeTime = fadetime;
                isDimming = true;
            }
        }

        /**
         * Undim the lights when the player exits the trigger
         * 
         * @param other The entering collider
         */
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.GetAttachedOWRigidbody().CompareTag("Player"))
            {
                this.changeTime = fadetime;
                isDimming = false;
            }
        }

        /**
         * Update the light intensity every frame
         */
        private void Update()
        {
            if (changeTime > 0)
            {
                //Dim them if we're fading
                if (isDimming)
                {
                    for (int i = 0; i < this.lights.Count(); i++)
                    {
                        this.lights[i].intensity -= this.intensities[i] * (Time.deltaTime / fadetime);
                        this.lights[i].intensity = Mathf.Max(0, this.lights[i].intensity);
                    }
                }

                //Bring them back otherwise
                else
                {
                    for (int i = 0; i < this.lights.Count(); i++)
                    {
                        this.lights[i].intensity += this.intensities[i] * (Time.deltaTime / fadetime);
                        this.lights[i].intensity = Mathf.Min(this.intensities[i], this.lights[i].intensity);
                    }
                }
                changeTime -= Time.deltaTime;
            }
        }
    }
}
