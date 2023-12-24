using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            //Set them to meditate
            sitTime = Time.time;

            //Get ready for the end screen
            Locator.GetDeathManager().FinishedDLC();
            DeepBramble.instance.NewHorizonsAPI.SetDefaultSystem("SolarSystem");
            GameObject.Find("FlashbackCamera").transform.Find("Canvas_EchoesOver/EchoesOfTheEye").GetComponent<Text>().text = "Forgotten Castaways";
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
