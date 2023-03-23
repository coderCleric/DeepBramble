using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.BaseInheritors
{
    class KevinBody : OWRigidbody
    {
        /**
         * Need to put ourselves back under the old parent and do some setup
         */
        public override void Awake()
        {
            //Set up stuff before base behaviour
            _kinematicSimulation = true;
            _autoGenerateCenterOfMass = true;
            _isTargetable = false;
            _maintainOriginalCenterOfMass = true;

            //Do the base behaviour
            base.Awake();

            //Restore the parent
            transform.parent = _origParent;
        }
    }
}
