using NewHorizons.Components;
using NewHorizons.External.Modules;
using NewHorizons.Handlers;
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
        private float sitTimerLen = 10;

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
            receiver.ChangePrompt(TranslationHandler.GetTranslation("Sit", TranslationHandler.TextType.UI));
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

            //Set them to meditate
            sitTime = Time.time;

            //Disable certain effects
            Locator.GetToolModeSwapper().UnequipTool();
            Locator.GetFlashlight().TurnOff(playAudio: false);

            //These should not happen if the time loop is not active
            if (TimeLoop.IsTimeLoopEnabled())
            {
                //Reveal stuff and set conditions
                PlayerData.SetPersistentCondition("MET_DITYLUM", true);
                Locator.GetShipLogManager().RevealFact("DITYLUM_MOURNING_FACT_FC");
                Locator.GetShipLogManager().RevealFact("DITYLUM_FOUND_FACT_FC");

                //Get ready for the end screen
                Locator.GetDeathManager().FinishedDLC();
                DeepBramble.instance.NewHorizonsAPI.SetDefaultSystem("SolarSystem");
                GameObject.Find("FlashbackCamera").transform.Find("Canvas_EchoesOver/EchoesOfTheEye").GetComponent<Text>().text = TranslationHandler.GetTranslation("Forgotten Castaways", TranslationHandler.TextType.OTHER);
            }
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
            if (sitTime > 0 && Time.time > sitTime + sitTimerLen)
            {
                sitTime = -1;

                //If the time loop is inactive, activate the ending
                if(!TimeLoop.IsTimeLoopEnabled())
                {
                    GameOverModule GOModule = new GameOverModule() { text = "AT LEAST YOU’RE NOT THE ONLY ONE WHO’S THE LAST OF THEIR KIND" };
                    NHGameOverManager.Instance.StartGameOverSequence(GOModule, DeathType.Meditation, DeepBramble.instance);
                }

                //Otherwise, do the standard kill
                else
                    Locator.GetDeathManager().KillPlayer(DeathType.Meditation);
            }
        }
    }
}
