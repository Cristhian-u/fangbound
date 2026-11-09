using Fangbound.Enemies;
using Fangbound.Player;
using UnityEngine;

namespace Fangbound.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class CombatVisualFeedback : MonoBehaviour
    {
        [SerializeField] private CombatFeedbackData feedbackData;
        [SerializeField] private SpriteRenderer signalRenderer;
        private SpriteRenderer bodyRenderer;
        private EnemyHealth health;
        private EnemyPosture posture;
        private PlayerParry parry;
        private PlayerHealth playerHealth;
        private Color normalBodyColor;
        private float damageRemaining;
        private float signalRemaining;
        private float signalDuration;
        private float signalScale;
        private bool breakSignal;
        private PlayerCombat playerCombat;
        private PlayerPogo playerPogo;
        private Collider2D playerCollider;
        private SpriteRenderer[] swingSegments;
        private SpriteRenderer[] pogoSegments;
        private SpriteRenderer pickupRenderer;
        private float pickupRemaining;
        private Vector2 swingDirection;

        public bool IsDamageFlashVisible => damageRemaining > 0f && (health == null || !health.IsDead);
        public bool IsSignalVisible => signalRenderer != null && signalRenderer.enabled;
        public int DamageFlashCount { get; private set; }
        public int ParryFeedbackCount { get; private set; }
        public int PostureBreakCount { get; private set; }
        public bool IsAttackCueVisible => SegmentsVisible(swingSegments) || SegmentsVisible(pogoSegments);
        public Vector2 AttackCueDirection => SegmentsVisible(pogoSegments) ? Vector2.down : swingDirection;
        public bool IsFangPickupFeedbackVisible => pickupRenderer != null && pickupRenderer.enabled;
        public int FangPickupFeedbackCount { get; private set; }

        private void Awake()
        {
            bodyRenderer = GetComponent<SpriteRenderer>();
            health = GetComponent<EnemyHealth>();
            posture = GetComponent<EnemyPosture>();
            parry = GetComponent<PlayerParry>();
            playerHealth = GetComponent<PlayerHealth>();
            normalBodyColor = bodyRenderer.color;
            if (feedbackData == null || signalRenderer == null)
            {
                Debug.LogError("CombatVisualFeedback necesita datos y señal visual.", this);
                enabled = false;
                return;
            }
            playerCombat = GetComponent<PlayerCombat>();
            playerPogo = GetComponent<PlayerPogo>();
            playerCollider = GetComponent<Collider2D>();
            if (playerCombat != null && playerPogo != null && playerCollider != null) CreateFangVisuals();
        }

        private void OnEnable()
        {
            if (health != null) { health.DamageTaken += HandleDamage; health.Died += HandleDeath; }
            if (posture != null) posture.Broken += HandlePostureBreak;
            if (parry != null) parry.Succeeded += HandleParry;
            if (playerHealth != null) playerHealth.Died += HandlePlayerDeath;
        }

        private void OnDisable()
        {
            if (health != null) { health.DamageTaken -= HandleDamage; health.Died -= HandleDeath; }
            if (posture != null) posture.Broken -= HandlePostureBreak;
            if (parry != null) parry.Succeeded -= HandleParry;
            if (playerHealth != null) playerHealth.Died -= HandlePlayerDeath;
            damageRemaining = signalRemaining = 0f;
            if (signalRenderer != null) signalRenderer.enabled = false;
            ApplyColors();
            ClearFangVisuals();
        }

        private void LateUpdate()
        {
            damageRemaining = Mathf.Max(0f, damageRemaining - Time.unscaledDeltaTime);
            signalRemaining = Mathf.Max(0f, signalRemaining - Time.unscaledDeltaTime);
            ApplyColors();
            signalRenderer.enabled = signalRemaining > 0f && (health == null || !health.IsDead) && (playerHealth == null || !playerHealth.IsDead);
            if (signalRenderer.enabled)
            {
                float progress = 1f - signalRemaining / signalDuration;
                signalRenderer.transform.localScale = new Vector3(signalScale, signalScale, 1f) * (1f + progress * 0.12f);
                Color color = breakSignal ? feedbackData.PostureBreakColor : feedbackData.ParryColor;
                color.a *= 1f - progress;
                signalRenderer.color = color;
            }
            UpdateFangVisuals();
        }

        private void CreateFangVisuals()
        {
            Transform root = new GameObject("Fang Visuals").transform;
            root.SetParent(transform, false);
            // Hitbox coordinates are world units, independent of the placeholder body's scale.
            Vector3 scale = transform.lossyScale;
            root.localScale = new Vector3(1f / Mathf.Max(.0001f, Mathf.Abs(scale.x)), 1f / Mathf.Max(.0001f, Mathf.Abs(scale.y)), 1f);
            swingSegments = new SpriteRenderer[4]; pogoSegments = new SpriteRenderer[4];
            for (int i = 0; i < 4; i++)
            {
                swingSegments[i] = CreateFangRenderer("Fang Swing Segment " + i, root);
                pogoSegments[i] = CreateFangRenderer("Fang Pogo Segment " + i, root);
            }
            pickupRenderer = CreateFangRenderer("Fang Pickup Pulse", root);
            pickupRenderer.sortingOrder = bodyRenderer.sortingOrder - 1;
        }

        private SpriteRenderer CreateFangRenderer(string name, Transform parent)
        {
            var renderer = new GameObject(name, typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            renderer.transform.SetParent(parent, false);
            renderer.sprite = signalRenderer.sprite;
            renderer.sharedMaterial = signalRenderer.sharedMaterial;
            renderer.sortingLayerID = bodyRenderer.sortingLayerID;
            renderer.sortingOrder = bodyRenderer.sortingOrder + 2;
            renderer.enabled = false;
            return renderer;
        }

        private void UpdateFangVisuals()
        {
            if (pickupRenderer == null) return;
            if (playerHealth != null && playerHealth.IsDead) { ClearFangVisuals(); return; }
            if (playerCombat.TryGetAttackVisual(out Vector2 center, out Vector2 size, out Vector2 direction))
            {
                swingDirection = direction;
                DrawArc(swingSegments, center, size, direction);
            }
            else HideSegments(swingSegments);
            if (playerPogo.TryGetAttackVisual(out center, out size)) DrawArc(pogoSegments, center, size, Vector2.down);
            else HideSegments(pogoSegments);

            pickupRemaining = Mathf.Max(0, pickupRemaining - Time.unscaledDeltaTime);
            pickupRenderer.enabled = pickupRemaining > 0;
            if (pickupRenderer.enabled) UpdatePickupPulse();
        }

        private void DrawArc(SpriteRenderer[] segments, Vector2 center, Vector2 size, Vector2 direction)
        {
            Vector2 side = new Vector2(-direction.y, direction.x);
            bool horizontal = Mathf.Abs(direction.x) > .5f;
            float reach = (horizontal ? size.x : size.y) * .45f;
            float spread = (horizontal ? size.y : size.x) * .65f;
            Vector2 previous = default;
            for (int i = 0; i <= segments.Length; i++)
            {
                float angle = Mathf.Lerp(-70, 70, (float)i / segments.Length) * Mathf.Deg2Rad;
                Vector2 point = center + direction * (Mathf.Cos(angle) * reach) + side * (Mathf.Sin(angle) * spread);
                if (i > 0)
                {
                    SpriteRenderer renderer = segments[i - 1];
                    Vector2 delta = point - previous, midpoint = (point + previous) * .5f;
                    renderer.transform.SetPositionAndRotation(new Vector3(midpoint.x, midpoint.y, transform.position.z),
                        Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg));
                    renderer.transform.localScale = new Vector3(delta.magnitude, feedbackData.FangAttackWidth, 1);
                    renderer.color = feedbackData.FangAttackColor;
                    renderer.enabled = true;
                }
                previous = point;
            }
        }

        public void ShowFangAcquisition()
        {
            if (!isActiveAndEnabled || pickupRenderer == null || (playerHealth != null && playerHealth.IsDead)) return;
            FangPickupFeedbackCount++;
            pickupRemaining = feedbackData.FangPickupDuration;
            pickupRenderer.enabled = true;
            UpdatePickupPulse();
        }

        private void UpdatePickupPulse()
        {
            float progress = 1 - pickupRemaining / feedbackData.FangPickupDuration;
            Vector3 center = playerCollider.bounds.center;
            pickupRenderer.transform.SetPositionAndRotation(center, Quaternion.Euler(0, 0, 45));
            pickupRenderer.transform.localScale = Vector3.one * (feedbackData.FangPickupScale * (1 + progress * .3f));
            Color color = feedbackData.FangPickupColor; color.a *= 1 - progress;
            pickupRenderer.color = color;
        }

        private static bool SegmentsVisible(SpriteRenderer[] segments) => segments != null && segments[0] != null && segments[0].enabled;
        private static void HideSegments(SpriteRenderer[] segments)
        {
            if (segments != null) foreach (SpriteRenderer renderer in segments) if (renderer != null) renderer.enabled = false;
        }
        private void ClearFangVisuals()
        {
            HideSegments(swingSegments); HideSegments(pogoSegments); pickupRemaining = 0;
            if (pickupRenderer != null) pickupRenderer.enabled = false;
        }

        private void ApplyColors()
        {
            if (bodyRenderer == null) return;
            if (health != null && health.IsDead)
            {
                bodyRenderer.color = health.DeathColor;
                if (posture != null && posture.VisualRenderer != null) posture.VisualRenderer.color = health.DeathColor;
                return;
            }
            // PlayerParry and PlayerHealth keep owning the player's body; this
            // component only owns its separate success signal.
            if (posture == null && health == null) return;
            SpriteRenderer postureRenderer = posture != null ? posture.VisualRenderer : null;
            Color bodyColor = postureRenderer == bodyRenderer ? posture.VisualColor : normalBodyColor;
            if (posture != null && posture.IsVulnerable && postureRenderer != bodyRenderer && feedbackData != null)
                bodyColor = Color.Lerp(normalBodyColor, posture.VisualColor, feedbackData.VulnerableBodyTint);
            if (damageRemaining > 0f && feedbackData != null)
                bodyColor = posture != null && posture.IsVulnerable
                    ? Color.Lerp(bodyColor, feedbackData.DamageFlashColor, feedbackData.VulnerableDamageFlash)
                    : feedbackData.DamageFlashColor;
            bodyRenderer.color = bodyColor;
            if (postureRenderer != null && postureRenderer != bodyRenderer) postureRenderer.color = posture.VisualColor;
        }

        private void HandleDamage(int damage)
        {
            if (health.IsDead) return;
            DamageFlashCount++;
            damageRemaining = feedbackData.DamageFlashDuration;
            ApplyColors();
        }

        private void HandleParry()
        {
            ParryFeedbackCount++;
            ShowSignal(false);
        }

        private void HandlePostureBreak()
        {
            PostureBreakCount++;
            ShowSignal(true);
            ApplyColors();
        }

        private void ShowSignal(bool postureBreak)
        {
            if (breakSignal && signalRemaining > 0f && !postureBreak) return;
            breakSignal = postureBreak;
            signalDuration = postureBreak ? feedbackData.PostureBreakFeedbackDuration : feedbackData.ParryFeedbackDuration;
            signalRemaining = signalDuration;
            signalScale = postureBreak ? feedbackData.PostureBreakScale : feedbackData.ParryScale;
            signalRenderer.enabled = true;
            signalRenderer.color = postureBreak ? feedbackData.PostureBreakColor : feedbackData.ParryColor;
        }

        private void HandleDeath()
        {
            damageRemaining = signalRemaining = 0f;
            signalRenderer.enabled = false;
            ApplyColors();
        }

        private void HandlePlayerDeath()
        {
            ClearFangVisuals();
            signalRemaining = 0f;
            if (signalRenderer != null) signalRenderer.enabled = false;
        }
    }
}
