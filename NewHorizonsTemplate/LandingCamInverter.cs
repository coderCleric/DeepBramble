using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble
{
    class LandingCamInverter : MonoBehaviour
    {
        /**
         * When the ship enters the trigger, invert the landing camera orientation
         * 
         * other: The other collider involved in the collision
         */
        private void OnTriggerEnter(Collider other)
        {
            //Need to find if the right collider entered
            if (other.gameObject.name.Equals("Cockpit_Collision_Front") && other.transform.parent.parent.parent.parent.parent.GetComponent<ShipBody>() != null)
            {
                AlignShipWithReferenceFrame shipAligner = other.transform.parent.parent.parent.parent.parent.GetComponent<AlignShipWithReferenceFrame>();
                shipAligner.SetLocalAlignmentAxis(shipAligner._localAlignmentAxis * -1);
                DeepBramble.debugPrint("Cam Inverting");
            }
        }

        /**
         * When the ship exits the trigger, reorient the landing camera orientation
         * 
         * other: The other collider involved in the collision
         */
        private void OnTriggerExit(Collider other)
        {
            //Need to find if the right collider entered
            if (other.gameObject.name.Equals("Cockpit_Collision_Front") && other.transform.parent.parent.parent.parent.parent.GetComponent<ShipBody>() != null)
            {
                AlignShipWithReferenceFrame shipAligner = other.transform.parent.parent.parent.parent.parent.GetComponent<AlignShipWithReferenceFrame>();
                shipAligner.SetLocalAlignmentAxis(shipAligner._localAlignmentAxis * -1);
                DeepBramble.debugPrint("Cam Reorienting");
            }
        }
    }
}
