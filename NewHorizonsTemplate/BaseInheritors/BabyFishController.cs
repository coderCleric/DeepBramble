using UnityEngine;

namespace DeepBramble.BaseInheritors
{
	/**
	 * Override the Awake to not look for the angler fluid volume, since we got rid of it
	 */
    class BabyFishController : AnglerfishController
    {
		public CharacterDialogueTree dialogue = null;
		private static float turnSpeed = 180;
		private static float speed = 30;
		private static float accel = 30;

		public new void Awake()
		{
			//Not a clean way to call SectoredMonoBehaviour.Awake(), so just gonna copy+paste here
			if (_sector == null)
			{
				_sector = GetComponentInParent<Sector>();
			}
			if (_sector != null)
			{
				_sector.OnOccupantEnterSector += new OWEvent<SectorDetector>.OWCallback(OnSectorOccupantAdded);
				_sector.OnOccupantExitSector += new OWEvent<SectorDetector>.OWCallback(OnSectorOccupantRemoved);
				_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
			}

			//AnglerfishController without the fluid volume
			_anglerBody = this.GetRequiredComponent<OWRigidbody>();
			_impactSensor = this.GetRequiredComponent<ImpactSensor>();
			_noiseSensor = this.GetRequiredComponentInChildren<NoiseSensor>();
			_currentState = AnglerState.Lurking;
			_turningInPlace = false;
			_stunTimer = 0f;
			_consumeStartTime = -1f;
			_consumeComplete = false;

			//Grab & disable dialogue, if it's there
			dialogue = GetComponentInChildren<CharacterDialogueTree>();
			if(dialogue != null)
				dialogue.gameObject.SetActive(false);
		}

		/**
		 * Override OnEnable to not mess with the nonexistent angler fluid volume
		 */
		public new void OnEnable()
		{
			_impactSensor.OnImpact += OnImpact;
			_noiseSensor.OnClosestAudibleNoise += OnClosestAudibleNoise;
		}

		/**
		 * Make OnDisable not look for the fluid volume, since it doesn't exist
		 */

		private new void OnDisable()
		{
			_impactSensor.OnImpact -= OnImpact;
			_noiseSensor.OnClosestAudibleNoise -= OnClosestAudibleNoise;
		}

		/**
		 * Make a new BabyFishController, attaching it to the given object
		 * 
		 * param target The target game object
		 */
		public static void AddBabyController(GameObject target)
        {
			BabyFishController controller = target.AddComponent<BabyFishController>();
			controller._mouthOffset = new Vector3(0, 0, 0.5f);
			controller._acceleration = accel;
			controller._investigateSpeed = 5;
			controller._chaseSpeed = speed;
			controller._turnSpeed = turnSpeed;
			controller._quickTurnSpeed = 360;
			controller._arrivalDistance = 1;
			controller._pursueDistance = 45;
			controller._escapeDistance = 100;
        }
	}
}
