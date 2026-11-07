using Fangbound.Combat;
using UnityEngine;

namespace Fangbound.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController), typeof(PlayerParry))]
    public sealed class PlayerImpactReceiver : MonoBehaviour, IIncomingAttackReceiver
    {
        private PlayerController playerController;
        private PlayerParry playerParry;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            playerParry = GetComponent<PlayerParry>();
        }

        public IncomingAttackOutcome ReceiveAttack(EnemyAttackInstance attack)
        {
            if (attack == null || !attack.TryResolve())
            {
                return IncomingAttackOutcome.Ignored;
            }

            if (playerController.IsDashing)
            {
                return IncomingAttackOutcome.AvoidedByDash;
            }

            if (playerParry.TryConsumeWindow())
            {
                return IncomingAttackOutcome.Parried;
            }

            playerParry.ShowUnblockedImpact();
            return IncomingAttackOutcome.Hit;
        }
    }
}