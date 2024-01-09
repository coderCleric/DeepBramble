using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.MiscBehaviours
{
    public class NodeKiller : MonoBehaviour
    {
        private bool dead = false;
        private float dieTime = 0;
        private bool waitingToDie = false;

        //Things it manipulates on death
        private GameObject effects = null;
        private GameObject liveNode = null;
        private GameObject caps = null;
        private Material deadMat = null;
        private OWAudioSource deathAudio = null;

        /**
         * When it wakes up, grab some needed things
         */
        private void Awake()
        {
            effects = transform.Find("Effects").gameObject;
            liveNode = transform.Find("Terrain_DB_BrambleSphere_Inner_v2").gameObject;
            caps = transform.Find("dead_node/gateway_seals").gameObject;
            deadMat = caps.GetComponentInChildren<MeshRenderer>().material;
            deathAudio = transform.Find("dead_node/wood_audio").gameObject.GetComponent<OWAudioSource>();
            caps.SetActive(false);
        }

        /**
         * Kills the node that the killer is attached to. Requires a very specific object structure, 
         * make sure the killer is with the inner fog warp at the top level of the node
         */
        public void KillNode()
        {
            //Can't kill what's already dead
            if(dead) 
                return;

            //Flicker the screen and set up the death
            GlobalMessenger<float, float>.FireEvent("FlickerOffAndOn", 3f, 4f);
            dieTime = Time.time + 3f;
            waitingToDie = true; 
            
            //Rumble the controller
            RumbleManager.Pulse(0.5f, 0.5f, 4.3f);

            //Emergency recall the scout
            if (ForgottenLocator.probeDilated)
                Locator.GetProbe().ExternalRetrieve();
            DeepBramble.recallTimer = -999;

            //Play some freaky wood sounds
            deathAudio.Play();

            //Make it impossible to enter
            gameObject.GetComponent<InnerFogWarpVolume>()._warpRadius = 0f;

            //Set it to be dead
            dead = true;

            //Reveal ship log fact
            Locator.GetShipLogManager().RevealFact("DITYLUM_EJECTION_FACT_FC");
        }

        /**
         * Need to listen for when we should actually die
         */
        private void Update()
        {
            //Need to wait for the proper moment to die
            if (waitingToDie && dieTime <= Time.time)
            {
                waitingToDie = false;

                //Make the node visibly dead
                effects.SetActive(false);
                caps.SetActive(true);
                foreach(MeshRenderer rend in liveNode.GetComponentsInChildren<MeshRenderer>())
                {
                    rend.material = deadMat;
                }

                //Disable the Dree signal
                GetComponentInChildren<AudioSignal>().SetSignalActivation(false, 0);

                //Trigger Ditylum to get going
                ForgottenLocator.swimmingDitylum.LockPlayerOn();
                ForgottenLocator.swimmingDitylum.BeginSequence();

                //Enable the Ditylum at the lab
                ForgottenLocator.sadDitylum.gameObject.SetActive(true);

                //Perma-block the quantum rock
                ForgottenLocator.permaBlockRock.SetActive(true);
                ForgottenLocator.permaBlockableSocket.isPermaBlocked = true;
                ForgottenLocator.quantumRock.Collapse();
            }
        }
    }
}
