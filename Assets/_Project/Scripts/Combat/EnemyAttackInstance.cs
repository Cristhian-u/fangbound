namespace Fangbound.Combat
{
    public sealed class EnemyAttackInstance
    {
        private bool resolved;

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