using UnityEngine;

namespace DeepBramble.BaseInheritors
{
    class PlayerExplosionController : ExplosionController
    {
		private OWAudioSource actualAudioController;

        /**
         * Special awake method to do what we need
         */
        public new void Awake()
        {
			_propID_ExplosionTime = Shader.PropertyToID("_ExplosionTime");
			_matPropBlock = new MaterialPropertyBlock();
			_matPropBlock.SetFloat(_propID_ExplosionTime, 0f);
			_light = GetComponent<Light>();
			_lightIntensity = _light.intensity;
			_lightRadius = _light.range;
			_renderer = GetComponent<MeshRenderer>();
			_renderer.enabled = false;
			_renderer.SetPropertyBlock(_matPropBlock);
			_light.enabled = false;
			_light.intensity = 0f;
			_light.range = 0.01f;
			actualAudioController = GetComponent<OWAudioSource>();
			_playing = false;
			_timer = 0f;
			_forceVolume = GetComponent<RadialForceVolume>();
			_length = 2f;
		}

		/**
		 * Very similar play, but different audio source
		 */
		public new void Play()
        {
			_forceVolume.SetVolumeActivation(active: true);
			if (Vector3.Distance(base.transform.position, Locator.GetPlayerTransform().position) < base.transform.localScale.x * GetComponent<SphereCollider>().radius)
			{
				RumbleManager.PulseShipExplode();
			}
			_renderer.enabled = true;
			_light.enabled = true;
			float num = actualAudioController.PlayOneShot(AudioType.ShipDamageShipExplosion).length;
			if (num > _length)
			{
				_length = num;
			}
			_playing = true;
			enabled = true;
		}
    }
}
