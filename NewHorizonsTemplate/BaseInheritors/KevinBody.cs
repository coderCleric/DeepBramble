namespace DeepBramble.BaseInheritors
{
    class KevinBody : OWRigidbody
    {
        /**
         * Need to put ourselves back under the old parent and do some setup
         */
        public override void Awake()
        {
            //Set up stuff before base behaviour
            _kinematicSimulation = true;
            _autoGenerateCenterOfMass = true;
            _isTargetable = false;
            _maintainOriginalCenterOfMass = true;

            //Do the base behaviour
            base.Awake();
        }

        /**
         * NH broke this so gotta shove this in start instead
         */
        public override void Start()
        {
            base.Start();
            transform.parent = _origParent;
        }
    }
}
