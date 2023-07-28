using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.Triggers
{
    internal class CoolZone : MonoBehaviour
    {

        /**
         * Put our events on the trigger volume
         */
        private void Awake()
        {
            OWTriggerVolume trig = gameObject.GetComponent<OWTriggerVolume>();
            trig.OnEntry += DisableHeat;
            trig.OnExit += EnableHeat;
        }

        /**
         * When the player enters, up the count and disable the heat
         * 
         * @param other The entering collider
         */
        private void DisableHeat(GameObject other)
        {
            DeepBramble.debugPrint(other.name + " entered the cool zone");

            //Remove the hazard
            if (other.CompareTag("PlayerDetector") || other.CompareTag("ProbeDetector"))
            {
                DeepBramble.instance.ModHelper.Events.Unity.FireOnNextUpdate(() =>
                {
                    other.GetComponent<HazardDetector>().RemoveVolume(ForgottenLocator.hotNodeHazard);
                });
            }

            //Unpin the notification
            if (other.CompareTag("PlayerDetector"))
                NotificationManager.SharedInstance.UnpinNotification(HeatWarningTrigger.heatNotification);
        }

        /**
         * When the player leaves, lower the count and enable the heat
         * 
         * @param other The entering collider
         */
        private void EnableHeat(GameObject other)
        {
            DeepBramble.debugPrint(other.name + " left the cool zone");

            //Add the hazard back
            if (other.CompareTag("PlayerDetector") || other.CompareTag("ProbeDetector"))
            {
                DeepBramble.instance.ModHelper.Events.Unity.FireOnNextUpdate(() =>
                {
                    other.GetComponent<HazardDetector>().AddVolume(ForgottenLocator.hotNodeHazard);
                });
            }

            //Post and pin the notification
            if (other.CompareTag("PlayerDetector"))
                NotificationManager.SharedInstance.PostNotification(HeatWarningTrigger.heatNotification, true);
        }

        /**
         * Remove our events if we're destroyed
         */
        private void OnDestroy()
        {
            OWTriggerVolume trig = gameObject.GetComponent<OWTriggerVolume>();
            trig.OnEntry -= DisableHeat;
            trig.OnExit -= EnableHeat;
        }
    }
}
