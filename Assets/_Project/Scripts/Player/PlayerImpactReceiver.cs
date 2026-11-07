using Fangbound.Combat;
using UnityEngine;

namespace Fangbound.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController), typeof(PlayerParry), typeof(PlayerHealth))]
    public sealed class PlayerImpactReceiver : MonoBehaviour, IIncomingAttackReceiver
    {
        private PlayerController playerController;
        private PlayerParry playerParry;
        private PlayerHealth playerHealth;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            playerParry = GetComponent<PlayerParry>();
            playerHealth = GetComponent<PlayerHealth>();
        }

        public IncomingAttackOutcome ReceiveAttack(EnemyAttackInstance attack)
        {
            if (attack == null || !attack.TryResolve())
            {
                return IncomingAttackOutcome.Ignored;
            }

            if (playerHealth.IsDead)
            {
                return IncomingAttackOutcome.IgnoredByDeath;
            }

            if (playerController.IsDashing)
            {
                return IncomingAttackOutcome.AvoidedByDash;
            }

            if (playerHealth.IsInvulnerable)
            {
                return IncomingAttackOutcome.IgnoredByInvulnerability;
            }

            if (playerParry.TryConsumeWindow())
            {
                return IncomingAttackOutcome.Parried;
            }

            if (playerHealth.TryTakeDamage(attack.Damage, attack.SourcePosition))
            {
                return IncomingAttackOutcome.Hit;
            }

            return playerHealth.IsDead
                ? IncomingAttackOutcome.IgnoredByDeath
                : IncomingAttackOutcome.IgnoredByInvulnerability;
        }
    }
}