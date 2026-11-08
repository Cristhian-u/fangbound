using Fangbound.Player;
using UnityEngine;

namespace Fangbound.World
{
    [DisallowMultipleComponent, RequireComponent(typeof(BoxCollider2D))]
    public sealed class RoomExit : MonoBehaviour
    {
        [SerializeField] private string connectionId;
        [SerializeField] private RoomDefinition destination;
        [SerializeField] private string destinationEntranceId;
        public string ConnectionId => connectionId;
        public RoomDefinition Destination => destination;
        public string DestinationEntranceId => destinationEntranceId;

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
            RoomSession session = RoomSession.Instance;
            if (player != null && !player.IsDead && session != null)
                session.TryTravel(player, destination, destinationEntranceId);
        }
    }
}
