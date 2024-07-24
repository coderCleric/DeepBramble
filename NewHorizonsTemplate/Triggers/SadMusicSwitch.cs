using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.Triggers
{
    public class SadMusicSwitch : MonoBehaviour
    {
        public AudioVolume audioVol;
        public float fadeTime = 1;
        private float fadeComplete = -1;
        private bool alreadySwitched = false;

        /**
         * On awake, connect to the actual trigger
         */
        private void Awake()
        {
            GetComponent<OWTriggerVolume>().OnEntry += SwitchAudio;
        }

        /**
         * When the player enters the area, switch the music
         */
        private void SwitchAudio(GameObject other)
        {
            if (!alreadySwitched && other.CompareTag("PlayerDetector"))
            {
                audioVol._owAudioSrc.FadeOut(fadeTime);
                fadeComplete = Time.time + fadeTime;
                alreadySwitched = true;
            }
        }

        /**
         * Disentangle from triggers if we're destroyed
         */
        private void OnDestroy()
        {
            GetComponent<OWTriggerVolume>().OnEntry -= SwitchAudio;
        }

        /**
         * Do the switch at the end of the fade
         */
        private void Update()
        {
            if(fadeComplete > 0 && Time.time > fadeComplete)
            {
                fadeComplete = -1;
                audioVol._owAudioSrc.AssignAudioLibraryClip(AudioType.SadNomaiTheme);
                audioVol._owAudioSrc.FadeIn(1);
            }
        }
    }
}
