using UnityEngine;

namespace DeepBramble.BaseInheritors
{
    public class GravCrystalItem : OWItem
    {
        public bool intact { get; private set; } = true;

        //Components
        private MeshRenderer intactRenderer;
        private MeshRenderer crackedRenderer;
        private Light light;

        /**
         * Need to give it some type or it can be placed anywhere
         */
        public override void Awake()
        {
            base.Awake();
            _type = ItemType.Lantern;

            //Grab components
            intactRenderer = transform.Find("intact_renderer").GetComponent<MeshRenderer>();
            crackedRenderer = transform.Find("cracked_renderer").GetComponent<MeshRenderer>();
            light = GetComponentInChildren<Light>();
        }

        /**
         * Sets whether or not it's intact
         */
        public void SetIntact(bool intact)
        {
            //May not need to do anything
            if (intact == this.intact)
                return;
            this.intact = intact;

            //If false, disable stuff
            if(!intact)
            {
                intactRenderer.gameObject.SetActive(false);
                crackedRenderer.gameObject.SetActive(true);
                light.enabled = false;
            }

            //If true, enable stuff
            if (intact)
            {
                intactRenderer.gameObject.SetActive(true);
                crackedRenderer.gameObject.SetActive(false);
                GetComponentInChildren<ParticleSystem>().Play();
                light.enabled = true;
            }
        }

        /**
         * Play the socketing animation
         */
        public override void PlaySocketAnimation()
        {
            GetComponentInChildren<Animator>().SetTrigger("insert");
        }

        /**
         * Gives the display name of the item
         * 
         * @return The display name, as a string
         */
        public override string GetDisplayName()
        {
            return "Gravity Crystal";
        }

        /**
         * Makes a new GravCrystalItem on the given transform. Object should have the expected hierarchy
         * 
         * @param tf The transform of the base object
         */
        public static void MakeItem(Transform tf)
        {
            GravCrystalItem item = tf.gameObject.AddComponent<GravCrystalItem>();
            item._localDropOffset = new Vector3(0, -0.07f, 0);
        }
    }
}
