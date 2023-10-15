using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.Triggers
{
    public class AudioSwitchTrigger : MonoBehaviour
    {
        private bool audioSwitched = false;

        /**
         * When starting, need to prime the trigger
         */
        private void Awake()
        {
            gameObject.GetComponent<OWTriggerVolume>().OnEntry += SwitchAudio;
            DeepBramble.debugPrint("audio switcher woke up");
        }

        /**
         * Switch the audio when we trigger
         */
        private void SwitchAudio(GameObject other)
        {
            DeepBramble.debugPrint(other.name + " was detected by audio switcher");
            if (!audioSwitched && other.CompareTag("PlayerDetector"))
            {
                DeepBramble.debugPrint("Switching audio");
                audioSwitched = true;
                transform.parent.Find("domestic_ambience_calm").gameObject.SetActive(true);
            }
        }

        /**
         * Disentangle from triggers if we're destroyed
         */
        private void OnDestroy()
        {
            GetComponent<OWTriggerVolume>().OnEntry -= SwitchAudio;
        }
    }
}
