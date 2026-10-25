using UnityEngine;

namespace RooflineRelay.Audio
{
    public class RooflineAudioManager : MonoBehaviour
    {
        public static RooflineAudioManager Instance { get; private set; }

        [Header("Locomotion & Parkour SFX")]
        public AudioClip footstepScuff1;
        public AudioClip footstepScuff2;
        public AudioClip jumpTakeoff;
        public AudioClip hardLanding;
        public AudioClip vaultImpact;
        public AudioClip slideSwoosh;
        public AudioClip rollImpact;

        [Header("Gameplay Events")]
        public AudioClip checkpointActivate;
        public AudioClip objectiveComplete;

        [Header("Ambience & Hazards")]
        public AudioClip droneHoverLoop;
        public AudioClip rooftopWindAmbience;

        [Header("Volume Controls")]
        [Range(0f, 1f)] public float masterVolume = 1.0f;
        [Range(0f, 1f)] public float sfxVolume = 0.85f;
        [Range(0f, 1f)] public float ambienceVolume = 0.35f;

        private AudioSource _ambienceSource;
        private AudioSource _sfx2DSource;
        private bool _footstepToggle = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Ambience Source Setup (Looping 2D)
            _ambienceSource = gameObject.AddComponent<AudioSource>();
            _ambienceSource.loop = true;
            _ambienceSource.spatialBlend = 0f;
            _ambienceSource.playOnAwake = false;
            _ambienceSource.volume = ambienceVolume * masterVolume;

            // 2D SFX Source
            _sfx2DSource = gameObject.AddComponent<AudioSource>();
            _sfx2DSource.loop = false;
            _sfx2DSource.spatialBlend = 0f;
            _sfx2DSource.playOnAwake = false;
            _sfx2DSource.volume = sfxVolume * masterVolume;
        }

        private void Start()
        {
            PlayAmbience();
        }

        public void PlayAmbience()
        {
            if (rooftopWindAmbience != null && _ambienceSource != null)
            {
                _ambienceSource.clip = rooftopWindAmbience;
                _ambienceSource.volume = ambienceVolume * masterVolume;
                _ambienceSource.Play();
            }
        }

        public void PlayFootstep(Vector3 position)
        {
            _footstepToggle = !_footstepToggle;
            AudioClip clip = _footstepToggle ? footstepScuff1 : footstepScuff2;
            PlaySfxPitchVaried(clip, position, 0.45f, 0.94f, 1.06f);
        }

        public void PlayJump(Vector3 position)
        {
            PlaySfxPitchVaried(jumpTakeoff, position, 0.75f, 0.95f, 1.05f);
        }

        public void PlayLanding(Vector3 position)
        {
            PlaySfxPitchVaried(hardLanding, position, 0.85f, 0.90f, 1.05f);
        }

        public void PlayVault(Vector3 position)
        {
            PlaySfxPitchVaried(vaultImpact, position, 0.80f, 0.95f, 1.05f);
        }

        public void PlaySlide(Vector3 position)
        {
            PlaySfxPitchVaried(slideSwoosh, position, 0.70f, 0.95f, 1.05f);
        }

        public void PlayRoll(Vector3 position)
        {
            PlaySfxPitchVaried(rollImpact, position, 0.80f, 0.92f, 1.05f);
        }

        public void PlayCheckpoint(Vector3 position)
        {
            PlaySfxPitchVaried(checkpointActivate, position, 1.0f, 1.0f, 1.0f);
        }

        public void PlayObjective(Vector3 position)
        {
            PlaySfxPitchVaried(objectiveComplete, position, 1.0f, 1.0f, 1.0f);
        }

        private void PlaySfxPitchVaried(AudioClip clip, Vector3 position, float volumeScale, float minPitch, float maxPitch)
        {
            if (clip == null) return;

            // Spawn lightweight transient audio source for clean 3D positioning and pitch variation
            GameObject tempGo = new GameObject("TempSFX_" + clip.name);
            tempGo.transform.position = position;

            AudioSource source = tempGo.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volumeScale * sfxVolume * masterVolume;
            source.pitch = Random.Range(minPitch, maxPitch);
            source.spatialBlend = 0.5f; // Hybrid spatial blend so player always hears it clearly
            source.minDistance = 3f;
            source.maxDistance = 30f;
            source.Play();

            Destroy(tempGo, clip.length / Mathf.Max(0.5f, source.pitch) + 0.1f);
        }
    }
}
