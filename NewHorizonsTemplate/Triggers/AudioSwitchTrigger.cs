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
        private void Start()
        {
            GetComponent<OWTriggerVolume>().OnEntry += OnEntry;
        }

        /**
         * Switch the audio when we trigger
         */
        private void OnEntry(GameObject other)
        {
            if (!audioSwitched && other.CompareTag("PlayerDetector"))
            {
                DeepBramble.debugPrint("Switching audio");
                audioSwitched = true;
                OWAudioSource source = transform.parent.Find("domestic_ambience").GetComponent<OWAudioSource>();
                source.AssignAudioLibraryClip(AudioType.Reel_2_Backdrop_A);
            }
        }

        /**
         * Disentangle from triggers if we're destroyed
         */
        private void OnDestroy()
        {
            GetComponent<OWTriggerVolume>().OnEntry -= OnEntry;
        }
    }
}
