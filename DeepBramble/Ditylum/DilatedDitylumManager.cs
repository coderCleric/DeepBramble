using NewHorizons.Components;
using NewHorizons.External.Modules;
using UnityEngine;

namespace DeepBramble.Ditylum
{
    public class DilatedDitylumManager : MonoBehaviour
    {
        public float supernovaTime = -1;

        /**
         * Makes Ditylum look at the player, matching their up direction
         */
        public void LookAtPlayer()
        {
            transform.LookAt(Locator.GetPlayerTransform(), Locator.GetPlayerTransform().up);
            gameObject.GetComponent<Animator>().SetTrigger("turn");
        }

        /**
         * If needed, count down to the supernova
         */
        private void Update()
        {
            if (supernovaTime > 0 && Time.time > supernovaTime)
            {
                supernovaTime = -1;
                GameOverModule GOModule = new GameOverModule() {text = "YOU WERE BOTH CONSUMED BY A FOREIGN STAR"};
                NHGameOverManager.Instance.StartGameOverSequence(GOModule, DeathType.Supernova, DeepBramble.instance);
            }
        }
    }
}
