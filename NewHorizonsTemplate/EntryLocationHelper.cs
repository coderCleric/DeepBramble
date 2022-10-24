using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble
{
    /**
     * This class is responsible for making ship log entry locations properly refract through their nodes
     */
    class EntryLocationHelper
    {
        private Dictionary<string, OuterFogWarpVolume> entryDictionary;

        /**
         * Make a new entry location helper
         */
        public EntryLocationHelper()
        {
            //Make the dictionary
            this.entryDictionary = null;
        }

        /**
         * Prep the dictionary for use
         */
        public void PrepDictionary()
        {
            //Make the dictionary instance
            this.entryDictionary = new Dictionary<string, OuterFogWarpVolume>();

            //Go through each astroobject, making their dictionary entries as we go
            foreach (AstroObject i in Component.FindObjectsOfType<AstroObject>())
            {
                //Switch based on the name (if they have a custom name)
                if (i._name == AstroObject.Name.CustomString)
                {
                    switch (i._customName)
                    {
                        //We're in the large dimension
                        case "Large Dimension":
                            OuterFogWarpVolume warpVolume = i.transform.Find("Sector").Find("OuterWarp").GetComponent<OuterFogWarpVolume>();
                            this.entryDictionary.Add("PLANETOID_CLUSTER_ENTRY", warpVolume);
                            this.entryDictionary.Add("TREE_SHRINE_ENTRY", warpVolume);
                            this.entryDictionary.Add("CAMPSITE_ENTRY", warpVolume);
                            break;
                    }
                }
            }
        }

        /**
         * Refracts every entry location through the warps, as dictated by the dictionary
         */
        public void FixEntryOuterWarps()
        {
            //Do stuff for every entry
            foreach (ShipLogEntryLocation i in Component.FindObjectsOfType<ShipLogEntryLocation>())
            {
                //Only do stuff if it's in the dictionary
                if(this.entryDictionary.ContainsKey(i.GetEntryID()))
                {
                    i._outerFogWarpVolume = this.entryDictionary[i.GetEntryID()];//Set it's outer fog warp to the one in the dictionary
                }
            }
        }
    }
}
