using DeepBramble.MiscBehaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.BaseInheritors
{
    public class InjectorSocket : OWItemSocket
    {

        /**
         * Need to give the transform before the base awake method (it'll always be the active transform)
         */
        public override void Awake()
        {
            _socketTransform = transform.Find("guide_transform");

            base.Awake();
        }

        /**
         * Only accepts items that are injectors
         * 
         * @param item The item to be checked
         * @return True if it is acceptable, false otherwise
         */
        public override bool AcceptsItem(OWItem item)
        {
            return item is InjectorItem;
        }

        /**
         * When something gets slotted in, need to kill the dilation node
         * 
         * @param item The item that was placed
         */
        public override bool PlaceIntoSocket(OWItem item)
        {
            bool ret = base.PlaceIntoSocket(item);
            if (ret)
            {
                DeepBramble.debugPrint("Injector socket should kill node");
                EnableInteraction(false);
                ForgottenLocator.dilationNodeKiller.KillNode();
            }
            return ret;
        }
    }
}
