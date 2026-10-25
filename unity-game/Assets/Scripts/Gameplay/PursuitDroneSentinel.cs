using UnityEngine;
using RooflineRelay.Character;
using RooflineRelay.Audio;

namespace RooflineRelay.Gameplay
{
    public class PursuitDroneSentinel : MonoBehaviour
    {
        [Header("Patrol Waypoints")]
        public Vector3 patrolPointA = new Vector3(-3.5f, 3.2f, 13.5f);
        public Vector3 patrolPointB = new Vector3(3.5f, 3.2f, 13.5f);
        public float patrolSpeed = 2.2f;

        [Header("Hover Motion")]
        public float bobAmplitude = 0.2f;
        public float bobFrequency = 2.4f;

        [Header("Detection Cone")]
        public float detectionRadius = 1.8f;
        public float detectionDistance = 5.0f;
        public LayerMask playerLayer = ~0;
        public Color scanColor = new Color(0.2f, 0.8f, 1.0f);
        public Color alertColor = new Color(1.0f, 0.1f, 0.1f);

        [Header("Audio")]
        public AudioClip hoverAudioClip;
        private AudioSource _audioSource;
        private Light _spotlight;

        private Vector3 _currentTarget;
        private bool _isPatrollingToB = true;
        private float _timeSinceLastDetection = 0f;

        private void Awake()
        {
            _currentTarget = patrolPointB;

            // Audio Setup
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.loop = true;
            _audioSource.spatialBlend = 1.0f; // 3D Audio
            _audioSource.minDistance = 3.0f;
            _audioSource.maxDistance = 25.0f;
            _audioSource.volume = 0.65f;
            _audioSource.playOnAwake = false;

            // Downward scanning spotlight
            var lightGo = new GameObject("Drone_Scan_Spotlight");
            lightGo.transform.SetParent(transform);
            lightGo.transform.localPosition = Vector3.down * 0.3f;
            lightGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            _spotlight = lightGo.AddComponent<Light>();
            _spotlight.type = LightType.Spot;
            _spotlight.spotAngle = 45f;
            _spotlight.range = 8.0f;
            _spotlight.color = scanColor;
            _spotlight.intensity = 2.5f;
        }

        private void Start()
        {
            if (hoverAudioClip != null)
            {
                _audioSource.clip = hoverAudioClip;
                _audioSource.Play();
            }
        }

        private void Update()
        {
            HandlePatrolMovement();
            ScanForPlayer();
        }

        private void HandlePatrolMovement()
        {
            // Move horizontally between Point A and Point B
            Vector3 targetXZ = new Vector3(_currentTarget.x, patrolPointA.y, _currentTarget.z);
            Vector3 currentXZ = new Vector3(transform.position.x, patrolPointA.y, transform.position.z);

            Vector3 moveDir = (targetXZ - currentXZ).normalized;
            transform.position += moveDir * (patrolSpeed * Time.deltaTime);

            // Add smooth vertical bobbing
            float hoverY = patrolPointA.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            transform.position = new Vector3(transform.position.x, hoverY, transform.position.z);

            // Face patrol direction
            if (moveDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 4.0f);
            }

            // Switch waypoint when close
            if (Vector3.Distance(currentXZ, targetXZ) < 0.4f)
            {
                _isPatrollingToB = !_isPatrollingToB;
                _currentTarget = _isPatrollingToB ? patrolPointB : patrolPointA;
            }
        }

        private void ScanForPlayer()
        {
            _timeSinceLastDetection += Time.deltaTime;

            // Downward sphere cast to represent conical scanning beam
            Vector3 rayOrigin = transform.position + Vector3.down * 0.2f;
            if (Physics.SphereCast(rayOrigin, detectionRadius, Vector3.down, out RaycastHit hit, detectionDistance, playerLayer))
            {
                var player = hit.collider.GetComponent<PlayerParkourController>();
                if (player != null && _timeSinceLastDetection > 2.0f)
                {
                    _timeSinceLastDetection = 0f;
                    _spotlight.color = alertColor;
                    Debug.LogWarning("[PursuitDroneSentinel] PLAYER DETECTED IN ROOFTOP GAP SCAN CONE! Intercept triggered.");

                    // Alert sound and respawn player at checkpoint
                    if (RooflineAudioManager.Instance != null)
                    {
                        RooflineAudioManager.Instance.PlayLanding(transform.position);
                    }

                    player.RespawnAtCheckpoint();
                }
            }
            else
            {
                if (_timeSinceLastDetection > 1.2f)
                {
                    _spotlight.color = scanColor;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(patrolPointA, patrolPointB);
            Gizmos.DrawWireSphere(patrolPointA, 0.4f);
            Gizmos.DrawWireSphere(patrolPointB, 0.4f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, Vector3.down * detectionDistance);
            Gizmos.DrawWireSphere(transform.position + Vector3.down * detectionDistance, detectionRadius);
        }
    }
}
