using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.MiscBehaviours
{
    public class DilatedDitylumManager : MonoBehaviour
    {
        /**
         * Makes Ditylum look at the player, matching their up direction
         */
        public void LookAtPlayer()
        {
            transform.LookAt(Locator.GetPlayerTransform(), Locator.GetPlayerTransform().up);
        }
    }
}
