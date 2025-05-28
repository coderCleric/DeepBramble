using UnityEngine;
using DeepBramble.BaseInheritors;
using DeepBramble.Helpers;

namespace DeepBramble.Triggers
{
    class BabyBiter : MonoBehaviour
    {
        private Transform fishTransform = null;
        private CharacterDialogueTree dialogue = null;
        private float biteAchievementTime = -1;

        /**
         * On start, grab the fish that we're attached to
         */
        public void Start()
        {
            this.fishTransform = this.transform.parent;
        }

        /**
         * Grant the achievement for talking to Ernesto
         */
        private void OnTalk()
        {
            AchievementHelper.GrantAchievement("FC.ERNESTO");
        }

        /**
         * When disabled, uncouple
         */
        private void OnDisable()
        {
            if (dialogue != null)
                dialogue.OnStartConversation -= OnTalk;
        }

        /**
         * When something biteable enters the bite trigger, latch onto it
         */
        private void OnTriggerEnter(Collider other)
        {
            //Gonna need the rigidbody for identification
            OWRigidbody otherBody = other.gameObject.GetAttachedOWRigidbody();

            //Only do stuff if it's the ship or the player
            if(otherBody.CompareTag("Player") || otherBody.CompareTag("Ship"))
            {
                DeepBramble.debugPrint("Either player or ship should be bitten");

                //Set the other object as our parent so we move with it
                fishTransform.SetParent(other.transform, true);

                //Destroy/disable just about everything related to the fish movement
                Component.Destroy(fishTransform.GetComponent<ImpactSensor>());
                Component.Destroy(fishTransform.GetComponent<BabyFishController>());
                Component.Destroy(fishTransform.GetComponent<OWRigidbody>());
                foreach (Collider i in fishTransform.GetComponentsInChildren<Collider>())
                    i.enabled = false;
                foreach (OWCollider i in fishTransform.GetComponentsInChildren<OWCollider>())
                    i.enabled = false;

                //Set the fish to idle and disable the brain
                fishTransform.GetComponent<BabyFishController>().ChangeState(AnglerfishController.AnglerState.Lurking);
                fishTransform.GetComponent<BabyFishController>().enabled = false;

                //Second, if it's the player, start hurting them
                if (otherBody.CompareTag("Player"))
                {
                    this.transform.parent.Find("HazardVolume").gameObject.SetActive(true);
                    biteAchievementTime = Time.time + 30;
                }

                //If it was the ship, do the ship log reveal and increment the achievement counter
                else
                {
                    Locator.GetShipLogManager().RevealFact("ANGLER_DISTRACTION_SUCCESS_FACT_FC");
                    AchievementHelper.fishLatched++;
                }

                //If it was the ship and this has a dialogue, enable the dialogue
                dialogue = fishTransform.GetComponent<BabyFishController>().dialogue;
                if (otherBody.CompareTag("Ship") && dialogue != null)
                {
                    dialogue.gameObject.SetActive(true);
                    dialogue.OnStartConversation += OnTalk;
                }
            }
        }

        /**
         * Track the timer for the achievement
         */
        private void Update()
        {
            if(biteAchievementTime > 0 && Time.time >= biteAchievementTime)
            {
                if (!Locator.GetDeathManager().IsPlayerDying() && !Locator.GetDeathManager().IsPlayerDead())
                    AchievementHelper.GrantAchievement("FC.BABY_BITE");
            }
        }
    }
}
