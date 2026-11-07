using System;
using Fangbound.CameraSystem;
using Fangbound.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fangbound.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerHealth), typeof(PlayerController))]
    public sealed class PlayerCheckpoint : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private CameraFollow2D cameraFollow;
        private PlayerHealth health;
        private PlayerController controller;
        private InputAction interactAction;

        public MemoryFlame CurrentCheckpoint { get; private set; }
        public MemoryFlame NearbyFlame { get; private set; }
        public event Action<MemoryFlame> CheckpointChanged;
        public event Action<MemoryFlame> Rested;

        private void Awake()
        {
            health = GetComponent<PlayerHealth>();
            controller = GetComponent<PlayerController>();
            if (inputActions == null)
            {
                Debug.LogError("PlayerCheckpoint necesita Input Actions.", this);
                enabled = false;
                return;
            }
            interactAction = inputActions.FindActionMap("Player", true).FindAction("Interact", true);
            if (cameraFollow == null) cameraFollow = FindAnyObjectByType<CameraFollow2D>();
        }
        private void OnEnable()
        {
            if (interactAction != null) interactAction.Enable();
            if (health != null) health.Respawned += HandleRespawn;
        }
        private void OnDisable()
        {
            if (interactAction != null) interactAction.Disable();
            if (health != null) health.Respawned -= HandleRespawn;
            NearbyFlame = null;
        }

        private bool CanRest => isActiveAndEnabled && health != null && !health.IsDead &&
            !controller.IsControlLocked && !controller.IsDashing;

        private void Update()
        {
            NearbyFlame = CanRest ? MemoryFlame.FindClosest(transform.position) : null;
            if (NearbyFlame != null && interactAction.WasPressedThisFrame()) TryRest(NearbyFlame);
        }

        public bool TryRest(MemoryFlame flame)
        {
            if (!CanRest || flame == null || !flame.IsInRange(transform.position) ||
                !flame.TryGetSafePose(health, out _, out _)) return false;
            if (CurrentCheckpoint != flame)
            {
                if (CurrentCheckpoint != null) CurrentCheckpoint.SetCurrent(false);
                CurrentCheckpoint = flame;
                flame.SetCurrent(true);
                CheckpointChanged?.Invoke(flame);
            }
            health.TryHeal(health.MaximumHealth);
            flame.ShowActivation();
            Rested?.Invoke(flame);
            return true;
        }

        public bool TryGetRespawnPose(out Vector3 position, out Quaternion rotation)
        {
            position = default;
            rotation = Quaternion.identity;
            return CurrentCheckpoint != null && CurrentCheckpoint.TryGetSafePose(health, out position, out rotation);
        }

        public void ClearCheckpoint()
        {
            if (CurrentCheckpoint != null) CurrentCheckpoint.SetCurrent(false);
            CurrentCheckpoint = null;
            CheckpointChanged?.Invoke(null);
        }

        private void HandleRespawn()
        {
            if (cameraFollow != null) cameraFollow.SnapToTarget();
        }

        private void OnGUI()
        {
            if (!CanRest || NearbyFlame == null || interactAction == null) return;
            string binding = interactAction.GetBindingDisplayString();
            GUI.Box(new Rect(Screen.width * 0.5f - 140f, Screen.height - 70f, 280f, 38f), $"{binding} — Descansar");
        }
    }
}
