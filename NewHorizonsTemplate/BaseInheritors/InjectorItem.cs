using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.BaseInheritors
{
    public class InjectorItem : OWItem
    {
        /**
         * Need to set up a couple of things when the object wakes up
         */
        public override void Awake()
        {
            base.Awake();
            this._localDropNormal = new Vector3(0, 0, -1);
            this._localDropOffset = new Vector3(0, 0, -0.0698f);
        }

        /**
         * Just gives the name
         */
        public override string GetDisplayName()
        {
            return "Toxin Injector";
        }

        /**
         * Play the socketing animation
         */
        public override void PlaySocketAnimation()
        {
            GetComponentInChildren<Animator>().SetTrigger("socket");
        }
    }
}
