using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using OWML.ModHelper;
using UnityEngine;

namespace DeepBramble.Helpers
{
    public static class TitleScreenHelper
    {
        //The different objects needed for the title screen to work right
        public static AssetBundle titleBundle = null;
        public static AudioClip titleMusic = null;
        private static bool vanillaTitle = false;
        public static GameObject titleEffectsObject = null;
        private static GameObject cricketAudio = null;

        /**
         * Creates all of the objects when the title screen first loads, and disables them if they're not yet needed
         */
        public static void FirstTimeTitleEdits()
        {
            DeepBramble.debugPrint("First time title edits");

            //Load the title screen music
            titleMusic = AudioUtilities.LoadAudio(Path.Combine(DeepBramble.instance.ModHelper.Manifest.ModFolderPath, "assets", "Audio", "title_music.ogg"));

            //Determine if we'll actually want to use this stuff (need to load data early to do this)
            StandaloneProfileManager.SharedInstance.Initialize();
            bool editsNeeded = StandaloneProfileManager.SharedInstance.currentProfileGameSave.GetPersistentCondition("DeepBrambleFound") && !vanillaTitle;

            //Find the background object
            GameObject backgroundObject = GameObject.Find("Scene/Background");
            if (backgroundObject == null)
            {
                DeepBramble.debugPrint("Couldn't find background object");
                return;
            }

            //Load the custom effects bundle, make it a child of the background object
            titleEffectsObject = titleBundle.LoadAsset<GameObject>("Assets/Prefabs/titlescreeneffects.prefab");
            if (titleEffectsObject == null)
            {
                DeepBramble.debugPrint("Couldn't load title effects object");
                return;
            }
            titleEffectsObject = GameObject.Instantiate(titleEffectsObject, backgroundObject.transform);
            AssetBundleUtilities.ReplaceShaders(titleEffectsObject);
            titleEffectsObject.name = "DB Title Effects Object";
            titleEffectsObject.transform.position = new Vector3(116.455f, 368.8177f, -47.0909f);
            titleEffectsObject.transform.rotation = Quaternion.Euler(327.4284f, 1.9997f, 340.8541f);

            //Change the campfire appearance
            //Would need to revert this, so just gonna disable it for now
            //CampFireHelper.ChangeFireAppearance(backgroundObject.transform.Find("PlanetPivot/Prefab_HEA_Campfire/Controller_Campfire").GetComponent<Campfire>());

            //Disable the cricket noises
            cricketAudio = GameObject.Find("Scene/AudioSource_Ambience");
            cricketAudio.SetActive(false);

            //Some changes should only be made if the title screen is altered
            if (editsNeeded)
            {
                //Set the animator to play at a specific point
                Animator animator = GameObject.Find("Scene").GetComponent<Animator>();
                animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0.6f);

                //Set the custom audio
                OWAudioSource musicSource = GameObject.Find("Scene/AudioSource_Music").GetComponent<OWAudioSource>();
                musicSource._audioLibraryClip = (AudioType.None);
                musicSource.GetAudioSource().clip = titleMusic;
                musicSource.SetMaxVolume(0.2f);
            }
            else
            {
                titleEffectsObject.SetActive(false);
                cricketAudio.SetActive(true);
                cricketAudio.GetComponent<OWAudioSource>().Play();
            }

            //Subscribe to future changes
            StandaloneProfileManager.SharedInstance.OnProfileSignInComplete += OnProfileLoaded;

            DeepBramble.debugPrint("Title edits complete");
        }

        /**
         * Alter the title screen when a profile is loaded
         */
        private static void OnProfileLoaded(ProfileManagerSignInResult result)
        {
            //Error check for bad load or not having initialized
            if (result != ProfileManagerSignInResult.COMPLETE || titleEffectsObject == null)
                return;

            if(StandaloneProfileManager.SharedInstance.currentProfileGameSave.GetPersistentCondition("DeepBrambleFound") && !vanillaTitle)
                EnableTitleEdits();
            else
                DisableTitleEdits();
        }

        /**
         * Enables the special title screen behavior
         */
        public static void EnableTitleEdits()
        {
            titleEffectsObject.SetActive(true);
            cricketAudio.SetActive(false);
            DeepBramble.debugPrint("Title edits enabled");

            //Set the custom audio
            OWAudioSource musicSource = GameObject.Find("Scene/AudioSource_Music").GetComponent<OWAudioSource>();
            musicSource._audioLibraryClip = AudioType.None;
            musicSource.GetAudioSource().clip = titleMusic;
            musicSource.SetMaxVolume(0.2f);
            musicSource.Play();
        }

        /**
         * Disables the special title screen behavior
         */
        public static void DisableTitleEdits()
        {
            titleEffectsObject.SetActive(false);
            cricketAudio.SetActive(true);
            cricketAudio.GetComponent<OWAudioSource>().Play();
            DeepBramble.debugPrint("Title edits disabled");

            //Undo the custom audio
            OWAudioSource musicSource = GameObject.Find("Scene/AudioSource_Music").GetComponent<OWAudioSource>();
            musicSource.AssignAudioLibraryClip(AudioType.MainMenuTheme);
            musicSource.SetMaxVolume(0.1f);
            musicSource.Play();
        }

        /**
         * Sets whether or not to use the vanilla title screen
         */
        public static void SetVanillaTitle(bool vanillaTitle)
        {
            TitleScreenHelper.vanillaTitle = vanillaTitle;
            
            //If necessary, update the current title screen appearance
            if(titleEffectsObject != null)
            {
                if (StandaloneProfileManager.SharedInstance.currentProfileGameSave.GetPersistentCondition("DeepBrambleFound") && !vanillaTitle)
                    EnableTitleEdits();
                else
                    DisableTitleEdits();
            }

        }
    }
}
