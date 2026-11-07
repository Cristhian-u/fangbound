using UnityEngine;

namespace Fangbound.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyPosture))]
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
        [SerializeField] private EnemyPosture posture;
        [SerializeField] private SpriteRenderer telegraphRenderer;

        private readonly Collider2D[] hitResults = new Collider2D[16];
        private ContactFilter2D attackFilter;
        private AttackState state;
        private float stateRemaining;
        private EnemyAttackInstance currentAttack;

        private void Awake()
        {
            if (posture == null)
            {
                posture = GetComponent<EnemyPosture>();
            }

            if (attackData == null || posture == null || telegraphRenderer == null)
            {
                Debug.LogError("TrainingAttackEmitter necesita datos, postura y telegraph visual.", this);
                enabled = false;
                return;
            }

            attackFilter = ContactFilter2D.noFilter;
            attackFilter.useTriggers = true;
            state = AttackState.Waiting;
            stateRemaining = attackData.InitialDelay;
            telegraphRenderer.enabled = false;
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

        private void FixedUpdate()
        {
            if (state != AttackState.Active || currentAttack == null)
            {
                return;
            }

            Vector2 center = (Vector2)transform.position + attackData.HitboxOffset;
            int hitCount = Physics2D.OverlapBox(center, attackData.HitboxSize, 0f, attackFilter, hitResults);
            for (int index = 0; index < hitCount; index++)
            {
                Collider2D hit = hitResults[index];
                if (hit == null || hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                IIncomingAttackReceiver receiver = hit.GetComponentInParent<IIncomingAttackReceiver>();
                if (receiver == null)
                {
                    continue;
                }

                IncomingAttackOutcome outcome = receiver.ReceiveAttack(currentAttack);
                if (outcome == IncomingAttackOutcome.Parried)
                {
                    posture.TryApplyParryDamage();
                }

                if (outcome != IncomingAttackOutcome.Ignored)
                {
                    return;
                }
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
            state = AttackState.Active;
            stateRemaining = attackData.ActiveDuration;
            currentAttack = new EnemyAttackInstance(
                (Vector2)transform.position + attackData.HitboxOffset,
                attackData.Damage);
            telegraphRenderer.color = attackData.AttackColor;
        }

        private void EndAttack()
        {
            state = AttackState.Cooldown;
            stateRemaining = attackData.Cooldown;
            currentAttack = null;
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