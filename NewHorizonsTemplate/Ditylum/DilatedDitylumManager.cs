using UnityEngine;

namespace DeepBramble.Ditylum
{
    public class DilatedDitylumManager : MonoBehaviour
    {
        /**
         * Makes Ditylum look at the player, matching their up direction
         */
        public void LookAtPlayer()
        {
            transform.LookAt(Locator.GetPlayerTransform(), Locator.GetPlayerTransform().up);
            gameObject.GetComponent<Animator>().SetTrigger("turn");
        }
    }
}
