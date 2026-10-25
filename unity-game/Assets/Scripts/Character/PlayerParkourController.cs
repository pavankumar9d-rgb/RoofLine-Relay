using System.Collections;
using UnityEngine;
using RooflineRelay.Audio;

namespace RooflineRelay.Character
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public class PlayerParkourController : MonoBehaviour
    {
        [Header("Locomotion Speeds")]
        public float walkSpeed = 2.0f;
        public float runSpeed = 5.5f;
        public float sprintSpeed = 8.5f;
        public float rotationSpeed = 12.0f;

        [Header("Jump & Gravity")]
        public float jumpForce = 7.0f;
        public float gravity = -20.0f;
        public float groundCheckOffset = 0.2f;

        [Header("Parkour & Vaulting")]
        public float vaultDistance = 1.6f;
        public float vaultHeightMax = 1.3f;
        public float vaultHeightMin = 0.4f;
        public float vaultDuration = 0.65f;
        public LayerMask obstacleLayer = ~0;

        [Header("Parkour Polish: Slide & Roll")]
        public float slideSpeed = 7.5f;
        public float slideDuration = 0.75f;
        public float rollDuration = 0.85f;

        [Header("Respawn & Boundaries")]
        public float killY = -8.0f;
        public Vector3 spawnPoint = new Vector3(0f, 0.5f, 0f);

        [Header("Mobile Touch Controls (Landscape)")]
        public bool enableMobileTouch = true;
        public Vector2 mobileMoveInput = Vector2.zero;
        public bool mobileSprint = false;
        public bool mobileSlide = false;

        // Components
        private CharacterController _characterController;
        private Animator _animator;
        private Transform _cameraTransform;

        // Movement State
        private Vector3 _velocity;
        private float _currentSpeed = 0f;
        private bool _isGrounded;
        private bool _isVaulting = false;
        private bool _isSliding = false;
        private bool _isRolling = false;
        private float _airTime = 0f;
        private float _footstepTimer = 0f;

        // Checkpoint System
        private Vector3 _lastCheckpoint;
        private int _checkpointsReached = 0;
        private bool _objectiveCompleted = false;

        // Animator Hashes
        private static readonly int SpeedParam = Animator.StringToHash("Speed");
        private static readonly int GroundedParam = Animator.StringToHash("Grounded");
        private static readonly int JumpParam = Animator.StringToHash("Jump");
        private static readonly int FallingParam = Animator.StringToHash("Falling");
        private static readonly int VaultParam = Animator.StringToHash("Vault");
        private static readonly int SlideParam = Animator.StringToHash("Slide");
        private static readonly int RollParam = Animator.StringToHash("Roll");
        private static readonly int HardLandingParam = Animator.StringToHash("HardLanding");

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();

            // Setup CharacterController dimensions for Courier
            _characterController.height = 1.8f;
            _characterController.radius = 0.35f;
            _characterController.center = new Vector3(0f, 0.9f, 0f);

            var mainCam = Camera.main;
            if (mainCam != null)
            {
                _cameraTransform = mainCam.transform;
            }

            _lastCheckpoint = spawnPoint;
        }

        private void Start()
        {
            transform.position = spawnPoint;
        }

        private void Update()
        {
            // Fall out of bounds check
            if (transform.position.y < killY)
            {
                RespawnAtCheckpoint();
                return;
            }

            if (_isVaulting)
            {
                return;
            }

            CheckGrounded();

            if (!_isSliding)
            {
                HandleMovement();
                HandleJumpAndGravity();
                CheckVaultObstacle();
                CheckSlideInput();
            }

            UpdateFootstepAudio();
            UpdateAnimator();
        }

        private void CheckGrounded()
        {
            _isGrounded = _characterController.isGrounded;

            if (_isGrounded)
            {
                if (_velocity.y < 0f)
                {
                    _velocity.y = -2.0f;
                }

                if (_airTime > 0.40f)
                {
                    // If moving forward upon landing, fluidly roll!
                    float h = Input.GetAxisRaw("Horizontal");
                    float v = Input.GetAxisRaw("Vertical");
                    bool hasForwardInput = (new Vector2(h, v).magnitude > 0.3f) || (mobileMoveInput.magnitude > 0.3f);

                    if (hasForwardInput && !_isRolling)
                    {
                        StartCoroutine(PerformRollRoutine());
                    }
                    else
                    {
                        _animator.SetTrigger(HardLandingParam);
                        if (RooflineAudioManager.Instance != null)
                        {
                            RooflineAudioManager.Instance.PlayLanding(transform.position);
                        }
                    }
                }

                _airTime = 0f;
            }
            else
            {
                _airTime += Time.deltaTime;
            }
        }

        private void HandleMovement()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            if (mobileMoveInput.sqrMagnitude > 0.01f)
            {
                h = mobileMoveInput.x;
                v = mobileMoveInput.y;
            }

            Vector3 inputDir = new Vector3(h, 0f, v).normalized;

            if (inputDir.magnitude > 0.1f)
            {
                Vector3 camForward = _cameraTransform != null ? _cameraTransform.forward : Vector3.forward;
                Vector3 camRight = _cameraTransform != null ? _cameraTransform.right : Vector3.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                Vector3 targetMoveDir = (camForward * inputDir.z + camRight * inputDir.x).normalized;

                bool isSprinting = Input.GetKey(KeyCode.LeftShift) || mobileSprint;
                float targetSpeed = isSprinting ? sprintSpeed : runSpeed;
                if (inputDir.magnitude < 0.5f)
                {
                    targetSpeed = walkSpeed;
                }

                _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * 10f);

                Quaternion targetRot = Quaternion.LookRotation(targetMoveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);

                _characterController.Move(targetMoveDir * (_currentSpeed * Time.deltaTime));
            }
            else
            {
                _currentSpeed = Mathf.Lerp(_currentSpeed, 0f, Time.deltaTime * 12f);
            }
        }

        private void HandleJumpAndGravity()
        {
            bool jumpRequested = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);

            if (jumpRequested && _isGrounded)
            {
                _velocity.y = jumpForce;
                _animator.SetTrigger(JumpParam);
                _isGrounded = false;

                if (RooflineAudioManager.Instance != null)
                {
                    RooflineAudioManager.Instance.PlayJump(transform.position);
                }
            }

            _velocity.y += gravity * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);
        }

        private void CheckVaultObstacle()
        {
            if (_currentSpeed < walkSpeed || !_isGrounded)
            {
                return;
            }

            Vector3 origin = transform.position + Vector3.up * 0.4f;
            Vector3 forward = transform.forward;

            if (Physics.Raycast(origin, forward, out RaycastHit lowHit, vaultDistance, obstacleLayer))
            {
                Vector3 highOrigin = transform.position + Vector3.up * 1.5f;
                if (!Physics.Raycast(highOrigin, forward, vaultDistance, obstacleLayer))
                {
                    StartCoroutine(PerformVaultRoutine(lowHit.point));
                }
            }
        }

        private void CheckSlideInput()
        {
            bool slideRequested = Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl) || mobileSlide;
            mobileSlide = false;

            if (slideRequested && _isGrounded && _currentSpeed > 3.0f && !_isSliding)
            {
                StartCoroutine(PerformSlideRoutine());
            }
        }

        private IEnumerator PerformSlideRoutine()
        {
            _isSliding = true;
            _animator.SetTrigger(SlideParam);

            if (RooflineAudioManager.Instance != null)
            {
                RooflineAudioManager.Instance.PlaySlide(transform.position);
            }

            // Lower capsule height for low hurdle clearance
            _characterController.height = 0.85f;
            _characterController.center = new Vector3(0f, 0.425f, 0f);

            float elapsed = 0f;
            Vector3 slideDir = transform.forward;
            float initialSpeed = Mathf.Max(_currentSpeed, slideSpeed);

            while (elapsed < slideDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / slideDuration;

                // Friction deceleration
                float currentSlideSpeed = Mathf.Lerp(initialSpeed, walkSpeed * 0.5f, t);
                _characterController.Move(slideDir * (currentSlideSpeed * Time.deltaTime));

                // Apply gravity while sliding
                _velocity.y += gravity * Time.deltaTime;
                _characterController.Move(_velocity * Time.deltaTime);

                yield return null;
            }

            // Headroom check before standing
            while (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.up, 1.4f, obstacleLayer))
            {
                _characterController.Move(slideDir * (walkSpeed * Time.deltaTime));
                yield return null;
            }

            // Restore capsule height
            _characterController.height = 1.8f;
            _characterController.center = new Vector3(0f, 0.9f, 0f);
            _isSliding = false;
        }

        private IEnumerator PerformRollRoutine()
        {
            _isRolling = true;
            _animator.SetTrigger(RollParam);

            if (RooflineAudioManager.Instance != null)
            {
                RooflineAudioManager.Instance.PlayRoll(transform.position);
            }

            float elapsed = 0f;
            Vector3 rollDir = transform.forward;

            while (elapsed < rollDuration)
            {
                elapsed += Time.deltaTime;
                _characterController.Move(rollDir * (runSpeed * 0.8f * Time.deltaTime));

                _velocity.y = -2.0f;
                _characterController.Move(_velocity * Time.deltaTime);

                yield return null;
            }

            _isRolling = false;
        }

        private IEnumerator PerformVaultRoutine(Vector3 obstaclePoint)
        {
            _isVaulting = true;
            _animator.SetTrigger(VaultParam);

            if (RooflineAudioManager.Instance != null)
            {
                RooflineAudioManager.Instance.PlayVault(transform.position);
            }

            Vector3 startPos = transform.position;
            Vector3 targetPos = obstaclePoint + transform.forward * 1.5f;
            targetPos.y = startPos.y;

            _characterController.enabled = false;

            float elapsed = 0f;
            while (elapsed < vaultDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / vaultDuration);

                Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
                float arc = Mathf.Sin(t * Mathf.PI) * 0.5f;
                currentPos.y += arc;

                transform.position = currentPos;
                yield return null;
            }

            transform.position = targetPos;
            _characterController.enabled = true;
            _isVaulting = false;
        }

        private void UpdateFootstepAudio()
        {
            if (!_isGrounded || _isVaulting || _isSliding || _currentSpeed < 0.3f)
            {
                _footstepTimer = 0f;
                return;
            }

            _footstepTimer += Time.deltaTime;
            float stepInterval = _currentSpeed > 6.0f ? 0.28f : (_currentSpeed > 3.0f ? 0.38f : 0.52f);

            if (_footstepTimer >= stepInterval)
            {
                _footstepTimer = 0f;
                if (RooflineAudioManager.Instance != null)
                {
                    RooflineAudioManager.Instance.PlayFootstep(transform.position);
                }
            }
        }

        private void UpdateAnimator()
        {
            _animator.SetFloat(SpeedParam, _currentSpeed);
            _animator.SetBool(GroundedParam, _isGrounded);
            _animator.SetBool(FallingParam, !_isGrounded && _velocity.y < -2.5f);
        }

        public void SetCheckpoint(Vector3 newCheckpoint)
        {
            _lastCheckpoint = newCheckpoint;
            _checkpointsReached++;
            Debug.Log($"[ParkourController] Checkpoint #{_checkpointsReached} registered at {newCheckpoint}");
        }

        public void CompleteObjective()
        {
            _objectiveCompleted = true;
            Debug.Log("[ParkourController] RELAY OBJECTIVE COMPLETED!");
        }

        public void RespawnAtCheckpoint()
        {
            Debug.Log($"[ParkourController] Player fell out of bounds or caught by sentinel. Respawning at {_lastCheckpoint}");
            _characterController.enabled = false;
            transform.position = _lastCheckpoint + Vector3.up * 0.5f;
            _velocity = Vector3.zero;
            _currentSpeed = 0f;
            _characterController.enabled = true;
            _animator.SetTrigger(HardLandingParam);

            // Snap camera immediately to prevent dragging from beneath the floor
            if (_cameraTransform != null)
            {
                var tpCam = _cameraTransform.GetComponent<RooflineRelay.CameraControl.ThirdPersonCamera>();
                if (tpCam != null)
                {
                    tpCam.SnapToTarget();
                }
            }

            if (RooflineAudioManager.Instance != null)
            {
                RooflineAudioManager.Instance.PlayLanding(transform.position);
            }
        }

        private void OnGUI()
        {
            // Top HUD
            GUI.Box(new Rect(Screen.width / 2f - 200, 10, 400, 50), "");
            GUI.Label(new Rect(Screen.width / 2f - 190, 14, 380, 24), $"<b>ROOFLINE RELAY — PROTOTYPE</b> | Speed: {_currentSpeed:F1} m/s");
            string status = _objectiveCompleted ? "<color=#00ff88>OBJECTIVE COMPLETE!</color>" : $"Checkpoints: {_checkpointsReached}/1";
            GUI.Label(new Rect(Screen.width / 2f - 190, 32, 380, 24), $"Status: {status} | Grounded: {_isGrounded} | State: {(_isSliding ? "SLIDE" : (_isRolling ? "ROLL" : (_isVaulting ? "VAULT" : "RUN")))}");

            // Mobile Touch Controls Overlay
            if (enableMobileTouch)
            {
                // Left Virtual D-Pad
                float leftY = Screen.height - 150;
                if (GUI.RepeatButton(new Rect(30, leftY - 45, 60, 40), "▲ Up")) mobileMoveInput = new Vector2(0, 1);
                else if (GUI.RepeatButton(new Rect(30, leftY + 45, 60, 40), "▼ Down")) mobileMoveInput = new Vector2(0, -1);
                else if (GUI.RepeatButton(new Rect(5, leftY, 50, 40), "◀ Left")) mobileMoveInput = new Vector2(-1, 0);
                else if (GUI.RepeatButton(new Rect(65, leftY, 50, 40), "▶ Right")) mobileMoveInput = new Vector2(1, 0);
                else mobileMoveInput = Vector2.zero;

                // Right Jump, Sprint, Slide Touch Buttons
                float rightX = Screen.width - 200;
                float rightY = Screen.height - 140;

                mobileSprint = GUI.RepeatButton(new Rect(rightX, rightY + 30, 60, 50), "Sprint");

                if (GUI.Button(new Rect(rightX + 65, rightY + 30, 55, 50), "Slide"))
                {
                    mobileSlide = true;
                }

                if (GUI.Button(new Rect(rightX + 125, rightY, 65, 80), "JUMP"))
                {
                    if (_isGrounded)
                    {
                        _velocity.y = jumpForce;
                        _animator.SetTrigger(JumpParam);
                        _isGrounded = false;
                        if (RooflineAudioManager.Instance != null)
                        {
                            RooflineAudioManager.Instance.PlayJump(transform.position);
                        }
                    }
                }
            }
        }
    }
}
