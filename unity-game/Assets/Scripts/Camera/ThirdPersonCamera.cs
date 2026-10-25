using UnityEngine;

namespace RooflineRelay.CameraControl
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target & Offsets")]
        public Transform target;
        public Vector3 targetOffset = new Vector3(0f, 1.35f, 0f);
        public float distance = 5.2f;
        public float minDistance = 2.2f;
        public float maxDistance = 6.8f;

        [Header("Smoothness & Headroom")]
        public float positionSmoothTime = 0.10f;
        public float verticalSmoothTime = 0.12f;
        public float rotationSmoothSpeed = 7.0f;

        [Header("Angles & Input")]
        public float defaultPitch = 15.0f;
        public float mouseSensitivity = 2.5f;

        [Header("Collision & Occlusion")]
        public LayerMask collisionMask = ~0;
        public float cameraRadius = 0.28f;
        public float collisionBuffer = 0.30f;

        private Vector3 _currentVelocity;
        private Vector3 _smoothedFocusPoint;
        private float _focusPointVelocityY;
        private float _currentYaw = 0f;
        private float _currentPitch = 15f;

        private void Start()
        {
            if (target != null)
            {
                _currentYaw = target.eulerAngles.y;
                _smoothedFocusPoint = target.position + targetOffset;
            }
            _currentPitch = defaultPitch;

            // Ensure Camera component has adequate FOV and near clip plane
            var cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.fieldOfView = 65f;
                cam.nearClipPlane = 0.15f;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            // Dynamic upward headroom: when character ascends (jump/vault), elevate focus to prevent upper body cropping
            float deltaY = target.position.y - (_smoothedFocusPoint.y - targetOffset.y);
            float upwardLead = deltaY > 0.05f ? Mathf.Clamp(deltaY * 0.35f, 0f, 0.50f) : 0f;

            Vector3 rawFocusPoint = target.position + targetOffset + Vector3.up * upwardLead;
            float smoothY = Mathf.SmoothDamp(_smoothedFocusPoint.y, rawFocusPoint.y, ref _focusPointVelocityY, verticalSmoothTime);
            _smoothedFocusPoint = new Vector3(rawFocusPoint.x, smoothY, rawFocusPoint.z);

            // Mouse orbit look
            if (Input.GetMouseButton(1))
            {
                _currentYaw += Input.GetAxis("Mouse X") * mouseSensitivity;
                _currentPitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
                _currentPitch = Mathf.Clamp(_currentPitch, -12f, 55f);
            }
            else
            {
                // Align yaw gently behind player when moving
                float targetYaw = target.eulerAngles.y;
                _currentYaw = Mathf.LerpAngle(_currentYaw, targetYaw, Time.deltaTime * rotationSmoothSpeed * 0.45f);
            }

            Quaternion rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
            Vector3 camDirection = -(rotation * Vector3.forward);
            Vector3 targetCamPos = _smoothedFocusPoint + camDirection * distance;

            // SphereCast collision to prevent camera from ever clipping near walls or obstacles
            float desiredDistance = distance;
            Ray ray = new Ray(_smoothedFocusPoint, camDirection);

            RaycastHit[] hits = Physics.SphereCastAll(ray, cameraRadius, distance, collisionMask, QueryTriggerInteraction.Ignore);
            float nearestValidDistance = distance;

            foreach (var hit in hits)
            {
                if (hit.transform == target || hit.transform.IsChildOf(target))
                {
                    continue;
                }

                if (hit.distance < nearestValidDistance)
                {
                    nearestValidDistance = Mathf.Max(hit.distance - collisionBuffer, minDistance);
                }
            }

            desiredDistance = Mathf.Clamp(nearestValidDistance, minDistance, maxDistance);
            Vector3 finalDesiredPos = _smoothedFocusPoint + camDirection * desiredDistance;

            // Enforce minimum floor clearance so camera can never pass through or get stuck beneath rooftop floor
            finalDesiredPos.y = Mathf.Max(finalDesiredPos.y, 0.75f);

            transform.position = Vector3.SmoothDamp(transform.position, finalDesiredPos, ref _currentVelocity, positionSmoothTime);
            transform.LookAt(_smoothedFocusPoint);
        }

        public void SnapToTarget()
        {
            if (target != null)
            {
                _currentYaw = target.eulerAngles.y;
                _smoothedFocusPoint = target.position + targetOffset;
                _currentVelocity = Vector3.zero;
                _focusPointVelocityY = 0f;

                Quaternion rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
                Vector3 camDirection = -(rotation * Vector3.forward);
                Vector3 snappedPos = _smoothedFocusPoint + camDirection * distance;
                snappedPos.y = Mathf.Max(snappedPos.y, 0.75f);
                transform.position = snappedPos;
                transform.LookAt(_smoothedFocusPoint);
            }
        }
    }
}
