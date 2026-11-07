using System.Collections.Generic;
using Fangbound.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fangbound.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController), typeof(Collider2D), typeof(SpriteRenderer))]
    public sealed class PlayerCombat : MonoBehaviour
    {
        private enum AttackDirection
        {
            None,
            Horizontal,
            Upward
        }

        [SerializeField] private PlayerCombatData combatData;
        [SerializeField] private InputActionAsset inputActions;

        private readonly Collider2D[] hitResults = new Collider2D[32];
        private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

        private PlayerController playerController;
        private Collider2D bodyCollider;
        private SpriteRenderer playerRenderer;
        private InputActionMap playerMap;
        private InputAction moveAction;
        private InputAction attackAction;
        private ContactFilter2D hitFilter;
        private AttackDirection currentAttack;
        private float attackTimeRemaining;
        private int facingDirection = 1;
        private bool hitstopRequested;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            bodyCollider = GetComponent<Collider2D>();
            playerRenderer = GetComponent<SpriteRenderer>();

            if (combatData == null || inputActions == null)
            {
                Debug.LogError("PlayerCombat necesita Combat Data e Input Actions.", this);
                enabled = false;
                return;
            }

            playerMap = inputActions.FindActionMap("Player", true);
            moveAction = playerMap.FindAction("Move", true);
            attackAction = playerMap.FindAction("Attack", true);
            hitFilter = ContactFilter2D.noFilter;
            hitFilter.useTriggers = true;
            facingDirection = playerRenderer.flipX ? -1 : 1;
        }

        private void OnEnable()
        {
            if (attackAction != null && !attackAction.enabled)
            {
                attackAction.Enable();
            }
        }

        private void OnDisable()
        {
            if (attackAction != null && attackAction.enabled)
            {
                attackAction.Disable();
            }

            currentAttack = AttackDirection.None;
            attackTimeRemaining = 0f;
            hitTargets.Clear();
            hitstopRequested = false;
        }

        private void Update()
        {
            if (attackAction == null || moveAction == null || !attackAction.WasPressedThisFrame() ||
                playerController.IsDashing || playerController.IsControlLocked)
            {
                return;
            }

            Vector2 moveInput = moveAction.ReadValue<Vector2>();
            if (moveInput.y < -0.5f)
            {
                return;
            }

            if (attackTimeRemaining > 0f)
            {
                return;
            }

            if (moveInput.x < -0.1f)
            {
                facingDirection = -1;
            }
            else if (moveInput.x > 0.1f)
            {
                facingDirection = 1;
            }
            else
            {
                facingDirection = playerRenderer.flipX ? -1 : 1;
            }

            currentAttack = moveInput.y > 0.5f ? AttackDirection.Upward : AttackDirection.Horizontal;
            hitstopRequested = false;
            attackTimeRemaining = combatData.AttackWindow;
            hitTargets.Clear();
        }

        private void FixedUpdate()
        {
            if (currentAttack == AttackDirection.None)
            {
                return;
            }

            if (playerController.IsDashing || playerController.IsControlLocked)
            {
                EndAttack();
                return;
            }

            ApplyHitbox();
            attackTimeRemaining -= Time.fixedDeltaTime;
            if (attackTimeRemaining <= 0f)
            {
                EndAttack();
            }
        }

        private void ApplyHitbox()
        {
            Bounds bounds = bodyCollider.bounds;
            Vector2 center;
            Vector2 size;

            if (currentAttack == AttackDirection.Upward)
            {
                center = (Vector2)bounds.center + combatData.UpwardHitboxOffset;
                size = combatData.UpwardHitboxSize;
            }
            else
            {
                Vector2 offset = combatData.HorizontalHitboxOffset;
                offset.x *= facingDirection;
                center = (Vector2)bounds.center + offset;
                size = combatData.HorizontalHitboxSize;
            }

            int hitCount = Physics2D.OverlapBox(center, size, 0f, hitFilter, hitResults);
            for (int index = 0; index < hitCount; index++)
            {
                Collider2D hit = hitResults[index];
                if (hit == null || hit == bodyCollider || hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable == null || !damageable.CanReceiveDamage || !hitTargets.Add(damageable))
                {
                    continue;
                }

                damageable.TakeDamage(combatData.Damage);
                if (!hitstopRequested)
                {
                    hitstopRequested = true;
                    CombatHitstop.Request(CombatHitstopKind.Normal);
                }
            }
        }

        private void EndAttack()
        {
            currentAttack = AttackDirection.None;
            attackTimeRemaining = 0f;
            hitTargets.Clear();
            hitstopRequested = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (combatData == null)
            {
                return;
            }

            Collider2D collider = GetComponent<Collider2D>();
            if (collider == null)
            {
                return;
            }

            Bounds bounds = collider.bounds;
            Gizmos.color = Color.cyan;
            Vector2 horizontalOffset = combatData.HorizontalHitboxOffset;
            horizontalOffset.x *= playerRenderer != null && playerRenderer.flipX ? -1 : 1;
            Gizmos.DrawWireCube((Vector2)bounds.center + horizontalOffset, combatData.HorizontalHitboxSize);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube((Vector2)bounds.center + combatData.UpwardHitboxOffset, combatData.UpwardHitboxSize);
        }
    }
}
