using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble
{
    /**
     * This class allows bramble dimensions to grab floating objects, ensuring they don't float out.
     */
    public class BrambleContainer
    {
        //Static
        private static BrambleContainer activeDimension = null;
        public static List<BrambleContainer> containers = new List<BrambleContainer>();

        //Instanced
        private GameObject dimensionBody;
        private bool shouldLoad;
        private List<BodyContainer> rigidBodies;

        /**
         * Make a new Bramble Container
         * 
         * @param dimensionBody The bramble dimension body that this container should contain
         * @param bodyNames The names of all of the floating objects this dimension should grab
         */
        public BrambleContainer(GameObject dimensionBody, string[] bodyNames)
        {
            //Initialize attributes
            this.dimensionBody = dimensionBody;
            this.shouldLoad = false;

            //Grab the bodies
            this.grabBodies(bodyNames);

            //Add to the list
            containers.Add(this);

            //Sleep everything in the dimension
            this.sleepBodies();
        }

        /**
         * Make a new Bramble Container, say whether or not it should be loaded
         * 
         * @param dimensionBody The bramble dimension body that this container should contain
         * @param bodyNames The names of all of the floating objects this dimension should grab
         * @param loadByDefault True if this dimension should be considered loaded by default, false otherwise
         */
        public BrambleContainer(GameObject dimensionBody, string[] bodyNames, bool loadByDefault)
        {
            //Initialize attributes
            this.dimensionBody = dimensionBody;
            this.shouldLoad = loadByDefault;

            //Grab the bodies
            this.grabBodies(bodyNames);

            //Add to the list
            containers.Add(this);

            //Make this the active dimension
            activeDimension = this;
        }

        /**
         * Find the rigidbodies with the needed names, add them to our list
         * 
         * @param names The array of names of objects we should grab and stuff in containers
         */
        public void grabBodies(string[] names)
        {
            this.rigidBodies = new List<BodyContainer>();

            //Go through every OWRigidbody
            foreach(OWRigidbody i in Component.FindObjectsOfType<OWRigidbody>())
            {
                foreach (string name in names) {
                    if (name.Equals(i.gameObject.name)) {
                        //If we want it, grab it
                        this.rigidBodies.Add(new BodyContainer(i));

                        //Set it to simulate in this sector
                        i._simulateInSector = this.dimensionBody.transform.GetComponentInChildren<Sector>();

                        DeepBramble.debugPrint(dimensionBody.name + " grabbed " + i.gameObject.name);
                    }
                }
            }
        }

        /**
         * Put all of the bodies in the dimension to sleep
         */
        public void sleepBodies()
        {
            foreach(BodyContainer i in this.rigidBodies)
            {
                i.freeze();
            }
        }

        /**
         * Wakes all of the bodies in the dimension
         */
        public void wakeBodies()
        {
            foreach (BodyContainer i in this.rigidBodies)
            {
                i.unfreeze();
            }
        }

        /**
         * Sets the given dimension as the active dimension. Automatically wakes up this dimension, and puts the other one to sleep
         * 
         * @param dimension The dimension that should be made active
         */
        public static void setActiveDimension(GameObject dimension)
        {
            //Skip the sleeping if there is no active dimension
            if (activeDimension != null)
                activeDimension.sleepBodies();

            //Find the container for the provided dimension
            foreach(BrambleContainer i in containers)
            {
                if(i.dimensionBody == dimension)
                {
                    i.wakeBodies();
                    activeDimension = i;
                    return;
                }
            }

            //We entered no recorded dimension, set activedimension to null
            activeDimension = null;
        }

        /**
         * Clears the active dimension and all dimension containers
         */
        public static void clear()
        {
            activeDimension = null;
            containers = new List<BrambleContainer>();
        }
    }
}
