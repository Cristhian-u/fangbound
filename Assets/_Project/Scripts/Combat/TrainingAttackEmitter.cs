using UnityEngine;

namespace Fangbound.Combat
{
    [DisallowMultipleComponent]
    public sealed class TrainingAttackEmitter : MonoBehaviour
    {
        private enum AttackState
        {
            Waiting,
            Telegraphing,
            Active,
            Cooldown
        }

        [SerializeField] private TrainingAttackData attackData;
        [SerializeField] private EnemyAttackHitbox attackHitbox;
        [SerializeField] private SpriteRenderer telegraphRenderer;

        private AttackState state;
        private float stateRemaining;

        private void Awake()
        {
            if (attackData == null || attackHitbox == null || telegraphRenderer == null)
            {
                Debug.LogError("TrainingAttackEmitter necesita datos, hitbox compartida y telegraph visual.", this);
                enabled = false;
                return;
            }

            state = AttackState.Waiting;
            stateRemaining = attackData.InitialDelay;
            telegraphRenderer.enabled = false;
        }

        private void OnDisable()
        {
            if (attackHitbox != null)
            {
                attackHitbox.EndAttack();
            }
            if (telegraphRenderer != null) telegraphRenderer.enabled = false;
        }

        private void Start()
        {
            attackHitbox.ConfigureShape(attackData.HitboxOffset, attackData.HitboxSize);
        }

        private void Update()
        {
            stateRemaining -= Time.deltaTime;
            if (stateRemaining > 0f)
            {
                return;
            }

            switch (state)
            {
                case AttackState.Waiting:
                case AttackState.Cooldown:
                    BeginTelegraph();
                    break;
                case AttackState.Telegraphing:
                    BeginAttack();
                    break;
                case AttackState.Active:
                    EndAttack();
                    break;
            }
        }

        private void BeginTelegraph()
        {
            state = AttackState.Telegraphing;
            stateRemaining = attackData.TelegraphDuration;
            telegraphRenderer.color = attackData.TelegraphColor;
            telegraphRenderer.enabled = true;
        }

        private void BeginAttack()
        {
            if (!attackHitbox.BeginAttack(attackData.Damage))
            {
                EnterCooldown();
                return;
            }

            state = AttackState.Active;
            stateRemaining = attackData.ActiveDuration;
            telegraphRenderer.color = attackData.AttackColor;
        }

        private void EndAttack()
        {
            attackHitbox.EndAttack();
            EnterCooldown();
        }

        private void EnterCooldown()
        {
            state = AttackState.Cooldown;
            stateRemaining = attackData.Cooldown;
            telegraphRenderer.enabled = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (attackData == null)
            {
                return;
            }

            Gizmos.color = attackData.TelegraphColor;
            Gizmos.DrawWireCube((Vector2)transform.position + attackData.HitboxOffset, attackData.HitboxSize);
        }
    }
}
