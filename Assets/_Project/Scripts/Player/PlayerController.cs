using UnityEngine;
using UnityEngine.InputSystem;

namespace Fangbound.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(PlayerAbilities))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerMovementData movementData;
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private SpriteRenderer playerRenderer;
        [SerializeField] private Transform facingIndicator;

        private readonly Collider2D[] groundResults = new Collider2D[8];

        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private PlayerAbilities abilities;
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction dashAction;
        private ContactFilter2D groundContactFilter;
        private float horizontalInput;
        private float jumpBufferRemaining;
        private float coyoteTimeRemaining;
        private float dashCooldownRemaining;
        private float dashTimeRemaining;
        private float dashDistanceRemaining;
        private float dashGravityScale;
        private bool dashRequested;
        private bool isDashing;
        private bool externalControlLocked;
        private bool airDashAvailable = true;
        private int airJumpsRemaining;
        private int dashDirection;
        private bool jumpReleased;
        private int facingDirection = 1;

        public bool IsGrounded { get; private set; }
        public bool IsDashing => isDashing;
        public bool IsControlLocked => externalControlLocked;

        public void SetExternalControlLocked(bool locked)
        {
            externalControlLocked = locked;
            if (!locked)
            {
                return;
            }

            horizontalInput = 0f;
            jumpBufferRemaining = 0f;
            jumpReleased = false;
            dashRequested = false;
            if (isDashing)
            {
                EndDash();
            }
        }

        public void ResetTransientState()
        {
            externalControlLocked = false;
            dashRequested = false;
            isDashing = false;
            dashCooldownRemaining = 0f;
            dashTimeRemaining = 0f;
            dashDistanceRemaining = 0f;
            dashGravityScale = movementData != null ? movementData.GravityScale : 1f;
            airDashAvailable = true;
            airJumpsRemaining = 0;
            coyoteTimeRemaining = 0f;
            jumpBufferRemaining = 0f;
            jumpReleased = false;
            horizontalInput = 0f;
            if (body != null)
            {
                body.gravityScale = dashGravityScale;
            }
        }

        public bool TryStartPogoDive()
        {
            if (IsGrounded || isDashing ||
                (body.linearVelocity.y <= 0.01f && CheckGrounded()))
            {
                return false;
            }

            coyoteTimeRemaining = 0f;
            jumpBufferRemaining = 0f;
            jumpReleased = false;
            return true;
        }

        public void BounceFromPogo(float upwardSpeed)
        {
            if (upwardSpeed <= 0f || isDashing)
            {
                return;
            }

            Vector2 velocity = body.linearVelocity;
            velocity.y = upwardSpeed;
            body.linearVelocity = velocity;
            IsGrounded = false;
            coyoteTimeRemaining = 0f;
            jumpBufferRemaining = 0f;
            jumpReleased = false;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            abilities = GetComponent<PlayerAbilities>();

            if (movementData == null || inputActions == null || playerRenderer == null)
            {
                Debug.LogError("PlayerController necesita Movement Data, Input Actions y Sprite Renderer.", this);
                enabled = false;
                return;
            }

            InputActionMap playerMap = inputActions.FindActionMap("Player", true);
            moveAction = playerMap.FindAction("Move", true);
            jumpAction = playerMap.FindAction("Jump", true);
            dashAction = playerMap.FindAction("Dash", true);

            groundContactFilter.SetLayerMask(groundLayers);
            groundContactFilter.useTriggers = false;

            body.gravityScale = movementData.GravityScale;
            body.constraints |= RigidbodyConstraints2D.FreezeRotation;
        }

        private void OnEnable()
        {
            EnableAction(moveAction);
            EnableAction(jumpAction);
            EnableAction(dashAction);
        }

        private void OnDisable()
        {
            DisableAction(moveAction);
            DisableAction(jumpAction);
            DisableAction(dashAction);

            if (isDashing && body != null)
            {
                EndDash();
            }
        }

        private void Update()
        {
            if (moveAction == null)
            {
                return;
            }

            if (externalControlLocked)
            {
                horizontalInput = 0f;
                jumpBufferRemaining = 0f;
                jumpReleased = false;
                dashRequested = false;
                return;
            }

            horizontalInput = Mathf.Clamp(moveAction.ReadValue<Vector2>().x, -1f, 1f);
            jumpBufferRemaining = Mathf.Max(0f, jumpBufferRemaining - Time.deltaTime);

            if (jumpAction.WasPressedThisFrame())
            {
                jumpBufferRemaining = movementData.JumpBufferTime;
            }

            if (jumpAction.WasReleasedThisFrame())
            {
                jumpReleased = true;
            }

            if (dashAction.WasPressedThisFrame() && !isDashing)
            {
                dashRequested = true;
            }
        }

        private void FixedUpdate()
        {
            float fixedDeltaTime = Time.fixedDeltaTime;
            dashCooldownRemaining = Mathf.Max(0f, dashCooldownRemaining - fixedDeltaTime);

            if (isDashing)
            {
                UpdateDash(fixedDeltaTime);
                return;
            }

            if (externalControlLocked)
            {
                Vector2 lockedVelocity = body.linearVelocity;
                if (lockedVelocity.y < -movementData.MaxFallSpeed)
                {
                    lockedVelocity.y = -movementData.MaxFallSpeed;
                }

                body.linearVelocity = lockedVelocity;
                return;
            }

            Vector2 velocity = body.linearVelocity;

            IsGrounded = velocity.y <= 0.01f && CheckGrounded();
            if (IsGrounded)
            {
                coyoteTimeRemaining = movementData.CoyoteTime;
                airJumpsRemaining = 0;
                airDashAvailable = true;
            }
            else
            {
                coyoteTimeRemaining = Mathf.Max(0f, coyoteTimeRemaining - fixedDeltaTime);
            }

            ApplyHorizontalMovement(ref velocity, fixedDeltaTime);
            ConsumeBufferedJump(ref velocity);

            if (jumpReleased)
            {
                if (velocity.y > 0f)
                {
                    velocity.y *= movementData.JumpCutMultiplier;
                }

                jumpReleased = false;
            }

            if (velocity.y < -movementData.MaxFallSpeed)
            {
                velocity.y = -movementData.MaxFallSpeed;
            }

            body.linearVelocity = velocity;

            if (dashRequested)
            {
                dashRequested = false;
                TryStartDash();
            }
        }

        private void ConsumeBufferedJump(ref Vector2 velocity)
        {
            if (jumpBufferRemaining <= 0f)
            {
                return;
            }

            if (coyoteTimeRemaining > 0f)
            {
                velocity.y = movementData.JumpVelocity;
                jumpBufferRemaining = 0f;
                coyoteTimeRemaining = 0f;
                airJumpsRemaining = abilities.DoubleJumpUnlocked ? 1 : 0;
                IsGrounded = false;
                return;
            }

            if (!IsGrounded && abilities.DoubleJumpUnlocked && airJumpsRemaining > 0)
            {
                velocity.y = movementData.DoubleJumpVelocity;
                jumpBufferRemaining = 0f;
                airJumpsRemaining--;
                IsGrounded = false;
            }
        }

        private void TryStartDash()
        {
            if (!abilities.DashUnlocked || dashCooldownRemaining > 0f || movementData.DashDistance <= 0f ||
                movementData.DashSpeed <= 0f || movementData.DashDuration <= 0f)
            {
                return;
            }

            if (!IsGrounded && !airDashAvailable)
            {
                return;
            }

            if (!IsGrounded)
            {
                airDashAvailable = false;
            }

            isDashing = true;
            IsGrounded = false;
            dashDirection = facingDirection;
            dashTimeRemaining = movementData.DashDuration;
            dashDistanceRemaining = movementData.DashDistance;
            dashGravityScale = body.gravityScale;
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.zero;
            dashCooldownRemaining = movementData.DashCooldown;
        }

        private void UpdateDash(float fixedDeltaTime)
        {
            if (dashTimeRemaining <= 0f || dashDistanceRemaining <= 0f)
            {
                EndDash();
                return;
            }

            float distanceThisStep = Mathf.Min(movementData.DashSpeed * fixedDeltaTime, dashDistanceRemaining);
            body.linearVelocity = new Vector2(dashDirection * distanceThisStep / fixedDeltaTime, 0f);
            dashDistanceRemaining -= distanceThisStep;
            dashTimeRemaining -= fixedDeltaTime;

            if (dashTimeRemaining <= 0f || dashDistanceRemaining <= 0f)
            {
                EndDash();
            }
        }

        private void EndDash()
        {
            isDashing = false;
            body.gravityScale = dashGravityScale;
        }

        private bool CheckGrounded()
        {
            Bounds bounds = bodyCollider.bounds;
            float probeHeight = movementData.GroundCheckDistance;
            Vector2 probeSize = new Vector2(
                Mathf.Max(0.1f, bounds.size.x * movementData.GroundCheckWidth),
                probeHeight);
            Vector2 probeCenter = new Vector2(
                bounds.center.x,
                bounds.min.y - probeHeight * 0.5f + 0.025f);

            int hitCount = Physics2D.OverlapBox(
                probeCenter,
                probeSize,
                0f,
                groundContactFilter,
                groundResults);

            for (int index = 0; index < hitCount; index++)
            {
                if (groundResults[index] != null && groundResults[index] != bodyCollider)
                {
                    return true;
                }
            }

            return false;
        }

        public void SetFacingDirection(int direction)
        {
            facingDirection = direction < 0 ? -1 : 1;
            playerRenderer.flipX = facingDirection < 0;
            if (facingIndicator != null)
            {
                Vector3 indicatorPosition = facingIndicator.localPosition;
                indicatorPosition.x = Mathf.Abs(indicatorPosition.x) * facingDirection;
                facingIndicator.localPosition = indicatorPosition;
            }
        }

        private void ApplyHorizontalMovement(ref Vector2 velocity, float fixedDeltaTime)
        {
            if (Mathf.Abs(horizontalInput) > 0.01f) SetFacingDirection(horizontalInput > 0f ? 1 : -1);

            float targetSpeed = horizontalInput * movementData.MoveSpeed;
            float accelerationRate;

            if (Mathf.Abs(horizontalInput) <= 0.01f)
            {
                accelerationRate = movementData.Deceleration;
            }
            else if (Mathf.Abs(velocity.x) > 0.01f && Mathf.Sign(velocity.x) != Mathf.Sign(horizontalInput))
            {
                accelerationRate = movementData.TurnAcceleration;
            }
            else
            {
                accelerationRate = movementData.Acceleration;
            }

            velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, accelerationRate * fixedDeltaTime);
        }

        private static void EnableAction(InputAction action)
        {
            if (action != null && !action.enabled)
            {
                action.Enable();
            }
        }

        private static void DisableAction(InputAction action)
        {
            if (action != null && action.enabled)
            {
                action.Disable();
            }
        }
    }
}
