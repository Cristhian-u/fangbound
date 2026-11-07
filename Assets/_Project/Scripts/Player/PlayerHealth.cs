using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fangbound.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(PlayerController))]
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private PlayerHealthData healthData;
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private Transform spawnPoint;

        private Rigidbody2D body;
        private SpriteRenderer playerRenderer;
        private PlayerController playerController;
        private PlayerCombat playerCombat;
        private PlayerPogo playerPogo;
        private PlayerParry playerParry;
        private PlayerImpactReceiver impactReceiver;
        private PlayerCheckpoint checkpoint;
        private InputAction restartAction;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private Color originalColor;
        private bool originalRendererEnabled;
        private float originalGravityScale;
        private float invulnerabilityRemaining;
        private float knockbackControlRemaining;
        private float blinkElapsed;

        public int CurrentHealth { get; private set; }
        public int MaximumHealth => healthData != null ? healthData.MaximumHealth : 0;
        public bool IsDead { get; private set; }
        public bool IsInvulnerable => !IsDead && invulnerabilityRemaining > 0f;

        public event Action<int, int> HealthChanged;
        public event Action<int> DamageTaken;
        public event Action<int> Healed;
        public event Action Died;
        public event Action Respawned;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            playerRenderer = GetComponent<SpriteRenderer>();
            playerController = GetComponent<PlayerController>();
            playerCombat = GetComponent<PlayerCombat>();
            playerPogo = GetComponent<PlayerPogo>();
            playerParry = GetComponent<PlayerParry>();
            impactReceiver = GetComponent<PlayerImpactReceiver>();
            checkpoint = GetComponent<PlayerCheckpoint>();

            if (healthData == null || inputActions == null)
            {
                Debug.LogError("PlayerHealth necesita Health Data e Input Actions.", this);
                enabled = false;
                return;
            }

            restartAction = inputActions.FindActionMap("Player", true).FindAction("Restart", true);
            spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
            spawnRotation = spawnPoint != null ? spawnPoint.rotation : transform.rotation;
            originalColor = playerRenderer.color;
            originalRendererEnabled = playerRenderer.enabled;
            CurrentHealth = healthData.MaximumHealth;
        }

        private void Start()
        {
            originalGravityScale = body.gravityScale;
            HealthChanged?.Invoke(CurrentHealth, MaximumHealth);
        }

        private void OnEnable()
        {
            if (restartAction != null && !restartAction.enabled)
            {
                restartAction.Enable();
            }
        }

        private void OnDisable()
        {
            if (restartAction != null && restartAction.enabled)
            {
                restartAction.Disable();
            }
        }

        private void Update()
        {
            if (IsDead)
            {
                if (restartAction != null && restartAction.WasPressedThisFrame())
                {
                    RestartForTesting();
                }

                return;
            }

            if (knockbackControlRemaining > 0f)
            {
                knockbackControlRemaining = Mathf.Max(0f, knockbackControlRemaining - Time.deltaTime);
                if (knockbackControlRemaining <= 0f)
                {
                    playerController.SetExternalControlLocked(false);
                }
            }

            if (invulnerabilityRemaining > 0f)
            {
                invulnerabilityRemaining = Mathf.Max(0f, invulnerabilityRemaining - Time.deltaTime);
                blinkElapsed += Time.deltaTime;
                if (blinkElapsed >= healthData.InvulnerabilityFlashInterval)
                {
                    blinkElapsed = 0f;
                    playerRenderer.enabled = !playerRenderer.enabled;
                }

                if (invulnerabilityRemaining <= 0f)
                {
                    playerRenderer.enabled = originalRendererEnabled;
                    blinkElapsed = 0f;
                }
            }
        }

        public bool TryTakeDamage(int damage, Vector2 sourcePosition)
        {
            if (IsDead || IsInvulnerable || damage <= 0)
            {
                return false;
            }

            CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, MaximumHealth);
            invulnerabilityRemaining = healthData.InvulnerabilityDuration;
            knockbackControlRemaining = healthData.KnockbackControlDuration;
            blinkElapsed = 0f;

            float direction = Mathf.Sign(transform.position.x - sourcePosition.x);
            if (Mathf.Approximately(direction, 0f))
            {
                direction = playerRenderer.flipX ? -1f : 1f;
            }

            body.linearVelocity = new Vector2(
                direction * healthData.KnockbackHorizontalForce,
                healthData.KnockbackVerticalForce);
            playerController.SetExternalControlLocked(true);

            DamageTaken?.Invoke(damage);
            HealthChanged?.Invoke(CurrentHealth, MaximumHealth);

            if (CurrentHealth <= 0)
            {
                EnterDeathState();
            }

            return true;
        }

        public bool TryHeal(int amount)
        {
            if (IsDead || amount <= 0 || CurrentHealth >= MaximumHealth)
            {
                return false;
            }

            int previousHealth = CurrentHealth;
            CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaximumHealth);
            int healedAmount = CurrentHealth - previousHealth;
            Healed?.Invoke(healedAmount);
            HealthChanged?.Invoke(CurrentHealth, MaximumHealth);
            return healedAmount > 0;
        }

        public void RestartForTesting()
        {
            Vector3 restartPosition = spawnPosition;
            Quaternion restartRotation = spawnRotation;
            if (checkpoint != null && checkpoint.TryGetRespawnPose(out Vector3 checkpointPosition, out Quaternion checkpointRotation))
            {
                restartPosition = checkpointPosition;
                restartRotation = checkpointRotation;
            }
            // Also clear an in-progress attack/parry if this API is called while alive.
            playerCombat.enabled = false;
            playerPogo.enabled = false;
            playerParry.enabled = false;
            playerController.enabled = false;
            IsDead = false;
            CurrentHealth = MaximumHealth;
            invulnerabilityRemaining = 0f;
            knockbackControlRemaining = 0f;
            blinkElapsed = 0f;

            transform.SetPositionAndRotation(restartPosition, restartRotation);
            body.position = restartPosition;
            body.rotation = restartRotation.eulerAngles.z;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.gravityScale = originalGravityScale;
            playerRenderer.enabled = originalRendererEnabled;
            playerRenderer.color = originalColor;

            playerController.enabled = true;
            playerController.ResetTransientState();
            playerCombat.enabled = true;
            playerPogo.enabled = true;
            playerParry.enabled = true;
            impactReceiver.enabled = true;
            playerPogo.ResetForRespawn();

            HealthChanged?.Invoke(CurrentHealth, MaximumHealth);
            Respawned?.Invoke();
        }

        private void EnterDeathState()
        {
            IsDead = true;
            invulnerabilityRemaining = 0f;
            knockbackControlRemaining = 0f;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.gravityScale = 0f;
            playerController.SetExternalControlLocked(true);
            playerCombat.enabled = false;
            playerPogo.enabled = false;
            playerParry.enabled = false;
            playerRenderer.enabled = true;
            playerRenderer.color = healthData.DeathColor;
            Died?.Invoke();
        }
    }
}
