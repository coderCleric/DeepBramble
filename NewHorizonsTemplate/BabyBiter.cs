using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NewHorizons;

namespace DeepBramble
{
    class BabyBiter : MonoBehaviour
    {
        Transform fishTransform = null;

        /**
         * On start, grab the fish that we're attached to
         */
        public void Start()
        {
            this.fishTransform = this.transform.parent;
        }

        /**
         * When something biteable enters the bit trigger, latch onto it
         */
        private void OnTriggerEnter(Collider other)
        {
            //Gonna need the rigidbody for identification
            OWRigidbody otherBody = other.gameObject.GetAttachedOWRigidbody();

            //Only do stuff if it's the ship or the player
            if(otherBody.CompareTag("Player"))
            {
                DeepBramble.debugPrint("Either player or ship should be bitten");

                //Set the other object as our parent so we move with it
                fishTransform.SetParent(other.transform, true);

                //Destroy/disable just about everything related to the fish movement
                Component.Destroy(fishTransform.GetComponent<ImpactSensor>());
                Component.Destroy(fishTransform.GetComponent<BabyFishController>());
                Component.Destroy(fishTransform.GetComponent<OWRigidbody>());
                foreach (Collider i in fishTransform.GetComponentsInChildren<Collider>())
                    i.enabled = false;
                foreach (OWCollider i in fishTransform.GetComponentsInChildren<OWCollider>())
                    i.enabled = false;

                //Set the fish to idle and disable the brain
                fishTransform.GetComponent<BabyFishController>().ChangeState(AnglerfishController.AnglerState.Lurking);
                fishTransform.GetComponent<BabyFishController>().enabled = false;

                //Second, if it's the player, start hurting them
            }
        }
    }
}
