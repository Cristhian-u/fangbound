using UnityEngine;

namespace Fangbound.Combat
{
    [CreateAssetMenu(fileName = "TrainingAttackData", menuName = "Fangbound/Training Attack Data")]
    public sealed class TrainingAttackData : ScriptableObject
    {
        [SerializeField, Min(0f)] private float initialDelay = 0.75f;
        [SerializeField, Min(0.01f)] private float telegraphDuration = 0.65f;
        [SerializeField, Min(0.01f)] private float activeDuration = 0.14f;
        [SerializeField, Min(0.01f)] private float cooldown = 1.3f;
        [SerializeField] private Vector2 hitboxOffset = new Vector2(-1.6f, -0.05f);
        [SerializeField, Min(0.01f)] private Vector2 hitboxSize = new Vector2(3.2f, 0.75f);
        [SerializeField] private Color telegraphColor = new Color(1f, 0.3f, 0.18f, 0.8f);
        [SerializeField] private Color attackColor = new Color(1f, 0.9f, 0.35f, 0.9f);

        public float InitialDelay => initialDelay;
        public float TelegraphDuration => telegraphDuration;
        public float ActiveDuration => activeDuration;
        public float Cooldown => cooldown;
        public Vector2 HitboxOffset => hitboxOffset;
        public Vector2 HitboxSize => hitboxSize;
        public Color TelegraphColor => telegraphColor;
        public Color AttackColor => attackColor;
    }
}