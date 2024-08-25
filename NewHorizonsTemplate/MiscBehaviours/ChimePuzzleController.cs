using DeepBramble.BaseInheritors;
using DeepBramble.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.MiscBehaviours
{
    public class ChimePuzzleController : MonoBehaviour
    {
        //Components it handles
        private ChimeTrigger[] chimes = new ChimeTrigger[4];
        private GravCrystalSocket[] sockets = new GravCrystalSocket[5];

        //Key for puzzle completion
        private int targetSocket = 3;
        private int[] order = new int[] {0, 3, 2};

        //Tracks puzzle state
        private int[] hits = new int[] { -1, -1, -1 };
        private int hitIndex = 0;

        /**
         * On awake, grab necessary components
         */
        private void Awake()
        {
            //Grab & assign the chimes
            foreach(OWTriggerVolume trigger in transform.Find("chimes").gameObject.GetComponentsInChildren<OWTriggerVolume>())
            {
                //Figure out which chime it is
                int index = 0;
                switch(trigger.transform.parent.name)
                {
                    case "dree_chime1":
                        index = 0;
                        break;
                    case "dree_chime2":
                        index = 1;
                        break;
                    case "dree_chime3":
                        index = 2;
                        break;
                    case "dree_chime4":
                        index = 3;
                        break;
                }

                //Make the component and add it to the array
                chimes[index] = trigger.gameObject.AddComponent<ChimeTrigger>();
                chimes[index].id = index;
                chimes[index].onChime += OnChime;
            }

            //Grab & assign the sockets
            foreach(Transform tf in transform.Find("sockets"))
            {
                //Figure out which socket it is
                int index = 0;
                switch (tf.name)
                {
                    case "empty_crystal_socket1":
                        index = 0;
                        break;
                    case "empty_crystal_socket2":
                        index = 1;
                        break;
                    case "empty_crystal_socket3":
                        index = 2;
                        break;
                    case "empty_crystal_socket4":
                        index = 3;
                        break;
                    case "empty_crystal_socket5":
                        index = 4;
                        break;
                }

                //Make the component and add it to the array
                sockets[index] = tf.gameObject.AddComponent<GravCrystalSocket>();
            }

            //Listen for crystals being removed
            sockets[targetSocket].OnSocketableRemoved += ResetProgress;
        }

        /**
         * When something is chimed, update the puzzle state
         */
        private void OnChime(int id)
        {
            //Only do stuff if the required slot has a socketed crystal
            GravCrystalItem crystal = sockets[targetSocket].GetSocketedItem() as GravCrystalItem;
            if (crystal == null)
                return;

            //First, update the state
            hits[hitIndex] = id;
            hitIndex++;
            hitIndex %= 3;

            //Then, check the state for success
            bool success = true;
            for(int i = 0; i < hits.Length; i++)
            {
                int actualI = (hitIndex + i) % 3;
                if (hits[actualI] != order[i])
                    success = false;
            }

            //If they win, do stuff
            if (success)
            {
                DeepBramble.debugPrint("Player did the chime puzzle");
                (sockets[targetSocket].GetSocketedItem() as GravCrystalItem).SetIntact(true);
                Locator.GetShipLogManager().RevealFact("CONCERT_SUCCESS_FACT_FC");
            }
        }

        /**
         * When the target socket is emptied, reset the progress
         */
        private void ResetProgress(OWItem item)
        {
            //Reset the recorded hits
            for(int i = 0; i < hits.Length; i++)
            {
                hits[i] = -1;
            }

            //Reset the index
            hitIndex = 0;
        }

        /**
         * If the component is destroyed, unlink
         */
        private void OnDestroy()
        {
            foreach(ChimeTrigger trigger in chimes)
            {
                if(trigger != null)
                    trigger.onChime -= OnChime;
            }
            sockets[targetSocket].OnSocketableRemoved -= ResetProgress;
        }
    }
}
