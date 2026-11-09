using System.Collections.Generic;
using Fangbound.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fangbound.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController), typeof(Collider2D))]
    public sealed class PlayerPogo : MonoBehaviour
    {
        [SerializeField] private PlayerPogoData pogoData;
        [SerializeField] private InputActionAsset inputActions;

        private readonly Collider2D[] hitResults = new Collider2D[16];
        private readonly HashSet<PogoTarget> hitTargets = new HashSet<PogoTarget>();
        private PlayerController controller;
        private PlayerAbilities abilities;
        private Collider2D bodyCollider;
        private InputAction moveAction;
        private InputAction attackAction;
        private ContactFilter2D hitFilter;
        private bool attackRequested;
        private bool attackAvailable = true;
        private float attackTimeRemaining;
        private int visualStartFrame = -1;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            abilities = GetComponent<PlayerAbilities>();
            bodyCollider = GetComponent<Collider2D>();

            if (pogoData == null || inputActions == null)
            {
                Debug.LogError("PlayerPogo necesita Pogo Data e Input Actions.", this);
                enabled = false;
                return;
            }

            InputActionMap playerMap = inputActions.FindActionMap("Player", true);
            moveAction = playerMap.FindAction("Move", true);
            attackAction = playerMap.FindAction("Attack", true);
            hitFilter = ContactFilter2D.noFilter;
            hitFilter.useTriggers = true;
        }

        private void OnDisable()
        {
            visualStartFrame = -1;
            attackRequested = false;
            attackTimeRemaining = 0f;
        }

        private void Update()
        {
            if ((abilities == null || abilities.FangOwned) && attackAction != null && !controller.IsControlLocked && attackAction.WasPressedThisFrame() &&
                moveAction.ReadValue<Vector2>().y < -0.5f)
            {
                attackRequested = true;
            }
        }

        public void ResetForRespawn()
        {
            visualStartFrame = -1;
            attackRequested = false;
            attackAvailable = true;
            attackTimeRemaining = 0f;
            hitTargets.Clear();
        }

        private void FixedUpdate()
        {
            if ((abilities != null && !abilities.FangOwned) || controller.IsControlLocked)
            {
                attackRequested = false;
                attackTimeRemaining = 0f;
                return;
            }

            if (controller.IsGrounded)
            {
                attackAvailable = true;
                attackTimeRemaining = 0f;
            }

            if (attackRequested)
            {
                attackRequested = false;
                if (attackAvailable && controller.TryStartPogoDive())
                {
                    attackAvailable = false;
                    attackTimeRemaining = pogoData.AttackWindow;
                    visualStartFrame = Time.frameCount;
                    hitTargets.Clear();
                }
            }

            if (attackTimeRemaining <= 0f || controller.IsDashing || controller.IsGrounded)
            {
                attackTimeRemaining = 0f;
                return;
            }

            Vector2 center = GetHitboxCenter();
            int hitCount = Physics2D.OverlapBox(center, pogoData.HitboxSize, 0f, hitFilter, hitResults);

            for (int index = 0; index < hitCount; index++)
            {
                Collider2D hit = hitResults[index];
                if (hit == null || hit == bodyCollider || hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                PogoTarget target = hit.GetComponentInParent<PogoTarget>();
                if (target == null || !target.isActiveAndEnabled)
                {
                    continue;
                }

                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if ((damageable != null && !damageable.CanReceiveDamage) || !hitTargets.Add(target))
                {
                    continue;
                }

                attackTimeRemaining = 0f;
                attackAvailable = true;
                controller.BounceFromPogo(pogoData.BounceSpeed);
                // Bounce first: a lethal hit may disable or destroy the target.
                damageable?.TakeDamage(pogoData.Damage);
                if (damageable != null) CombatHitstop.Request(CombatHitstopKind.Pogo);
                return;
            }

            attackTimeRemaining -= Time.fixedDeltaTime;
        }

        public bool TryGetAttackVisual(out Vector2 center, out Vector2 size)
        {
            center = size = default;
            // An immediate hit can consume the window within its first physics tick.
            // Sample that executed attack for this render frame only; no combat timer is extended.
            if (!isActiveAndEnabled || (attackTimeRemaining <= 0f && visualStartFrame != Time.frameCount) ||
                (abilities != null && !abilities.FangOwned) || controller.IsControlLocked || controller.IsDashing || controller.IsGrounded) return false;
            center = GetHitboxCenter(); size = pogoData.HitboxSize;
            return true;
        }

        private Vector2 GetHitboxCenter()
        {
            Bounds bounds = bodyCollider.bounds;
            return new Vector2(bounds.center.x + pogoData.HitboxOffset.x, bounds.min.y + pogoData.HitboxOffset.y);
        }

        private void OnDrawGizmosSelected()
        {
            if (pogoData == null) return;
            Collider2D collider = GetComponent<Collider2D>();
            if (collider == null) return;

            Bounds bounds = collider.bounds;
            Vector2 center = new Vector2(bounds.center.x + pogoData.HitboxOffset.x,
                bounds.min.y + pogoData.HitboxOffset.y);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(center, pogoData.HitboxSize);
        }
    }
}
