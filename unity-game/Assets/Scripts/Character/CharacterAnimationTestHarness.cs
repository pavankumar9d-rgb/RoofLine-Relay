using System.Collections;
using UnityEngine;

namespace RooflineRelay.Character
{
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimationTestHarness : MonoBehaviour
    {
        private Animator _animator;

        [Header("Animation Parameters")]
        public float speed = 0f;
        public bool isGrounded = true;
        public bool isFalling = false;

        [Header("Auto Demo")]
        public bool autoCycleDemo = false;
        public float demoCycleInterval = 2.5f;

        private float _timer = 0f;
        private int _demoStep = 0;
        private string _currentStateName = "Idle";

        private static readonly int SpeedParam = Animator.StringToHash("Speed");
        private static readonly int GroundedParam = Animator.StringToHash("Grounded");
        private static readonly int JumpParam = Animator.StringToHash("Jump");
        private static readonly int FallingParam = Animator.StringToHash("Falling");
        private static readonly int VaultParam = Animator.StringToHash("Vault");
        private static readonly int StumbleParam = Animator.StringToHash("Stumble");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            SetIdle();
        }

        private void Update()
        {
            if (autoCycleDemo)
            {
                _timer += Time.deltaTime;
                if (_timer >= demoCycleInterval)
                {
                    _timer = 0f;
                    StepDemo();
                }
            }

            // Keep animator synced with inspector fields
            _animator.SetFloat(SpeedParam, speed);
            _animator.SetBool(GroundedParam, isGrounded);
            _animator.SetBool(FallingParam, isFalling);

            // Track active clip info
            var clipInfo = _animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo != null && clipInfo.Length > 0)
            {
                _currentStateName = clipInfo[0].clip.name;
            }
        }

        public void SetIdle()
        {
            speed = 0f;
            isGrounded = true;
            isFalling = false;
            _animator.SetFloat(SpeedParam, speed);
            _animator.SetBool(GroundedParam, true);
            _animator.SetBool(FallingParam, false);
            Debug.Log("[TestHarness] State -> Idle");
        }

        public void SetWalk()
        {
            speed = 1.5f;
            isGrounded = true;
            isFalling = false;
            _animator.SetFloat(SpeedParam, speed);
            _animator.SetBool(GroundedParam, true);
            _animator.SetBool(FallingParam, false);
            Debug.Log("[TestHarness] State -> Walk (Speed: 1.5)");
        }

        public void SetRun()
        {
            speed = 5.0f;
            isGrounded = true;
            isFalling = false;
            _animator.SetFloat(SpeedParam, speed);
            _animator.SetBool(GroundedParam, true);
            _animator.SetBool(FallingParam, false);
            Debug.Log("[TestHarness] State -> Run (Speed: 5.0)");
        }

        public void TriggerJump()
        {
            isGrounded = false;
            isFalling = false;
            _animator.SetBool(GroundedParam, false);
            _animator.SetBool(FallingParam, false);
            _animator.SetTrigger(JumpParam);
            Debug.Log("[TestHarness] State -> Trigger Jump");
        }

        public void SetFalling()
        {
            isGrounded = false;
            isFalling = true;
            _animator.SetBool(GroundedParam, false);
            _animator.SetBool(FallingParam, true);
            Debug.Log("[TestHarness] State -> Falling");
        }

        public void TriggerLanding()
        {
            isGrounded = true;
            isFalling = false;
            _animator.SetBool(GroundedParam, true);
            _animator.SetBool(FallingParam, false);
            Debug.Log("[TestHarness] State -> Landed");
        }

        public void TriggerVault()
        {
            _animator.SetTrigger(VaultParam);
            Debug.Log("[TestHarness] State -> Trigger Vault");
        }

        public void TriggerStumble()
        {
            _animator.SetTrigger(StumbleParam);
            Debug.Log("[TestHarness] State -> Trigger Stumble");
        }

        private void StepDemo()
        {
            _demoStep = (_demoStep + 1) % 8;
            switch (_demoStep)
            {
                case 0: SetIdle(); break;
                case 1: SetWalk(); break;
                case 2: SetRun(); break;
                case 3: TriggerJump(); break;
                case 4: SetFalling(); break;
                case 5: TriggerLanding(); break;
                case 6: TriggerVault(); break;
                case 7: TriggerStumble(); break;
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, 240, 420), GUI.skin.box);
            GUILayout.Label("<b>Roofline Relay — Animation Test</b>");
            GUILayout.Label($"Active Clip: <b>{_currentStateName}</b>");
            GUILayout.Label($"Speed: {speed:F1} | Grounded: {isGrounded} | Falling: {isFalling}");
            GUILayout.Space(6);

            if (GUILayout.Button("Idle (Speed = 0)")) SetIdle();
            if (GUILayout.Button("Walk (Speed = 1.5)")) SetWalk();
            if (GUILayout.Button("Run (Speed = 5.0)")) SetRun();
            if (GUILayout.Button("Jump Takeoff")) TriggerJump();
            if (GUILayout.Button("In Air / Falling")) SetFalling();
            if (GUILayout.Button("Land (Grounded)")) TriggerLanding();
            if (GUILayout.Button("Vault")) TriggerVault();
            if (GUILayout.Button("Stumble")) TriggerStumble();

            GUILayout.Space(8);
            autoCycleDemo = GUILayout.Toggle(autoCycleDemo, "Auto-Cycle Demo Mode");

            GUILayout.EndArea();
        }
    }
}
