using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.Triggers
{
    internal class HeatWarningTrigger : MonoBehaviour
    {
        public static NotificationData heatNotification = new NotificationData(NotificationTarget.Player, "WARNING: EXCESSIVE HEAT DETECTED");

        /**
         * Put our events on the trigger volume
         */
        private void Awake()
        {
            OWTriggerVolume trig = gameObject.GetComponent<OWTriggerVolume>();
            trig.OnEntry += PostNotification;
            trig.OnExit += UnpostNotification;
        }

        /**
         * Push a warning and prime the player to explode on enter
         * 
         * @param other The other collider
         */
        private void PostNotification(GameObject other)
        {
            if (other.CompareTag("PlayerDetector"))
            {
                //Post and pin the notification
                NotificationManager.SharedInstance.PostNotification(heatNotification, true);
            }
        }

        /**
         * Remove the warning and stop the player from exploding on exit
         * 
         * @param other The other collider
         */
        private void UnpostNotification(GameObject other)
        {
            if (other.CompareTag("PlayerDetector"))
            {
                //Post and pin the notification
                NotificationManager.SharedInstance.UnpinNotification(heatNotification);
            }
        }

        /**
         * Remove our events if we're destroyed
         */
        private void OnDestroy()
        {
            OWTriggerVolume trig = gameObject.GetComponent<OWTriggerVolume>();
            trig.OnEntry -= PostNotification;
            trig.OnExit -= UnpostNotification;
        }
    }
}
