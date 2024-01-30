using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.Helpers
{
    /**
     * This class is responsible for making ship log entry locations properly refract through their nodes
     */
    public static class EntryLocationHelper
    {
        private static Dictionary<string, OuterFogWarpVolume> entryDictionary = null;

        /**
         * Prep the dictionary for use
         */
        public static void PrepDictionary()
        {
            //Make the dictionary instance
            entryDictionary = new Dictionary<string, OuterFogWarpVolume>();

            //Go through each astroobject, making their dictionary entries as we go
            foreach (AstroObject i in UnityEngine.Object.FindObjectsOfType<AstroObject>())
            {
                //Switch based on the name (if they have a custom name)
                if (i._name == AstroObject.Name.CustomString)
                {
                    switch (i._customName)
                    {
                        //We're on the dree planet
                        case "Dree Dimension":
                            OuterFogWarpVolume warpVolume = i.transform.Find("Sector").Find("OuterWarp").GetComponent<OuterFogWarpVolume>();
                            entryDictionary.Add("CONCERT_HALL_ENTRY_FC", warpVolume);
                            entryDictionary.Add("DREE_PARK_ENTRY_FC", warpVolume);
                            entryDictionary.Add("STORY_AREA_ENTRY_FC", warpVolume);
                            break;

                        //We're on the hot planet
                        case "Hot Dimension":
                            warpVolume = i.transform.Find("Sector").Find("OuterWarp").GetComponent<OuterFogWarpVolume>();
                            entryDictionary.Add("LANDING_ZONE_ENTRY_FC", warpVolume);
                            entryDictionary.Add("ABANDONED_SETTLEMENT_ENTRY_FC", warpVolume);
                            entryDictionary.Add("GAS_CAVE_ENTRY_FC", warpVolume);
                            entryDictionary.Add("SIGNAL_LAB_ENTRY_FC", warpVolume);
                            entryDictionary.Add("QUANTUM_CAVE_ENTRY_FC", warpVolume);
                            break;

                        //We're on the camp planet
                        case "Briar's Hollow":
                            warpVolume = i.transform.Find("Sector").Find("OuterWarp").GetComponent<OuterFogWarpVolume>();
                            entryDictionary.Add("CAMP_ENTRY_FC", warpVolume);
                            entryDictionary.Add("SOIL_LAB_FC", warpVolume);
                            entryDictionary.Add("TREEHOUSE_ENTRY_FC", warpVolume);
                            entryDictionary.Add("PLANETOID_CLUSTER_ENTRY_FC", warpVolume);
                            entryDictionary.Add("HOLE_OVERLOOK_ENTRY_FC", warpVolume);
                            entryDictionary.Add("GRAVITONS_FOLLY_ENTRY_FC", warpVolume);
                            entryDictionary.Add("PILLAR_TOWN_ENTRY_FC", warpVolume);
                            entryDictionary.Add("ALIEN_STUDY_ENTRY_FC", warpVolume);
                            entryDictionary.Add("EYE_SHRINE_ENTRY_FC", warpVolume);
                            entryDictionary.Add("POD_MEMORIAL_ENTRY_FC", warpVolume);
                            entryDictionary.Add("GRAV_CORE_ENTRY_FC", warpVolume);
                            entryDictionary.Add("CORE_LAB_ENTRY_FC", warpVolume);
                            entryDictionary.Add("QUARRY_ENTRY_FC", warpVolume);
                            break;
                    }
                }
            }
        }

        /**
         * Refracts every entry location through the warps, as dictated by the dictionary
         */
        public static void FixEntryOuterWarps()
        {
            //Do stuff for every entry
            foreach (ShipLogEntryLocation i in UnityEngine.Object.FindObjectsOfType<ShipLogEntryLocation>())
            {
                //Only do stuff if it's in the dictionary
                if (entryDictionary.ContainsKey(i.GetEntryID()))
                {
                    i._outerFogWarpVolume = entryDictionary[i.GetEntryID()];//Set it's outer fog warp to the one in the dictionary
                }
            }
        }
    }
}
