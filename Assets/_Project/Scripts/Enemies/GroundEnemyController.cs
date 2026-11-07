using Fangbound.Combat;
using Fangbound.Player;
using UnityEngine;

namespace Fangbound.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(EnemyHealth))]
    public sealed class GroundEnemyController : MonoBehaviour
    {
        public enum EnemyState { Patrol, Chase, Telegraph, Attack, Recovery, Vulnerable, Dead }

        [SerializeField] private GroundEnemyData enemyData;
        [SerializeField] private PlayerHealth target;
        [SerializeField] private EnemyAttackHitbox attackHitbox;
        [SerializeField] private SpriteRenderer telegraphRenderer;
        [SerializeField] private Transform facingIndicator;
        [SerializeField] private Vector2 patrolOffsets = new Vector2(-2.8f, 2.3f);
        [SerializeField] private LayerMask groundLayers = 1;

        private readonly Collider2D[] probeResults = new Collider2D[16];
        private ContactFilter2D terrainFilter;
        private Rigidbody2D body;
        private BoxCollider2D bodyCollider;
        private EnemyHealth health;
        private EnemyPosture posture;
        private float homeX;
        private float stateRemaining;
        private float chaseRetryRemaining;
        private int facingDirection = 1;

        public EnemyState State { get; private set; }
        public float LeftLimit => homeX + Mathf.Min(patrolOffsets.x, patrolOffsets.y);
        public float RightLimit => homeX + Mathf.Max(patrolOffsets.x, patrolOffsets.y);

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<BoxCollider2D>();
            health = GetComponent<EnemyHealth>();
            posture = GetComponent<EnemyPosture>();
            homeX = transform.position.x;
            terrainFilter.SetLayerMask(groundLayers);
            terrainFilter.useTriggers = false;
            if (enemyData == null || attackHitbox == null || telegraphRenderer == null)
            {
                Debug.LogError("GroundEnemyController necesita datos, hitbox y telegraph.", this);
                enabled = false;
                return;
            }
            health.Died += HandleDeath;
            attackHitbox.AttackResolved += HandleAttackResolved;
        }

        private void Start()
        {
            if (target == null) target = FindAnyObjectByType<PlayerHealth>();
            IgnorePlayerBodyCollisions();
            attackHitbox.ConfigureShape(enemyData.HitboxOffset, enemyData.HitboxSize);
            SetFacing(1);
            EnterState(EnemyState.Patrol);
        }

        private void OnEnable()
        {
            IgnorePlayerBodyCollisions();
        }

        private void IgnorePlayerBodyCollisions()
        {
            if (bodyCollider == null || target == null) return;
            // Only solid body contacts are excluded. Combat uses independent
            // overlap queries, and both bodies keep colliding with terrain.
            foreach (Collider2D playerCollider in target.GetComponentsInChildren<Collider2D>(true))
                if (!playerCollider.isTrigger) Physics2D.IgnoreCollision(bodyCollider, playerCollider);
        }

        private void OnDestroy()
        {
            if (health != null) health.Died -= HandleDeath;
            if (attackHitbox != null) attackHitbox.AttackResolved -= HandleAttackResolved;
        }

        private void OnDisable()
        {
            if (attackHitbox != null) attackHitbox.EndAttack();
            if (telegraphRenderer != null) telegraphRenderer.enabled = false;
            if (body != null && body.simulated) SetHorizontalSpeed(0f);
        }

        private void FixedUpdate()
        {
            if (health.IsDead) { HandleDeath(); return; }
            chaseRetryRemaining = Mathf.Max(0f, chaseRetryRemaining - Time.fixedDeltaTime);
            if (posture.IsVulnerable)
            {
                if (State != EnemyState.Vulnerable) EnterState(EnemyState.Vulnerable);
                SetHorizontalSpeed(0f);
                return;
            }
            if (State == EnemyState.Vulnerable) EnterState(EnemyState.Recovery, enemyData.RecoveryDuration);

            switch (State)
            {
                case EnemyState.Patrol:
                    if (body.position.x < LeftLimit) SetFacing(1);
                    else if (body.position.x > RightLimit) SetFacing(-1);
                    else if (chaseRetryRemaining <= 0f && CanTrackTarget(enemyData.DetectionRange))
                    {
                        EnterState(EnemyState.Chase);
                        break;
                    }
                    if (!TryMove(enemyData.PatrolSpeed)) SetFacing(-facingDirection);
                    break;
                case EnemyState.Chase:
                    if (body.position.x < LeftLimit || body.position.x > RightLimit || !CanTrackTarget(enemyData.LoseTargetRange))
                    {
                        EnterState(EnemyState.Patrol);
                        break;
                    }
                    float deltaX = target.transform.position.x - transform.position.x;
                    if (Mathf.Abs(deltaX) > 0.05f) SetFacing(deltaX < 0f ? -1 : 1);
                    if (Mathf.Abs(deltaX) <= enemyData.AttackRange)
                        EnterState(EnemyState.Telegraph, enemyData.TelegraphDuration);
                    else if (!TryMove(enemyData.ChaseSpeed))
                    {
                        // Give patrol time to move away before detecting the same
                        // unreachable target again. Reuse the existing recovery time.
                        SetFacing(-facingDirection);
                        chaseRetryRemaining = enemyData.RecoveryDuration;
                        EnterState(EnemyState.Patrol);
                    }
                    break;
                case EnemyState.Telegraph:
                case EnemyState.Attack:
                case EnemyState.Recovery:
                    SetHorizontalSpeed(0f);
                    stateRemaining -= Time.fixedDeltaTime;
                    if (stateRemaining > 0f) break;
                    if (State == EnemyState.Telegraph)
                    {
                        EnterState(EnemyState.Attack, enemyData.ActiveDuration);
                        attackHitbox.BeginAttack(enemyData.AttackDamage);
                    }
                    else if (State == EnemyState.Attack)
                        EnterState(EnemyState.Recovery, enemyData.RecoveryDuration);
                    else EnterState(CanTrackTarget(enemyData.LoseTargetRange) ? EnemyState.Chase : EnemyState.Patrol);
                    break;
            }
        }

        private bool CanTrackTarget(float range)
        {
            if (target == null || !target.isActiveAndEnabled || target.IsDead) return false;
            Vector2 offset = target.transform.position - transform.position;
            return Mathf.Abs(offset.x) <= range && Mathf.Abs(offset.y) <= enemyData.DetectionHeight;
        }

        private bool TryMove(float speed)
        {
            float step = speed * Time.fixedDeltaTime;
            float nextX = body.position.x + facingDirection * step;
            Bounds bounds = bodyCollider.bounds;
            float probeX = bounds.center.x + facingDirection * (bounds.extents.x + step + 0.05f);
            // An external displacement may leave the body outside its leash.
            // Allow steps toward it, even if one step cannot reach it yet.
            bool leavingLimits = (nextX < LeftLimit && nextX < body.position.x) ||
                (nextX > RightLimit && nextX > body.position.x);
            bool blocked = leavingLimits ||
                !HasTerrain(new Vector2(probeX, bounds.min.y - 0.12f), new Vector2(0.08f, 0.3f)) ||
                HasTerrain(new Vector2(probeX, bounds.center.y), new Vector2(0.08f, bounds.size.y * 0.7f));
            SetHorizontalSpeed(blocked ? 0f : facingDirection * speed);
            return !blocked;
        }

        private bool HasTerrain(Vector2 center, Vector2 size)
        {
            int count = Physics2D.OverlapBox(center, size, 0f, terrainFilter, probeResults);
            for (int index = 0; index < count; index++)
            {
                Collider2D hit = probeResults[index];
                if (!hit.transform.IsChildOf(transform) && hit.GetComponentInParent<PlayerHealth>() == null)
                    return true;
            }
            return false;
        }

        private void SetHorizontalSpeed(float speed)
        {
            body.linearVelocity = new Vector2(speed, body.linearVelocity.y);
        }

        private void SetFacing(int direction)
        {
            facingDirection = direction;
            attackHitbox.SetFacingDirection(direction);
            if (facingIndicator == null) return;
            Vector3 position = facingIndicator.localPosition;
            position.x = Mathf.Abs(position.x) * direction;
            facingIndicator.localPosition = position;
        }

        private void EnterState(EnemyState next, float duration = 0f)
        {
            attackHitbox.EndAttack();
            SetHorizontalSpeed(0f);
            State = next;
            stateRemaining = duration;
            telegraphRenderer.enabled = next == EnemyState.Telegraph || next == EnemyState.Attack;
            telegraphRenderer.color = next == EnemyState.Attack ? enemyData.AttackColor : enemyData.TelegraphColor;
        }

        private void HandleAttackResolved(IncomingAttackOutcome outcome)
        {
            if (outcome != IncomingAttackOutcome.Parried || State != EnemyState.Attack) return;
            EnterState(posture.IsVulnerable ? EnemyState.Vulnerable : EnemyState.Recovery, enemyData.RecoveryDuration);
        }

        private void HandleDeath()
        {
            if (State != EnemyState.Dead) EnterState(EnemyState.Dead);
        }

        private void OnDrawGizmosSelected()
        {
            float centerX = Application.isPlaying ? homeX : transform.position.x;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector3(centerX + patrolOffsets.x, transform.position.y),
                new Vector3(centerX + patrolOffsets.y, transform.position.y));
            if (enemyData == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(enemyData.DetectionRange * 2f, enemyData.DetectionHeight * 2f));
        }
    }
}
