using System.Collections;
using UnityEngine;

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

        [Header("Respawn & Boundaries")]
        public float killY = -8.0f;
        public Vector3 spawnPoint = new Vector3(0f, 0.5f, 0f);

        [Header("Mobile Touch Controls (Landscape)")]
        public bool enableMobileTouch = true;
        public Vector2 mobileMoveInput = Vector2.zero;
        public bool mobileSprint = false;

        // Components
        private CharacterController _characterController;
        private Animator _animator;
        private Transform _cameraTransform;

        // Movement State
        private Vector3 _velocity;
        private float _currentSpeed = 0f;
        private bool _isGrounded;
        private bool _isVaulting = false;
        private float _airTime = 0f;

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
            HandleMovement();
            HandleJumpAndGravity();
            CheckVaultObstacle();
            UpdateAnimator();
        }

        private void CheckGrounded()
        {
            _isGrounded = _characterController.isGrounded;

            if (_isGrounded)
            {
                if (_velocity.y < 0f)
                {
                    // Small negative force to keep grounded on slopes
                    _velocity.y = -2.0f;
                }

                if (_airTime > 0.45f)
                {
                    // Trigger landing on solid contact after significant fall
                    _animator.SetTrigger(HardLandingParam);
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
            // Gather Input: Desktop Keyboard + Mobile Touch
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
                // Align direction relative to camera
                Vector3 camForward = _cameraTransform != null ? _cameraTransform.forward : Vector3.forward;
                Vector3 camRight = _cameraTransform != null ? _cameraTransform.right : Vector3.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                Vector3 targetMoveDir = (camForward * inputDir.z + camRight * inputDir.x).normalized;

                // Determine target speed
                bool isSprinting = Input.GetKey(KeyCode.LeftShift) || mobileSprint;
                float targetSpeed = isSprinting ? sprintSpeed : runSpeed;
                if (inputDir.magnitude < 0.5f)
                {
                    targetSpeed = walkSpeed;
                }

                _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * 10f);

                // Smooth Rotation
                Quaternion targetRot = Quaternion.LookRotation(targetMoveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);

                // Apply Horizontal Movement
                _characterController.Move(targetMoveDir * (_currentSpeed * Time.deltaTime));
            }
            else
            {
                _currentSpeed = Mathf.Lerp(_currentSpeed, 0f, Time.deltaTime * 12f);
            }
        }

        private void HandleJumpAndGravity()
        {
            // Jump Trigger
            bool jumpRequested = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);

            if (jumpRequested && _isGrounded)
            {
                _velocity.y = jumpForce;
                _animator.SetTrigger(JumpParam);
                _isGrounded = false;
            }

            // Apply Gravity
            _velocity.y += gravity * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);
        }

        private void CheckVaultObstacle()
        {
            // Only check vault when running forward
            if (_currentSpeed < walkSpeed || !_isGrounded)
            {
                return;
            }

            Vector3 origin = transform.position + Vector3.up * 0.4f;
            Vector3 forward = transform.forward;

            // Raycast 1: Knee height
            if (Physics.Raycast(origin, forward, out RaycastHit lowHit, vaultDistance, obstacleLayer))
            {
                // Raycast 2: Head/Chest height (Must be clear for vault)
                Vector3 highOrigin = transform.position + Vector3.up * 1.5f;
                if (!Physics.Raycast(highOrigin, forward, vaultDistance, obstacleLayer))
                {
                    // Obstacle is vaultable!
                    StartCoroutine(PerformVaultRoutine(lowHit.point));
                }
            }
        }

        private IEnumerator PerformVaultRoutine(Vector3 obstaclePoint)
        {
            _isVaulting = true;
            _animator.SetTrigger(VaultParam);

            Vector3 startPos = transform.position;
            Vector3 targetPos = obstaclePoint + transform.forward * 1.5f;
            targetPos.y = startPos.y; // Keep level ground height

            float elapsed = 0f;
            while (elapsed < vaultDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / vaultDuration);

                // Arc movement over hurdle
                Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
                float arc = Mathf.Sin(t * Mathf.PI) * 0.5f;
                currentPos.y += arc;

                _characterController.enabled = false;
                transform.position = currentPos;
                _characterController.enabled = true;

                yield return null;
            }

            _isVaulting = false;
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
            Debug.Log($"[ParkourController] Player fell out of bounds. Respawning at {_lastCheckpoint}");
            _characterController.enabled = false;
            transform.position = _lastCheckpoint + Vector3.up * 0.5f;
            _velocity = Vector3.zero;
            _characterController.enabled = true;
            _animator.SetTrigger(HardLandingParam);
        }

        // GUI for Desktop & Touch Controls
        private void OnGUI()
        {
            // Top HUD
            GUI.Box(new Rect(Screen.width / 2f - 180, 10, 360, 48), "");
            GUI.Label(new Rect(Screen.width / 2f - 170, 14, 340, 24), $"<b>ROOFLINE RELAY — PROTOTYPE</b> | Speed: {_currentSpeed:F1} m/s");
            string status = _objectiveCompleted ? "<color=#00ff88>OBJECTIVE COMPLETE!</color>" : $"Checkpoints: {_checkpointsReached}/1";
            GUI.Label(new Rect(Screen.width / 2f - 170, 32, 340, 24), $"Status: {status} | Grounded: {_isGrounded}");

            // Mobile Touch Controls Overlay
            if (enableMobileTouch)
            {
                // Left Virtual D-Pad / Buttons
                float leftY = Screen.height - 150;
                if (GUI.RepeatButton(new Rect(30, leftY - 45, 60, 40), "▲ Forward")) mobileMoveInput = new Vector2(0, 1);
                else if (GUI.RepeatButton(new Rect(30, leftY + 45, 60, 40), "▼ Back")) mobileMoveInput = new Vector2(0, -1);
                else if (GUI.RepeatButton(new Rect(5, leftY, 50, 40), "◀ Left")) mobileMoveInput = new Vector2(-1, 0);
                else if (GUI.RepeatButton(new Rect(65, leftY, 50, 40), "▶ Right")) mobileMoveInput = new Vector2(1, 0);
                else mobileMoveInput = Vector2.zero;

                // Right Jump & Sprint Touch Buttons
                float rightX = Screen.width - 130;
                float rightY = Screen.height - 140;

                mobileSprint = GUI.RepeatButton(new Rect(rightX - 70, rightY + 30, 60, 60), "Sprint");

                if (GUI.Button(new Rect(rightX, rightY, 70, 70), "JUMP"))
                {
                    if (_isGrounded)
                    {
                        _velocity.y = jumpForce;
                        _animator.SetTrigger(JumpParam);
                        _isGrounded = false;
                    }
                }
            }
        }
    }
}
