using System;
using UnityEngine;

namespace Fangbound.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class EnemyAttackHitbox : MonoBehaviour
    {
        private readonly Collider2D[] hitResults = new Collider2D[16];

        private BoxCollider2D attackCollider;
        private EnemyPosture posture;
        private Transform owner;
        private ContactFilter2D hitFilter;
        private EnemyAttackInstance currentAttack;
        private float baseLocalX;

        public bool IsActive => currentAttack != null;
        public Vector2 WorldCenter => transform.TransformPoint(attackCollider.offset);
        public Vector2 WorldSize => Vector2.Scale(attackCollider.size,
            new Vector2(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y)));

        public event Action<IncomingAttackOutcome> AttackResolved;

        private void Awake()
        {
            attackCollider = GetComponent<BoxCollider2D>();
            posture = GetComponentInParent<EnemyPosture>();
            owner = posture != null ? posture.transform : transform.parent;
            attackCollider.isTrigger = true;
            attackCollider.enabled = false;
            baseLocalX = Mathf.Abs(transform.localPosition.x);
            hitFilter = ContactFilter2D.noFilter;
            hitFilter.useTriggers = true;
        }

        private void FixedUpdate()
        {
            if (currentAttack == null)
            {
                return;
            }

            int hitCount = Physics2D.OverlapBox(WorldCenter, WorldSize, transform.eulerAngles.z, hitFilter, hitResults);
            for (int index = 0; index < hitCount; index++)
            {
                Collider2D hit = hitResults[index];
                if (hit == null || (owner != null && hit.transform.IsChildOf(owner)))
                {
                    continue;
                }

                IIncomingAttackReceiver receiver = hit.GetComponentInParent<IIncomingAttackReceiver>();
                if (receiver == null || (receiver is Behaviour behaviour && !behaviour.isActiveAndEnabled))
                {
                    continue;
                }

                IncomingAttackOutcome outcome = receiver.ReceiveAttack(currentAttack);
                if (outcome == IncomingAttackOutcome.Parried && posture != null)
                {
                    posture.TryApplyParryDamage();
                }

                if (outcome != IncomingAttackOutcome.Ignored)
                {
                    ResolveAttack(outcome);
                    return;
                }
            }
        }

        public void SetFacingDirection(int direction)
        {
            Vector3 localPosition = transform.localPosition;
            localPosition.x = baseLocalX * (direction < 0 ? -1f : 1f);
            transform.localPosition = localPosition;
        }

        public void ConfigureShape(Vector2 offset, Vector2 size)
        {
            Transform parent = transform.parent;
            Vector3 parentScale = parent != null ? parent.lossyScale : Vector3.one;
            Vector3 position = transform.localPosition;
            position.x = offset.x / parentScale.x;
            position.y = offset.y / parentScale.y;
            transform.localPosition = position;
            transform.localScale = new Vector3(size.x / Mathf.Abs(parentScale.x), size.y / Mathf.Abs(parentScale.y), 1f);
            attackCollider.size = Vector2.one;
            attackCollider.offset = Vector2.zero;
            baseLocalX = Mathf.Abs(position.x);
        }

        public bool BeginAttack(int damage)
        {
            if (!isActiveAndEnabled || attackCollider == null || IsActive || damage <= 0)
            {
                return false;
            }

            // The collider defines the query shape; keeping it disabled prevents it
            // from becoming an extra hurtbox through GetComponentInParent<IDamageable>.
            currentAttack = new EnemyAttackInstance(WorldCenter, damage);
            return true;
        }

        public void EndAttack()
        {
            currentAttack = null;
            if (attackCollider != null)
            {
                attackCollider.enabled = false;
            }
        }

        private void OnDisable()
        {
            EndAttack();
        }

        private void ResolveAttack(IncomingAttackOutcome outcome)
        {
            EndAttack();
            AttackResolved?.Invoke(outcome);
        }
    }
}
