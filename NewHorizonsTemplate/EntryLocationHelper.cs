using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble
{
    class EntryLocationHelper
    {
        private Dictionary<string, OuterFogWarpVolume> entryDictionary;

        /**
         * Make a new signal helper
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
                            this.entryDictionary.Add("PLANETOID_CLUSTER_ENTRY", i.transform.Find("Sector").Find("OuterWarp").GetComponent<OuterFogWarpVolume>());
                            this.entryDictionary.Add("TREE_SHRINE_ENTRY", i.transform.Find("Sector").Find("OuterWarp").GetComponent<OuterFogWarpVolume>());
                            break;
                    }
                }
            }
        }

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
