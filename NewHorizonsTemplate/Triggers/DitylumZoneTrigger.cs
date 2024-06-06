using UnityEngine;

namespace DeepBramble.Triggers
{
    public class DitylumZoneTrigger : MonoBehaviour
    {
        public float maxFogDist;
        public float minFogDist = 52f;
        private PlanetaryFogController fogCore;
        private float maxIntensity;
        public EndlessCylinder warpCylinder = null;
        private bool applyTerminalV = false;
        public float terminalV = 10f;

        /**
         * On awake, do some setup
         */
        private void Awake()
        {
            //Grab the fog core
            fogCore = transform.parent.Find("atmo/FogSphere_Hub").GetComponent<PlanetaryFogController>();
            maxFogDist = fogCore.fogRadius;
            maxIntensity = fogCore.fogDensity;

            //Set up the trigger
            GetComponent<OWTriggerVolume>().OnEntry += OnEnter;
            GetComponent<OWTriggerVolume>().OnExit += OnExit;
        }

        /**
         * When the player enters the trigger, disable the warp cylinder
         * 
         * @param other The other object
         */
        private void OnEnter(GameObject other)
        {
            if (other.CompareTag("PlayerDetector"))
            {
                warpCylinder.SetActivation(false);
                applyTerminalV = true;
            }
        }

        /**
         * When the player exits the trigger, enable the warp cylinder
         * 
         * @param other The other object
         */
        private void OnExit(GameObject other)
        {
            if (other.CompareTag("PlayerDetector"))
            {
                warpCylinder.SetActivation(true);
                applyTerminalV = false;
            }
        }

        /**
         * Every frame, update the fog density
         */
        private void LateUpdate()
        {
            OWRigidbody playerBody = Locator.GetPlayerBody();
            float t = (Vector3.Distance(playerBody.transform.position, fogCore.transform.position) - minFogDist) / (maxFogDist - minFogDist);
            fogCore.fogDensity = Mathf.Lerp(maxIntensity, 0, t);

        }

        /**
         * Every frame, cap the player speed
         */
        private void FixedUpdate()
        {
            OWRigidbody playerBody = Locator.GetPlayerBody();
            if (applyTerminalV && playerBody.GetVelocity().magnitude > terminalV)
            {
                Vector3 wantedVel = playerBody.GetVelocity().normalized * terminalV;
                playerBody.SetVelocity(wantedVel);
            }
        }
    }
}
