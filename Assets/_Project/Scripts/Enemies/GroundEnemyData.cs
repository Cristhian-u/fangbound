using UnityEngine;

namespace Fangbound.Enemies
{
    [CreateAssetMenu(fileName = "GroundEnemyData", menuName = "Fangbound/Ground Enemy Data")]
    public sealed class GroundEnemyData : ScriptableObject
    {
        [SerializeField, Min(1)] private int maximumHealth = 4;
        [SerializeField, Min(1)] private int vulnerableDamageMultiplier = 2;
        [SerializeField, Min(0.01f)] private float patrolSpeed = 1.3f;
        [SerializeField, Min(0.01f)] private float chaseSpeed = 2.6f;
        [SerializeField, Min(0.01f)] private float detectionRange = 3.5f;
        [SerializeField, Min(0.01f)] private float loseTargetRange = 5.5f;
        [SerializeField, Min(0.01f)] private float detectionHeight = 1.2f;
        [SerializeField, Min(0.01f)] private float attackRange = 1.3f;
        [SerializeField, Min(1)] private int attackDamage = 1;
        [SerializeField, Min(0.01f)] private float telegraphDuration = 0.65f;
        [SerializeField, Min(0.01f)] private float activeDuration = 0.14f;
        [SerializeField, Min(0.01f)] private float recoveryDuration = 1.3f;
        [SerializeField] private Vector2 hitboxOffset = new Vector2(0.85f, 0f);
        [SerializeField] private Vector2 hitboxSize = new Vector2(1.2f, 0.8f);
        [SerializeField] private Color telegraphColor = new Color(1f, 0.3f, 0.18f, 0.55f);
        [SerializeField] private Color attackColor = new Color(1f, 0.9f, 0.35f, 0.8f);
        [SerializeField, Min(0.01f)] private float hitFlashDuration = 0.1f;
        [SerializeField] private Color deathColor = new Color(0.2f, 0.2f, 0.2f, 0.4f);

        public int MaximumHealth => maximumHealth;
        public int VulnerableDamageMultiplier => vulnerableDamageMultiplier;
        public float PatrolSpeed => patrolSpeed;
        public float ChaseSpeed => chaseSpeed;
        public float DetectionRange => detectionRange;
        public float LoseTargetRange => Mathf.Max(detectionRange, loseTargetRange);
        public float DetectionHeight => detectionHeight;
        public float AttackRange => attackRange;
        public int AttackDamage => attackDamage;
        public float TelegraphDuration => telegraphDuration;
        public float ActiveDuration => activeDuration;
        public float RecoveryDuration => recoveryDuration;
        public Vector2 HitboxOffset => hitboxOffset;
        public Vector2 HitboxSize => hitboxSize;
        public Color TelegraphColor => telegraphColor;
        public Color AttackColor => attackColor;
        public float HitFlashDuration => hitFlashDuration;
        public Color DeathColor => deathColor;
    }
}
