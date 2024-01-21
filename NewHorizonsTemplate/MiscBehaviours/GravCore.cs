using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.MiscBehaviours
{
    internal class GravCore : MonoBehaviour
    {
        private bool isShattered = false;
        private float startIntensity;
        private float shatterTime;
        private OWAudioSource loopSource;
        private OWAudioSource oneShotSource;
        private ParticleSystem moteSystem;
        private ParticleSystem sparkSystem;
        private Light light;
        private GameObject crackRenderer;
        private Lever[] beamLevers;
        private GravityVolume planetGrav;
        private DirectionalForceVolume[] artificialGravs;

        /**
         * Need to grab some key components on awake
         */
        private void Awake()
        {
            //Grab all of the local components
            loopSource = transform.Find("audio/audio_loop").gameObject.GetComponent<OWAudioSource>();
            oneShotSource = transform.Find("audio/audio_oneshot").gameObject.GetComponent<OWAudioSource>();
            moteSystem = transform.Find("Effects_NOM_WarpParticlesBlack").gameObject.GetComponent<ParticleSystem>();
            sparkSystem = transform.Find("Effects_HEA_Sparks").gameObject.GetComponent<ParticleSystem>();
            light = GetComponentInChildren<Light>();
            crackRenderer = transform.Find("renderers/crack_renderer").gameObject;

            //Grab all of the beams
            beamLevers = transform.parent.GetComponentsInChildren<Lever>();

            //Grab the gravity things
            Transform planetTransform = gameObject.GetAttachedOWRigidbody().transform;
            planetGrav = planetTransform.GetComponentInChildren<GravityVolume>();
            artificialGravs = planetTransform.GetComponentsInChildren<DirectionalForceVolume>();

            //Disable the broken computer
            transform.parent.Find("crystal_lab/core_scanner_computer_broken").gameObject.SetActive(false);
        }

        /**
         * If we detect a collision, we shatter
         * 
         * @param collision The collision
         */
        private void OnTriggerEnter(Collider other)
        {
            if (!isShattered)
            {
                //Do the local effects
                loopSource.FadeOut(0.5f);
                oneShotSource.Play();
                moteSystem.Stop();
                sparkSystem.Play();
                startIntensity = light.intensity;
                crackRenderer.SetActive(true);
                isShattered = true;
                shatterTime = Time.time;

                //Disable all of the lever beams
                foreach(Lever lever in beamLevers)
                {
                    lever.PermaDisable();
                }

                //Disable the gravity
                planetGrav.gameObject.SetActive(false);
                foreach(DirectionalForceVolume grav in artificialGravs)
                {
                    grav.gameObject.SetActive(false);
                }

                //Disable the fragile audio
                transform.parent.Find("fragile_audios/core_audio").gameObject.SetActive(false);
                transform.parent.Find("fragile_audios/planet_audio").GetComponent<OWAudioSource>().SetMaxVolume(0);

                //Swap the scanner computer
                transform.parent.Find("crystal_lab/core_scanner_computer_intact").gameObject.SetActive(false);
                transform.parent.Find("crystal_lab/core_scanner_computer_broken").gameObject.SetActive(true);

                //Reveal the ship log fact
                Locator.GetShipLogManager().RevealFact("CORE_BROKEN_FACT_FC");
            }
        }

        /**
         * If needed, fade out the light
         */
        private void Update()
        {
            if (isShattered)
                light.intensity = Mathf.Lerp(startIntensity, 0, (Time.time - shatterTime) / 0.5f);
        }
    }
}
