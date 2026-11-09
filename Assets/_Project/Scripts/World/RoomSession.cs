using System;
using System.Collections;
using Fangbound.Combat;
using Fangbound.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fangbound.World
{
    // Only this small session survives Single loads. Player components remain
    // authoritative; the snapshot is a transfer value, not a second health system.
    public sealed class RoomSession : MonoBehaviour
    {
        public static RoomSession Instance { get; private set; }
        public RoomScene CurrentRoom { get; private set; }
        public bool IsLoading { get; private set; }
        private RoomDefinition recoveryRoom;
        private RoomDefinition pendingRoom;
        private string pendingEntrance;
        private bool pendingRespawn;
        private Snapshot snapshot;
        private double readyAt;

        private struct Snapshot
        {
            public int Health, MaximumHealth;
            public bool Dash, DoubleJump, Fang;
            public CheckpointLocation Checkpoint;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;

        public static RoomSession Ensure()
        {
            if (Instance == null) Instance = new GameObject("Room Session").AddComponent<RoomSession>();
            return Instance;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += SceneLoaded;
        }

        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.GetComponentInChildren<RoomScene>() != null) return;
            // Entering a legacy scene explicitly ends the room-session scope.
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            if (Instance == this) Instance = null;
        }

        public void Enter(RoomScene room)
        {
            bool arriving = pendingRoom != null;
            if (arriving && pendingRoom != room.Definition)
                throw new InvalidOperationException("La sala cargada no corresponde a la transición pendiente.");
            CurrentRoom = room;
            PlayerHealth player = room.Player;
            if (!arriving)
            {
                recoveryRoom = room.Definition.RecoveryRoom;
                player.GetComponent<PlayerAbilities>().SetFangOwned(!room.Definition.StartWithoutFang);
            }
            else
            {
                if (player.MaximumHealth != snapshot.MaximumHealth)
                    throw new InvalidOperationException("Las salas deben compartir la configuración de vida máxima.");
                PlayerAbilities abilities = player.GetComponent<PlayerAbilities>();
                abilities.SetDashUnlocked(snapshot.Dash);
                abilities.SetDoubleJumpUnlocked(snapshot.DoubleJump);
                abilities.SetFangOwned(snapshot.Fang);
                player.GetComponent<PlayerCheckpoint>().RestoreLocation(snapshot.Checkpoint);
            }
            RoomEntrance entrance = room.FindEntrance(arriving ? pendingEntrance : room.Definition.InitialEntranceId);
            Vector3 position;
            Quaternion rotation;
            int facing = 1;
            bool atCheckpoint = arriving && pendingRespawn && snapshot.Checkpoint.IsValid &&
                snapshot.Checkpoint.ScenePath == room.Definition.ScenePath &&
                player.GetComponent<PlayerCheckpoint>().TryGetRespawnPose(out _, out _);
            if (atCheckpoint)
                player.GetComponent<PlayerCheckpoint>().TryGetRespawnPose(out position, out rotation);
            else
            {
                if (entrance == null || !entrance.IsSafe(player))
                    throw new InvalidOperationException("Entrada ausente o insegura: " + pendingEntrance);
                position = entrance.transform.position;
                rotation = entrance.transform.rotation;
                facing = entrance.FacingDirection;
            }
            player.RestoreAfterSceneLoad(position, rotation,
                !arriving || pendingRespawn ? player.MaximumHealth : snapshot.Health, arriving && pendingRespawn);
            player.GetComponent<PlayerController>().SetFacingDirection(facing);
            room.SynchronizeCamera();
            pendingRoom = null;
            IsLoading = false;
            readyAt = Time.realtimeSinceStartupAsDouble + .25d;
        }

        public bool TryTravel(PlayerHealth player, RoomDefinition target, string entrance)
        {
            if (CurrentRoom == null || player != CurrentRoom.Player || player.IsDead ||
                IsLoading || Time.realtimeSinceStartupAsDouble < readyAt) return false;
            return BeginLoad(player, target, entrance, false);
        }

        public bool TryRestart(PlayerHealth player)
        {
            if (CurrentRoom == null || CurrentRoom.Player != player) return false;
            if (IsLoading) return true;
            CheckpointLocation checkpoint = player.GetComponent<PlayerCheckpoint>().Location;
            RoomDefinition target = checkpoint.IsValid && checkpoint.Room != null ? checkpoint.Room : recoveryRoom;
            BeginLoad(player, target, target.InitialEntranceId, true);
            return true;
        }

        private bool BeginLoad(PlayerHealth player, RoomDefinition target, string entrance, bool respawn)
        {
            if (target == null || string.IsNullOrEmpty(entrance) || !Application.CanStreamedLevelBeLoaded(target.ScenePath))
            { Debug.LogWarning("Destino no disponible; revisar RoomDefinition y Build Settings.", this); return false; }
            PlayerAbilities abilities = player.GetComponent<PlayerAbilities>();
            snapshot = new Snapshot { Health = player.CurrentHealth, MaximumHealth = player.MaximumHealth,
                Dash = abilities.DashUnlocked, DoubleJump = abilities.DoubleJumpUnlocked, Fang = abilities.FangOwned,
                Checkpoint = player.GetComponent<PlayerCheckpoint>().Location };
            pendingRoom = target;
            pendingEntrance = entrance;
            pendingRespawn = respawn;
            IsLoading = true;
            CombatHitstop hitstop = FindAnyObjectByType<CombatHitstop>();
            if (hitstop != null) hitstop.Cancel();
            player.SuspendForSceneLoad();
            StartCoroutine(Load(target));
            return true;
        }

        private IEnumerator Load(RoomDefinition target)
        {
            AsyncOperation operation;
            try { operation = SceneManager.LoadSceneAsync(target.ScenePath, LoadSceneMode.Single); }
            catch (Exception exception)
            {
                IsLoading = false;
                pendingRoom = null;
                // A validated authored destination should not fail to load.
                // Restore the source body instead of leaving controls locked.
                if (!CurrentRoom.Player.IsDead)
                    CurrentRoom.Player.RestoreAfterSceneLoad(CurrentRoom.Player.transform.position,
                        CurrentRoom.Player.transform.rotation, snapshot.Health, false);
                else CurrentRoom.Player.GetComponent<Rigidbody2D>().simulated = true;
                Debug.LogException(exception);
                yield break;
            }
            yield return operation;
        }
    }
}
