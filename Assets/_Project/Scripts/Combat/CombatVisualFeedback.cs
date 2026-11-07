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

        public bool IsDamageFlashVisible => damageRemaining > 0f && (health == null || !health.IsDead);
        public bool IsSignalVisible => signalRenderer != null && signalRenderer.enabled;
        public int DamageFlashCount { get; private set; }
        public int ParryFeedbackCount { get; private set; }
        public int PostureBreakCount { get; private set; }

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
            }
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
            signalRemaining = 0f;
            if (signalRenderer != null) signalRenderer.enabled = false;
        }
    }
}
