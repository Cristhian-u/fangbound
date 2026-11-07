using UnityEngine;

namespace Fangbound.Combat
{
    public sealed class EnemyAttackInstance
    {
        private bool resolved;

        public Vector2 SourcePosition { get; }
        public int Damage { get; }

        public EnemyAttackInstance(Vector2 sourcePosition, int damage)
        {
            SourcePosition = sourcePosition;
            Damage = damage;
        }

        public bool TryResolve()
        {
            if (resolved)
            {
                return false;
            }

            resolved = true;
            return true;
        }
    }
}