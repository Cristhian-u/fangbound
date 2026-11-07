namespace Fangbound.Combat
{
    public interface IIncomingAttackReceiver
    {
        IncomingAttackOutcome ReceiveAttack(EnemyAttackInstance attack);
    }
}