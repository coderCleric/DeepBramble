using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.MiscBehaviours
{
    public class EndSceneAddition : MonoBehaviour
    {
        public static EndSceneAddition instance;
        public bool activated = false;
        private float delay = 2;
        private float speed = 5;
        
        private void Awake()
        {
            instance = this;
        }

        public void Activate()
        {
            activated = true;
        }

        private void LateUpdate()
        {
            if (activated)
            {
                //Count down if delayed
                if(delay > 0)
                {
                    delay -= Time.deltaTime;
                    return;
                }

                //If not delayed, move to the left
                transform.Translate(Vector3.left * speed * Time.deltaTime, Space.Self);
            }
        }
    }
}
