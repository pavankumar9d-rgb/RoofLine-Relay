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

                // Audio feedback
                if (RooflineRelay.Audio.RooflineAudioManager.Instance != null)
                {
                    RooflineRelay.Audio.RooflineAudioManager.Instance.PlayCheckpoint(transform.position);
                }

                // Emissive color feedback across all child meshes
                var renderers = GetComponentsInChildren<Renderer>();
                foreach (var r in renderers)
                {
                    if (r != null && r.material != null)
                    {
                        r.material.color = activeColor;
                        if (r.material.HasProperty("_EmissionColor"))
                        {
                            r.material.SetColor("_EmissionColor", activeColor * 2.0f);
                            r.material.EnableKeyword("_EMISSION");
                        }
                    }
                }

                Debug.Log($"[RooftopCheckpoint] Checkpoint activated by player at {transform.position}");
            }
        }
    }
}
