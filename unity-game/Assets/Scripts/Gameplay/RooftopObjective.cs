using UnityEngine;
using RooflineRelay.Character;

namespace RooflineRelay.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class RooftopObjective : MonoBehaviour
    {
        public bool isCompleted = false;
        public float rotationSpeed = 30.0f;
        public Color completeColor = new Color(0.0f, 0.8f, 1.0f);

        private void Update()
        {
            // Subtle landmark spin for beacon
            transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isCompleted) return;

            var player = other.GetComponent<PlayerParkourController>();
            if (player != null)
            {
                isCompleted = true;
                player.CompleteObjective();

                var renderer = GetComponentInChildren<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.color = completeColor;
                }

                Debug.Log("[RooftopObjective] SIGNATURE RELAY BEACON REACHED! MISSION SUCCESS!");
            }
        }
    }
}
