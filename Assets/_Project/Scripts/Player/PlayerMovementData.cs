using UnityEngine;

namespace Fangbound.Player
{
    [CreateAssetMenu(fileName = "PlayerMovementData", menuName = "Fangbound/Player Movement Data")]
    public sealed class PlayerMovementData : ScriptableObject
    {
        [Header("Horizontal Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 7.5f;
        [SerializeField, Min(0f)] private float acceleration = 50f;
        [SerializeField, Min(0f)] private float deceleration = 60f;
        [SerializeField, Min(0f)] private float turnAcceleration = 90f;

        [Header("Jump")]
        [SerializeField, Min(0f)] private float jumpVelocity = 13f;
        [SerializeField, Min(0f)] private float coyoteTime = 0.12f;
        [SerializeField, Min(0f)] private float jumpBufferTime = 0.12f;
        [SerializeField, Range(0.1f, 0.9f)] private float jumpCutMultiplier = 0.45f;
        [SerializeField, Min(0f)] private float doubleJumpVelocity = 12f;

        [Header("Dash")]
        [SerializeField, Min(0f)] private float dashDistance = 3.2f;
        [SerializeField, Min(0f)] private float dashSpeed = 18f;
        [SerializeField, Min(0f)] private float dashDuration = 0.18f;
        [SerializeField, Min(0f)] private float dashCooldown = 0.35f;
        [SerializeField] private bool dashHasInvulnerability = true;

        [Header("Physics")]
        [SerializeField, Min(0f)] private float gravityScale = 3f;
        [SerializeField, Min(0f)] private float maxFallSpeed = 18f;

        [Header("Ground Check")]
        [SerializeField, Min(0.01f)] private float groundCheckDistance = 0.14f;
        [SerializeField, Range(0.1f, 1f)] private float groundCheckWidth = 0.85f;

        public float MoveSpeed => moveSpeed;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        public float TurnAcceleration => turnAcceleration;
        public float JumpVelocity => jumpVelocity;
        public float CoyoteTime => coyoteTime;
        public float JumpBufferTime => jumpBufferTime;
        public float JumpCutMultiplier => jumpCutMultiplier;
        public float DoubleJumpVelocity => doubleJumpVelocity;
        public float DashDistance => dashDistance;
        public float DashSpeed => dashSpeed;
        public float DashDuration => dashDuration;
        public float DashCooldown => dashCooldown;
        public bool DashHasInvulnerability => dashHasInvulnerability;
        public float GravityScale => gravityScale;
        public float MaxFallSpeed => maxFallSpeed;
        public float GroundCheckDistance => groundCheckDistance;
        public float GroundCheckWidth => groundCheckWidth;
    }
}