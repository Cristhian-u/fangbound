using System;
using Fangbound.CameraSystem;
using Fangbound.Player;
using UnityEngine;

namespace Fangbound.World
{
    [DefaultExecutionOrder(200), DisallowMultipleComponent]
    public sealed class RoomScene : MonoBehaviour
    {
        [SerializeField] private RoomDefinition definition;
        [SerializeField] private PlayerHealth player;
        [SerializeField] private CameraFollow2D cameraFollow;
        public RoomDefinition Definition => definition;
        public PlayerHealth Player => player;

        private void Start()
        {
            if (definition == null || player == null || cameraFollow == null ||
                definition.ScenePath != gameObject.scene.path || definition.RecoveryRoom == null)
                throw new InvalidOperationException("RoomScene necesita definición, jugador, cámara y sala de recuperación válidos.");
            RoomSession.Ensure().Enter(this);
        }

        public RoomEntrance FindEntrance(string id)
        {
            RoomEntrance result = null;
            foreach (RoomEntrance entrance in GetComponentsInChildren<RoomEntrance>(true))
                if (entrance.EntranceId == id)
                {
                    if (result != null) throw new InvalidOperationException("Entrada duplicada: " + id);
                    result = entrance;
                }
            return result;
        }

        public void SynchronizeCamera() => cameraFollow.SnapToTarget();
    }
}
