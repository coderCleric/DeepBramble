using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.BaseInheritors
{
    class GravCrystalItem : OWItem
    {
        /**
         * Gives the display name of the item
         * 
         * @return The display name, as a string
         */
        public override string GetDisplayName()
        {
            return "Gravity Crystal";
        }

        /**
         * Check if the item can be dropped (it can't)
         * 
         * @return False, because you can't drop is, cheater
         */
        public override bool CheckIsDroppable()
        {
            return false;
        }

        /**
         * Makes a new GravCrystalItem on the given transform. Object should have the expected hierarchy
         * 
         * @param tf The transform of the base object
         */
        public static void MakeItem(Transform tf)
        {
            GravCrystalItem item = tf.gameObject.AddComponent<GravCrystalItem>();
            item._localDropOffset = new Vector3(0, -0.07f, 0);
        }
    }
}
