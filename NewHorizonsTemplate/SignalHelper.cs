using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble
{
    /**
     * This class is responsible for properly tying signalscope signals to their originating bodies
     */
    public  class SignalHelper
    {
        private  Dictionary<string, AstroObject> signalDictionary;

        /**
         * Make a new signal helper
         */
        public SignalHelper()
        {
            //Make the dictionary
            this.signalDictionary = null;
        }

        /**
         * Prep the dictionary for use
         */
        public void PrepDictionary()
        {
            //Make the dictionary instance
            this.signalDictionary = new Dictionary<string, AstroObject>();

            //Go through each astroobject, making their dictionary entries as we go
            foreach(AstroObject i in Component.FindObjectsOfType<AstroObject>())
            {
                //Switch based on the name (if they have a custom name)
                if(i._name == AstroObject.Name.CustomString)
                {
                    switch(i._customName)
                    {

                        case "Camp Planetoid": //We're on The Camp Planetoid
                            this.signalDictionary.Add("Signal_Camp Marker", i);
                            break;

                    }
                }
            }
        }

        /**
         * Fix the parents of all of the signals
         */
        public void FixSignalParents()
        {
            //Do stuff for every signal
            foreach (AudioSignal i in Component.FindObjectsOfType<AudioSignal>())
            {
                //Do nothing if the parent isn't called "Sector"
                if (i.transform.parent.name.Equals("Sector"))
                {
                    //Check if it's a name we're looking for
                    string name = i.gameObject.name;
                    if (this.signalDictionary.ContainsKey(name))
                    {
                        //If it is, make it a child of the astro object it leads to in the dictionary
                        i.transform.SetParent(this.signalDictionary[name].transform, false);
                    }
                }
            }
        }
    }
}
