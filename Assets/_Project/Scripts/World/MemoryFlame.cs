using System.Collections.Generic;
using Fangbound.Player;
using UnityEngine;

namespace Fangbound.World
{
    [DisallowMultipleComponent]
    public sealed class MemoryFlame : MonoBehaviour
    {
        [SerializeField] private MemoryFlameData flameData;
        [SerializeField] private Transform respawnPoint;
        [SerializeField] private SpriteRenderer flameRenderer;
        [SerializeField] private SpriteRenderer glowRenderer;
        [SerializeField] private LayerMask respawnBlockingLayers = 1;
        [SerializeField] private string checkpointId;

        private static readonly List<MemoryFlame> flames = new List<MemoryFlame>();
        private float activationRemaining;
        public bool IsCurrent { get; private set; }
        public bool IsActivationVisible => activationRemaining > 0f;
        public float InteractionRange => flameData != null ? flameData.InteractionRange : 0f;
        public string CheckpointId => checkpointId;

        public static MemoryFlame FindById(string scenePath, string id)
        {
            MemoryFlame found = null;
            foreach (MemoryFlame flame in flames)
                if (flame != null && flame.gameObject.scene.path == scenePath && flame.checkpointId == id)
                {
                    if (found != null) throw new System.InvalidOperationException("Checkpoint ID duplicado: " + id);
                    found = flame;
                }
            return found;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() => flames.Clear();

        private void Awake()
        {
            if (flameData == null || respawnPoint == null || flameRenderer == null)
            {
                Debug.LogError("MemoryFlame necesita datos, punto de reaparición y renderer de llama.", this);
                enabled = false;
                return;
            }
            RefreshVisual();
        }
        private void OnEnable() { if (!flames.Contains(this)) flames.Add(this); }
        private void OnDisable() => flames.Remove(this);

        private void Update()
        {
            activationRemaining = Mathf.Max(0f, activationRemaining - Time.unscaledDeltaTime);
            RefreshVisual();
        }

        public bool IsInRange(Vector2 position) => isActiveAndEnabled &&
            Vector2.SqrMagnitude(position - (Vector2)transform.position) <= InteractionRange * InteractionRange;

        public static MemoryFlame FindClosest(Vector2 position)
        {
            MemoryFlame closest = null;
            float bestDistance = float.PositiveInfinity;
            foreach (MemoryFlame flame in flames)
            {
                if (flame == null || !flame.IsInRange(position)) continue;
                float distance = Vector2.SqrMagnitude(position - (Vector2)flame.transform.position);
                if (distance < bestDistance) { bestDistance = distance; closest = flame; }
            }
            return closest;
        }

        public void SetCurrent(bool current)
        {
            IsCurrent = current;
            if (!current) activationRemaining = 0f;
            RefreshVisual();
        }

        public void ShowActivation()
        {
            activationRemaining = flameData.ActivationFlashDuration;
            RefreshVisual();
        }

        public bool TryGetSafePose(PlayerHealth player, out Vector3 position, out Quaternion rotation)
        {
            position = respawnPoint != null ? respawnPoint.position : transform.position;
            rotation = respawnPoint != null ? respawnPoint.rotation : Quaternion.identity;
            if (!isActiveAndEnabled || flameData == null || respawnPoint == null || player == null) return false;
            return IsSafePosition(player, position);
        }

        public bool IsSafePosition(PlayerHealth player, Vector3 position) => isActiveAndEnabled && flameData != null &&
            SpawnSafety2D.IsSafe(player, position, respawnBlockingLayers,
                flameData.SpawnClearance, flameData.GroundProbeDistance);

        private void RefreshVisual()
        {
            if (flameRenderer == null || flameData == null) return;
            Color color = activationRemaining > 0f ? flameData.ActivationColor :
                IsCurrent ? flameData.ActiveColor : flameData.InactiveColor;
            flameRenderer.color = color;
            if (glowRenderer == null) return;
            color.a = activationRemaining > 0f ? 0.5f : IsCurrent ? flameData.ActiveGlowAlpha : flameData.InactiveGlowAlpha;
            glowRenderer.color = color;
        }

        private void OnDrawGizmosSelected()
        {
            if (flameData == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, InteractionRange);
            if (respawnPoint != null) Gizmos.DrawWireCube(respawnPoint.position, new Vector3(0.7f, 1.1f, 0f));
        }
    }
}
