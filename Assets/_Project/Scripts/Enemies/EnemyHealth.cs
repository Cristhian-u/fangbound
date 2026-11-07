using System;
using Fangbound.Combat;
using UnityEngine;

namespace Fangbound.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(EnemyPosture))]
    public sealed class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private GroundEnemyData enemyData;

        private SpriteRenderer bodyRenderer;
        private Rigidbody2D body;
        private EnemyPosture posture;
        private CombatVisualFeedback visualFeedback;
        private Color normalColor;
        private float flashRemaining;

        public int CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0;
        public bool CanReceiveDamage => isActiveAndEnabled && !IsDead;
        public Color DeathColor => enemyData != null ? enemyData.DeathColor : Color.gray;
        public event Action<int> DamageTaken;
        public event Action Died;

        private void Awake()
        {
            bodyRenderer = GetComponent<SpriteRenderer>();
            body = GetComponent<Rigidbody2D>();
            posture = GetComponent<EnemyPosture>();
            visualFeedback = GetComponent<CombatVisualFeedback>();
            if (enemyData == null)
            {
                Debug.LogError("EnemyHealth necesita Enemy Data.", this);
                enabled = false;
                return;
            }

            normalColor = bodyRenderer.color;
            CurrentHealth = enemyData.MaximumHealth;
        }

        private void Update()
        {
            if (IsDead || flashRemaining <= 0f || (visualFeedback != null && visualFeedback.isActiveAndEnabled)) return;
            flashRemaining -= Time.deltaTime;
            if (flashRemaining <= 0f) bodyRenderer.color = normalColor;
        }

        public void TakeDamage(int damage)
        {
            if (!CanReceiveDamage || damage <= 0) return;
            int appliedDamage = damage * (posture.IsVulnerable ? enemyData.VulnerableDamageMultiplier : 1);
            CurrentHealth = Mathf.Max(0, CurrentHealth - appliedDamage);
            DamageTaken?.Invoke(appliedDamage);
            if (!IsDead)
            {
                if (visualFeedback == null || !visualFeedback.isActiveAndEnabled)
                {
                    flashRemaining = enemyData.HitFlashDuration;
                    bodyRenderer.color = Color.white;
                }
                return;
            }

            foreach (EnemyAttackHitbox hitbox in GetComponentsInChildren<EnemyAttackHitbox>()) hitbox.EndAttack();
            foreach (Collider2D collider in GetComponentsInChildren<Collider2D>()) collider.enabled = false;
            posture.enabled = false;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
            if (visualFeedback == null || !visualFeedback.isActiveAndEnabled) bodyRenderer.color = enemyData.DeathColor;
            Died?.Invoke();
        }
    }
}
