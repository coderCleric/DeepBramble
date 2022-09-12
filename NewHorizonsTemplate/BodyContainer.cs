using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble
{
    public class BodyContainer
    {
        private OWRigidbody body;
        private Vector3 savedVelocity;

        /**
         * Make a new body container with the given body
         */
        public BodyContainer(OWRigidbody body)
        {
            this.body = body;
            this.savedVelocity = Vector3.zero;
        }

        /**
         * Freeze the body by setting its velocity to 0
         */
        public void freeze()
        {
            this.savedVelocity = new Vector3(body.GetVelocity().x, body.GetVelocity().y, body.GetVelocity().z);
            this.body.SetVelocity(Vector3.zero);
        }

        /**
         * Unfreezes the body by setting its velocity to the saved value
         */
        public void unfreeze()
        {
            this.body.SetVelocity(this.savedVelocity);
        }
    }
}
