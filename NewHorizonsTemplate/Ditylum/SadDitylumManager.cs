using NewHorizons.Utility.Files;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace DeepBramble.Ditylum
{
    public class SadDitylumManager : MonoBehaviour
    {
        private PlayerAttachPoint anchor;
        private InteractReceiver receiver;
        private CharacterDialogueTree dialogue;
        private float sitTime = -1;

        /**
         * On awake, do some prep
         */
        private void Start() 
        {
            //Grab the anchor
            anchor = GetComponentInChildren<PlayerAttachPoint>();

            //Set up the interaction
            receiver = GetComponentInChildren<InteractReceiver>();
            receiver.OnPressInteract += Sit;
            receiver.ChangePrompt("Sit");
            receiver.SetInteractionEnabled(false);

            //Set up the dialogue
            dialogue = GetComponentInChildren<CharacterDialogueTree>();
            dialogue.OnEndConversation += Scream;

            //Disable the GO
            gameObject.SetActive(false);
        }

        /**
         * After talking, scream
         */
        private void Scream()
        {
            dialogue.gameObject.SetActive(false);
            GetComponent<Animator>().SetTrigger("cry");
        }

        /**
         * Play the cry audio
         */
        public void PlayCryAudio()
        {
            GetComponent<OWAudioSource>().Play();
            RumbleManager.Pulse(0.5f, 0.5f, 3f);
        }

        /**
         * When Ditylum screams, reveal the text
         */
        public void ShowText()
        {
            ForgottenLocator.griefText.Show();
        }

        /**
         * Enables the sit interaction
         */
        public void EnableSitting()
        {
            receiver.SetInteractionEnabled(true);
        }

        /**
         * When the player interacts after reading, have them sit
         */
        private void Sit()
        {
            //Do gameplay stuff
            receiver.SetInteractionEnabled(false);
            anchor.AttachPlayer();
            PlayerData.SetPersistentCondition("MET_DITYLUM", true);
            Locator.GetShipLogManager().RevealFact("DITYLUM_MOURNING_FACT_FC");

            //Set them to meditate
            sitTime = Time.time;

            //Get ready for the end screen
            Locator.GetDeathManager().FinishedDLC();
            DeepBramble.instance.NewHorizonsAPI.SetDefaultSystem("SolarSystem");
            GameObject.Find("FlashbackCamera").transform.Find("Canvas_EchoesOver/EchoesOfTheEye").GetComponent<Text>().text = "Forgotten Castaways";

            //Disable certain effects
            Locator.GetToolModeSwapper().UnequipTool();
            Locator.GetFlashlight().TurnOff(playAudio: false);
            transform.parent.Find("lab_music").gameObject.GetComponent<OWAudioSource>().FadeOut(1);
        }

        /**
         * If destroyed, remove trigger
         */
        private void OnDestroy()
        {
            receiver.OnPressInteract -= Sit;
            dialogue.OnEndConversation -= Scream;
        }

        /**
         * After the player sits, check for when to have them get up
         */
        private void Update()
        {
            if (sitTime > 0 && Time.time > sitTime + 5)
                Locator.GetDeathManager().KillPlayer(DeathType.Meditation);
        }
    }
}
