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
         * Need to give it some type or it can be placed anywhere
         */
        public override void Awake()
        {
            base.Awake();
            _type = ItemType.Lantern;
        }

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
