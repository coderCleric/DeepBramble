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
        //Allows easy editing, should be removed
        public static float speed = 50;
        public static float x = 600;
        public static float y = 20;
        public static float z = 556;
        public static float totalTime = 8;
        public static float delay = 0.5f;
        public static float zRot = -10;

        public static EndSceneAddition instance;
        public bool activated = false;
        
        private void Awake()
        {
            instance = this;
            gameObject.SetActive(false);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, zRot);
        }

        public void Activate()
        {
            activated = true;
            gameObject.SetActive(true);
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
