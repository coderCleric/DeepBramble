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
                        //We're in the large dimension
                        case "Large Dimension":
                            OuterFogWarpVolume warpVolume = i.transform.Find("Sector").Find("OuterWarp").GetComponent<OuterFogWarpVolume>();
                            entryDictionary.Add("PLANETOID_CLUSTER_ENTRY", warpVolume);
                            entryDictionary.Add("TREE_SHRINE_ENTRY", warpVolume);
                            entryDictionary.Add("CAMPSITE_ENTRY", warpVolume);
                            entryDictionary.Add("SOIL_LAB_ENTRY", warpVolume);
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
