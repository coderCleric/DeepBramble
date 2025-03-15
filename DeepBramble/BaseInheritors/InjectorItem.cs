using NewHorizons.Handlers;
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
            _type = ItemType.Lantern;
        }

        /**
         * Just gives the name
         */
        public override string GetDisplayName()
        {
            return TranslationHandler.GetTranslation("Toxin Injector", TranslationHandler.TextType.UI);
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
