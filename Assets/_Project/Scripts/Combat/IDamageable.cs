namespace Fangbound.Combat
{
    public interface IDamageable
    {
        bool CanReceiveDamage { get; }
        void TakeDamage(int damage);
    }
}