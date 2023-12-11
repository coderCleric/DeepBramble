using NewHorizons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepBramble.Helpers
{
    public static class BaseSystemHelper
    {
        /**
         * Do system-wide fixes
         */
        public static void FixBaseSystem()
        {
            //If the player knows about the vessel, give them the first fact for our mod
            ForgottenLocator.revealStartingRumor = true;

            //Make sure they don't respawn in Deep Bramble
            DeepBramble.instance.NewHorizonsAPI.SetDefaultSystem("SolarSystem");
        }
    }
}
