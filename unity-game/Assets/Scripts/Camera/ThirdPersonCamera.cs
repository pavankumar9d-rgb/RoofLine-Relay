using UnityEngine;

namespace RooflineRelay.CameraControl
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target & Offsets")]
        public Transform target;
        public Vector3 targetOffset = new Vector3(0f, 1.4f, 0f);
        public float distance = 4.2f;
        public float minDistance = 1.5f;
        public float maxDistance = 6.0f;

        [Header("Smoothness")]
        public float positionSmoothTime = 0.08f;
        public float rotationSmoothSpeed = 8.0f;

        [Header("Angles")]
        public float defaultPitch = 14.0f;
        public float mouseSensitivity = 2.5f;

        private Vector3 _currentVelocity;
        private float _currentYaw = 0f;
        private float _currentPitch = 14f;

        private void Start()
        {
            if (target != null)
            {
                _currentYaw = target.eulerAngles.y;
            }
            _currentPitch = defaultPitch;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            // Optional mouse orbit
            if (Input.GetMouseButton(1))
            {
                _currentYaw += Input.GetAxis("Mouse X") * mouseSensitivity;
                _currentPitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
                _currentPitch = Mathf.Clamp(_currentPitch, -10f, 60f);
            }
            else
            {
                // Align yaw behind target when moving
                float targetYaw = target.eulerAngles.y;
                _currentYaw = Mathf.LerpAngle(_currentYaw, targetYaw, Time.deltaTime * 3.5f);
            }

            Quaternion rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
            Vector3 focusPoint = target.position + targetOffset;
            Vector3 desiredPosition = focusPoint - (rotation * Vector3.forward * distance);

            // Simple raycast collision to prevent clipping through roofs
            if (Physics.Linecast(focusPoint, desiredPosition, out RaycastHit hit))
            {
                desiredPosition = hit.point + hit.normal * 0.2f;
            }

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _currentVelocity, positionSmoothTime);
            transform.LookAt(focusPoint);
        }
    }
}
