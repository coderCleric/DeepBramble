using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.Triggers
{
    class GasVolume : MonoBehaviour
    {
        private static JetpackThrusterModel playerPack;
        public static GameObject baseExplosion;

        private NotificationData gasNotification = new NotificationData(NotificationTarget.Player, "WARNING: FLAMMABLE GAS DETECTED");
        private BaseInheritors.PlayerExplosionController explosionController;
        private bool playerIn = false;
        private bool shouldExplode = false;
        private bool exploded = false;
        private float thrustTime = 0;
        private float explodeTime = 1;

        /**
         * Need to grab the player pack on start
         */
        private void Start()
        {
            playerPack = Locator.GetPlayerTransform().gameObject.GetComponent<JetpackThrusterModel>();
        }

        /**
         * Push a warning and prime the player to explode on enter
         * 
         * @param other The other collider
         */
        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.GetAttachedOWRigidbody().CompareTag("Player"))
            {
                //Post and pin the notification
                NotificationManager.SharedInstance.PostNotification(gasNotification, true);
                playerIn = true;
            }
        }

        /**
         * Remove the warning and stop the player from exploding on exit
         * 
         * @param other The other collider
         */
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.GetAttachedOWRigidbody().CompareTag("Player"))
            {
                //Post and pin the notification
                NotificationManager.SharedInstance.UnpinNotification(gasNotification);
                playerIn = false;
            }
        }

        /**
         * Every frame, check if the player should explode
         */
        private void FixedUpdate()
        {
            if(playerIn)
            {
                //If they're thrusting normally, get closer to exploding
                if (playerPack.IsTranslationalThrusterFiring())
                    thrustTime += Time.deltaTime;
                else //Otherwise, reset the time
                    thrustTime = 0;

                //Actually explode them
                if(shouldExplode && !exploded)
                {
                    DeepBramble.debugPrint("Player should explode now");
                    explosionController.Play();
                    exploded = true;
                }

                //Check if they should make the explosion
                if((thrustTime >= explodeTime || playerPack.IsBoosterFiring()) && !exploded)
                {
                    DeepBramble.debugPrint("Player explosion primed.");

                    //Load the explosion from the bundle & instantiate
                    GameObject explosionObject = GameObject.Instantiate(baseExplosion, Locator._playerTransform.position, Locator._playerTransform.rotation, baseExplosion.transform.parent);
                    explosionController = explosionObject.transform.Find("explosion").gameObject.AddComponent<BaseInheritors.PlayerExplosionController>();
                    shouldExplode = true;
                }
            }
        }
    }
}
