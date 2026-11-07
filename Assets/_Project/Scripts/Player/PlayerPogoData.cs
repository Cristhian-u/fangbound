using UnityEngine;

namespace Fangbound.Player
{
    [CreateAssetMenu(fileName = "PlayerPogoData", menuName = "Fangbound/Player Pogo Data")]
    public sealed class PlayerPogoData : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float attackWindow = 0.2f;
        [SerializeField, Min(0f)] private float bounceSpeed = 13f;
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField] private Vector2 hitboxOffset = new Vector2(0f, -0.18f);
        [SerializeField] private Vector2 hitboxSize = new Vector2(0.65f, 0.35f);

        public float AttackWindow => attackWindow;
        public float BounceSpeed => bounceSpeed;
        public int Damage => damage;
        public Vector2 HitboxOffset => hitboxOffset;
        public Vector2 HitboxSize => hitboxSize;
    }
}
