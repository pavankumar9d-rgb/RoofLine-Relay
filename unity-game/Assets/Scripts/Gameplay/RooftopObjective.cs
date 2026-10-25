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

                // Audio feedback
                if (RooflineRelay.Audio.RooflineAudioManager.Instance != null)
                {
                    RooflineRelay.Audio.RooflineAudioManager.Instance.PlayObjective(transform.position);
                }

                // Emissive color feedback across all child meshes
                var renderers = GetComponentsInChildren<Renderer>();
                foreach (var r in renderers)
                {
                    if (r != null && r.material != null)
                    {
                        r.material.color = completeColor;
                        if (r.material.HasProperty("_EmissionColor"))
                        {
                            r.material.SetColor("_EmissionColor", completeColor * 2.5f);
                            r.material.EnableKeyword("_EMISSION");
                        }
                    }
                }

                Debug.Log("[RooftopObjective] SIGNATURE RELAY BEACON REACHED! MISSION SUCCESS!");
            }
        }
    }
}
