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
        /**
         * When something biteable enters the bit trigger, latch onto it
         */
        private void OnTriggerEnter(Collider other)
        {
            //Gonna need the rigidbody for identification
            OWRigidbody otherBody = other.gameObject.GetAttachedOWRigidbody();

            //Only do stuff if it's the ship or the player
            if(otherBody.CompareTag("Player") || otherBody.CompareTag("Ship"))
            {
                DeepBramble.debugPrint("Either player or ship should be bitten");

                //First, latch onto them and enter a trance

                //Second, if it's the player, start hurting them
            }
        }
    }
}
