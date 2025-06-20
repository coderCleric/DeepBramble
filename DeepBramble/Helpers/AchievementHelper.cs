using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepBramble.Helpers
{
    public static class AchievementHelper
    {
        public static int fishLatched = 0;
        public static int fishPetCount = 0;
        public static bool stickExtendedThisMallow = false;

        /**
         * Reset everything to default values, to be called on loop
         */
        public static void Reset()
        {
            fishLatched = 0;
            fishPetCount = 0;
            stickExtendedThisMallow = false;
        }

        /**
         * Grant the requested achievement
         */
        public static void GrantAchievement(string id)
        {
            IAchievements api = DeepBramble.instance.AchievementsAPI;
            if (api != null)
            {
                api.EarnAchievement(id);
            }
        }

        /**
         * Increment number of fish pet
         */
        public static void FishPet()
        {
            fishPetCount++;
            if (fishPetCount >= 4)
                GrantAchievement("FC.PET");
        }
    }
}
