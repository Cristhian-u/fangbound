using UnityEngine;

namespace Fangbound.Player
{
    [CreateAssetMenu(fileName = "PlayerHealthData", menuName = "Fangbound/Player Health Data")]
    public sealed class PlayerHealthData : ScriptableObject
    {
        [SerializeField, Min(1)] private int maximumHealth = 5;
        [SerializeField, Min(0f)] private float invulnerabilityDuration = 0.8f;
        [SerializeField, Min(0f)] private float knockbackHorizontalForce = 7f;
        [SerializeField, Min(0f)] private float knockbackVerticalForce = 4.5f;
        [SerializeField, Min(0f)] private float knockbackControlDuration = 0.28f;
        [SerializeField, Min(0.01f)] private float invulnerabilityFlashInterval = 0.08f;
        [SerializeField] private Color deathColor = new Color(0.35f, 0.12f, 0.12f, 1f);

        public int MaximumHealth => maximumHealth;
        public float InvulnerabilityDuration => invulnerabilityDuration;
        public float KnockbackHorizontalForce => knockbackHorizontalForce;
        public float KnockbackVerticalForce => knockbackVerticalForce;
        public float KnockbackControlDuration => knockbackControlDuration;
        public float InvulnerabilityFlashInterval => invulnerabilityFlashInterval;
        public Color DeathColor => deathColor;
    }
}