using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepBramble.BaseInheritors
{
    public class SignalBody : OWRigidbody
    {
        /**
         * Need to put ourselves back under the old parent and do some setup
         */
        public override void Awake()
        {
            //Set up stuff before base behaviour
            _kinematicSimulation = true;

            //Do the base behavior
            base.Awake();
        }

        /**
         * NH broke this so gotta shove this in start instead
         */
        public override void Start()
        {
            base.Start();
            transform.parent = _origParent;

            //Make it so that you can autopilot
            //Implemented this and then realized it won't work on signals in my system ):
            OWRigidbody autopilotBody = GetAutopilotBody();
            if (autopilotBody != null)
                GetReferenceFrame()._autopilotArrivalDistance = autopilotBody.GetReferenceFrame()._autopilotArrivalDistance * 1.5f;
        }

        /**
         * Finds the closest parent body that can be used as an autopilot target
         */
        private OWRigidbody GetAutopilotBody()
        {
            OWRigidbody parentBody = _origParentBody;
            while (parentBody != null)
            {
                if (parentBody.GetReferenceFrame()._autopilotArrivalDistance > 0)
                    return parentBody;
                else
                    parentBody = parentBody._origParentBody;
            }

            return null;
        }

        /**
         * Make sure that the velocity matches what it should be with the parent
         */
        private void FixedUpdate()
        {
            if(_origParentBody != null)
                SetVelocity(_origParentBody.GetPointVelocity(transform.position));
        }
    }
}
