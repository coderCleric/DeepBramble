using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.MiscBehaviours
{
    public class DomesticFishController : MonoBehaviour
    {
        //Static shit
        private static bool playerSpooked = false;
        private static List<DomesticFishController> fishies = null;

        //Components
        private Animator animator = null;
        private AnglerfishFluidVolume killFluid = null;
        private InteractReceiver[] petZones = null;
        private OWAudioSource longRangeSource = null;
        private NoiseSensor noiseSensor = null;

        //Variables for the look behaviour
        private float lookSpeed = 120;
        private float lookBackTime = 3.5f;
        private OWRigidbody lookTarget = null;
        private Quaternion originalRotation = Quaternion.identity;
        private Quaternion finalStareRotation = Quaternion.identity;
        private bool lookedBack = false;
        private float lookStartTime = -1;
        private float stareDuration = 5;

        /**
         * Do some setup on awake
         */
        private void Start()
        {
            //Set up the animator properly
            animator = GetComponentInChildren<Animator>();
            animator.runtimeAnimatorController = Patches.anglerAnimator.runtimeAnimatorController;
            animator.SetFloat("MoveSpeed", 0);

            //Set up the kill zone
            killFluid = GetComponentInChildren<AnglerfishFluidVolume>();
            killFluid.OnCaughtObject += OnCaughtObject;

            //The pet zones
            petZones = GetComponentsInChildren<InteractReceiver>();
            foreach (InteractReceiver receiver in petZones)
            {
                receiver.OnPressInteract += OnPet;
                receiver.ChangePrompt("Pet");
            }

            //The noise detector
            noiseSensor = GetComponent<NoiseSensor>();
            noiseSensor.OnClosestAudibleNoise += OnClosestAudibleNoise;

            //Grab the audio source
            longRangeSource = transform.Find("AudioController/OneShotSource_LongRange").gameObject.GetComponent<OWAudioSource>();

            //Add ourselves to the static list
            fishies.Add(this);
        }

        /**
         * When the player enters the fluid, kill them
         * 
         * @param caughtBody The body that was caught
         */
        private void OnCaughtObject(OWRigidbody caughtBody)
        {
            if (caughtBody.CompareTag("Player"))
            {
                Locator.GetDeathManager().KillPlayer(DeathType.Digestion);
            }
        }

        /**
         * When the player pets the eye, disable the kill fluid and make a noise
         */
        private void OnPet()
        {
            DeepBramble.debugPrint("Eye pet");
            killFluid.gameObject.SetActive(false); 
            longRangeSource.pitch = UnityEngine.Random.Range(0.8f, 1f);
            longRangeSource.PlayOneShot(AudioType.DBAnglerfishDetectDisturbance);
        }

        /**
         * When the fish hears something, turn and look at it
         * 
         * @param noiseMaker The thing that made the noise
         */
        private void OnClosestAudibleNoise(NoiseMaker noiseMaker)
        {
            //Auto-return if the player has been spooked
            if (playerSpooked)
                return;

            //Otherwise, make all of the fish look
            playerSpooked = true;
            foreach (DomesticFishController controller in fishies)
            {
                controller.lookTarget = noiseMaker._attachedBody;
                controller.lookStartTime = Time.time;
                controller.originalRotation = controller.transform.rotation;
            }
            longRangeSource.PlayOneShot(AudioType.DBAnglerfishDetectDisturbance);

            //Enable the audio switching trigger
            transform.parent.parent.Find("audio_switcher").gameObject.SetActive(true);
        }

        /**
         * Disentangle from triggers if we're destroyed
         */
        private void OnDestroy()
        {
            killFluid.OnCaughtObject -= OnCaughtObject;
            noiseSensor.OnClosestAudibleNoise -= OnClosestAudibleNoise;
            foreach (InteractReceiver receiver in petZones)
                receiver.OnPressInteract -= OnPet;
        }

        /**
         * Resets the static things to prepare for a new loop
         */
        public static void Reset()
        {
            playerSpooked = false;
            fishies = new List<DomesticFishController>();
        }

        /**
         * Rotate the fishy if there's a target
         */
        private void FixedUpdate()
        {
            //Auto return if there's no target
            if (lookTarget == null)
                return;

            //Make it just stare at the player for a bit
            if (Time.time < lookStartTime + stareDuration)
            {
                Vector3 targetDirection = lookTarget.transform.position - transform.position;
                Vector3 axis = Vector3.Cross(targetDirection.normalized, transform.forward).normalized;
                float matchAngle = Vector3.SignedAngle(transform.forward, targetDirection.normalized, axis);
                float actualAngle = Mathf.Clamp(matchAngle, -lookSpeed * Time.deltaTime, lookSpeed * Time.deltaTime);
                transform.Rotate(axis, actualAngle, Space.World);
            }
            else
            {
                if(!lookedBack)
                {
                    lookedBack = true;
                    finalStareRotation = transform.rotation;
                }
                float lookAmount = (Time.time - (lookStartTime + stareDuration)) / lookBackTime;
                transform.rotation = Quaternion.Lerp(finalStareRotation, originalRotation, lookAmount);
            }
        }
    }
}