using System.Collections.Generic;

namespace DeepBramble.Helpers
{
    /**
     * This class is responsible for properly tying signalscope signals to their originating bodies
     */
    public static class SignalHelper
    {
        private static Dictionary<string, AstroObject> signalDictionary = null;

        /**
         * Prep the dictionary for use
         */
        public static void PrepDictionary()
        {
            //Make the dictionary instance
            signalDictionary = new Dictionary<string, AstroObject>();

            //Go through each astroobject, making their dictionary entries as we go
            foreach (AstroObject i in UnityEngine.Object.FindObjectsOfType<AstroObject>())
            {
                //Switch based on the name (if they have a custom name)
                if (i._name == AstroObject.Name.CustomString)
                {
                    switch (i._customName)
                    {

                        case "Lover's Rock": //We're on The Camp Planetoid
                            signalDictionary.Add("Camp_Marker_Signal", i);
                            break;

                        case "Graviton's Folly": //We're on Graviton's Folly
                            signalDictionary.Add("Gravitation_Anomaly_Signal", i);
                            break;

                    }
                }
            }
        }

        /**
         * Fix the parents of all of the signals
         */
        public static void FixSignalParents()
        {
            //Do stuff for every signal
            foreach (AudioSignal i in UnityEngine.Object.FindObjectsOfType<AudioSignal>())
            {
                //Do nothing if the parent isn't called "Sector"
                if (i.transform.parent.name.Equals("Sector"))
                {
                    //Check if it's a name we're looking for
                    string name = i.gameObject.name;
                    if (signalDictionary.ContainsKey(name))
                    {
                        //If it is, make it a child of the astro object it leads to in the dictionary
                        i.transform.SetParent(signalDictionary[name].transform, false);
                    }
                }
            }
        }
    }
}
