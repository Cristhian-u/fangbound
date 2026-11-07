using UnityEngine;

namespace Fangbound.World
{
    [CreateAssetMenu(fileName = "MemoryFlameData", menuName = "Fangbound/Memory Flame Data")]
    public sealed class MemoryFlameData : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float interactionRange = 1.3f;
        [SerializeField, Min(0.01f)] private float activationFlashDuration = 0.3f;
        [SerializeField] private Color inactiveColor = new Color(0.3f, 0.65f, 0.6f, 0.4f);
        [SerializeField] private Color activeColor = new Color(0.65f, 1f, 0.94f, 1f);
        [SerializeField] private Color activationColor = Color.white;
        [SerializeField, Range(0f, 1f)] private float inactiveGlowAlpha = 0.08f;
        [SerializeField, Range(0f, 1f)] private float activeGlowAlpha = 0.3f;
        [SerializeField, Min(0.01f)] private float spawnClearance = 0.02f;
        [SerializeField, Min(0.01f)] private float groundProbeDistance = 0.2f;

        public float InteractionRange => interactionRange;
        public float ActivationFlashDuration => activationFlashDuration;
        public Color InactiveColor => inactiveColor;
        public Color ActiveColor => activeColor;
        public Color ActivationColor => activationColor;
        public float InactiveGlowAlpha => inactiveGlowAlpha;
        public float ActiveGlowAlpha => activeGlowAlpha;
        public float SpawnClearance => spawnClearance;
        public float GroundProbeDistance => groundProbeDistance;
    }
}
