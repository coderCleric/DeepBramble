using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.Helpers
{
    /**
     * this class manipulates campfires to look how we want.
     */
    public static class CampFireHelper
    {
        /**
         * Enact all of our changes on the fires
         */
        public static void PrepFires()
        {
            //Go through each fire
            Campfire[] fires = UnityEngine.Object.FindObjectsOfType<Campfire>();
            foreach (Campfire fire in fires)
            {
                //Unlight each one
                fire.SetInitialState(Campfire.State.UNLIT);

                //Change the appearance
                ChangeFireAppearance(fire);
            }
        }

        /**
         * Change a specific fire's appearance
         * 
         * @param fire The fire to change
         */
        public static void ChangeFireAppearance(Campfire fire)
        {
            Transform actualCampfire = fire.transform.parent.Find("Props_HEA_Campfire");
            if (actualCampfire != null)
            {
                //Physical fire parts
                actualCampfire.Find("Campfire_Flames").gameObject.GetComponent<MeshRenderer>().material.color = new Color(0.03f, 0.1f, 0.6f, 7f);
                actualCampfire.Find("Campfire_Embers").gameObject.SetActive(false);
                actualCampfire.Find("Campfire_Logs").gameObject.SetActive(true);
                actualCampfire.Find("Campfire_Ash").GetComponent<MeshRenderer>().material.DisableKeyword("_EMISSION");

                //The smoke column
                fire.transform.parent.Find("Effects").Find("Effects_HEA_SmokeColumn").Find("Effects_HEA_SmokeColumn_Short")
                    .gameObject.GetComponent<MeshRenderer>().material.color = new Color(3f, 3f, 3f, 1);

                //The lights
                Transform lightRoot = fire.transform.parent.Find("Effects").Find("Lights");
                lightRoot.Find("CampfireLight").GetComponent<Light>().color = new Color(1, 1, 1, 1);
                lightRoot.Find("CampfireCenterLight1").GetComponent<Light>().color = new Color(1, 1, 1, 1);
                lightRoot.Find("CampfireCenterLight2").GetComponent<Light>().color = new Color(1, 1, 1, 1);

                //The sparks
                fire.transform.parent.Find("Effects").Find("Sparks").gameObject.SetActive(false);
            }
        }
    }
}
