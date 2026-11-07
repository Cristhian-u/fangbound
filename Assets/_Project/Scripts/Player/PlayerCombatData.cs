using UnityEngine;

namespace Fangbound.Player
{
    [CreateAssetMenu(fileName = "PlayerCombatData", menuName = "Fangbound/Player Combat Data")]
    public sealed class PlayerCombatData : ScriptableObject
    {
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField, Min(0.01f)] private float attackWindow = 0.12f;

        [Header("Horizontal Attack")]
        [SerializeField] private Vector2 horizontalHitboxOffset = new Vector2(0.65f, 0.05f);
        [SerializeField] private Vector2 horizontalHitboxSize = new Vector2(0.85f, 0.55f);

        [Header("Upward Attack")]
        [SerializeField] private Vector2 upwardHitboxOffset = new Vector2(0f, 0.75f);
        [SerializeField] private Vector2 upwardHitboxSize = new Vector2(0.6f, 0.8f);

        public int Damage => damage;
        public float AttackWindow => attackWindow;
        public Vector2 HorizontalHitboxOffset => horizontalHitboxOffset;
        public Vector2 HorizontalHitboxSize => horizontalHitboxSize;
        public Vector2 UpwardHitboxOffset => upwardHitboxOffset;
        public Vector2 UpwardHitboxSize => upwardHitboxSize;
    }
}