using UnityEngine;
using RooflineRelay.Character;

namespace RooflineRelay.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class RooftopCheckpoint : MonoBehaviour
    {
        public bool isTriggered = false;
        public Color activeColor = new Color(0.2f, 1.0f, 0.4f);

        private void OnTriggerEnter(Collider other)
        {
            if (isTriggered) return;

            var player = other.GetComponent<PlayerParkourController>();
            if (player != null)
            {
                isTriggered = true;
                player.SetCheckpoint(transform.position);

                // Emissive color feedback
                var renderer = GetComponentInChildren<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.color = activeColor;
                }

                Debug.Log($"[RooftopCheckpoint] Checkpoint activated by player at {transform.position}");
            }
        }
    }
}
