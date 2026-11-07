using System.Collections.Generic;
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
        private Collider2D bodyCollider;
        private InputAction moveAction;
        private InputAction attackAction;
        private ContactFilter2D hitFilter;
        private bool attackRequested;
        private bool attackAvailable = true;
        private float attackTimeRemaining;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
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
            attackRequested = false;
            attackTimeRemaining = 0f;
        }

        private void Update()
        {
            if (attackAction != null && !controller.IsControlLocked && attackAction.WasPressedThisFrame() &&
                moveAction.ReadValue<Vector2>().y < -0.5f)
            {
                attackRequested = true;
            }
        }

        public void ResetForRespawn()
        {
            attackRequested = false;
            attackAvailable = true;
            attackTimeRemaining = 0f;
            hitTargets.Clear();
        }

        private void FixedUpdate()
        {
            if (controller.IsControlLocked)
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
                    hitTargets.Clear();
                }
            }

            if (attackTimeRemaining <= 0f || controller.IsDashing || controller.IsGrounded)
            {
                attackTimeRemaining = 0f;
                return;
            }

            Bounds bounds = bodyCollider.bounds;
            Vector2 center = new Vector2(bounds.center.x + pogoData.HitboxOffset.x,
                bounds.min.y + pogoData.HitboxOffset.y);
            int hitCount = Physics2D.OverlapBox(center, pogoData.HitboxSize, 0f, hitFilter, hitResults);

            for (int index = 0; index < hitCount; index++)
            {
                Collider2D hit = hitResults[index];
                if (hit == null || hit == bodyCollider || hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                PogoTarget target = hit.GetComponentInParent<PogoTarget>();
                if (target == null || !target.isActiveAndEnabled || !hitTargets.Add(target))
                {
                    continue;
                }

                attackTimeRemaining = 0f;
                attackAvailable = true;
                controller.BounceFromPogo(pogoData.BounceSpeed);
                return;
            }

            attackTimeRemaining -= Time.fixedDeltaTime;
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
