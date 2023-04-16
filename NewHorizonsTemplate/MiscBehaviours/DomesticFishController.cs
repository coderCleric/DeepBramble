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
        public static bool playerSpooked = false;
        private Animator animator = null;
        private AnglerfishFluidVolume killFluid = null;
        private InteractReceiver[] petZones = null;
        private OWAudioSource longRangeSource = null;
        private NoiseSensor noiseSensor = null;

        //Variables for the look behaviour
        private float lookSpeed = 30;
        private OWRigidbody lookTarget = null;
        private Vector3 originalRotation = Vector3.zero;
        private Vector3 finalStareRotation = Vector3.zero;
        private float lookStartTime = -1;
        private float stareDuration = 3;

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

            //Otherwise, do some stuff
            playerSpooked = true;
            lookTarget = noiseMaker._attachedBody;
            lookStartTime = Time.time;
            originalRotation = transform.rotation.eulerAngles;
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
         * Rotate the fishy if there's a target
         */
        private void FixedUpdate()
        {
            //Auto return if there's no target
            if (lookTarget == null)
                return;

            /*
            Vector3 targetDirection = lookTarget.transform.position - transform.position;
            targetDirection = targetDirection.normalized;
            float lookAmount = lookSpeed * Time.deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, lookAmount, 0);
            transform.LookAt(newDirection);
            */

            Vector3 targetDirection = lookTarget.transform.position - transform.position;
            Vector3 axis = Vector3.Cross(targetDirection.normalized, transform.forward).normalized;
            float angle = Vector3.SignedAngle(transform.forward, targetDirection.normalized, axis);
            angle = Mathf.Clamp(angle, -lookSpeed, lookSpeed);
            angle *= Time.deltaTime;
            transform.Rotate(axis, angle, Space.World);
        }
    }
}