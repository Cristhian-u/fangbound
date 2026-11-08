using Fangbound.Player;
using UnityEngine;

namespace Fangbound.World
{
    [DisallowMultipleComponent]
    public sealed class RoomEntrance : MonoBehaviour
    {
        [SerializeField] private string entranceId;
        [SerializeField] private bool faceRight = true;
        [SerializeField] private LayerMask blockingLayers = 1;
        public string EntranceId => entranceId;
        public int FacingDirection => faceRight ? 1 : -1;
        public bool IsSafe(PlayerHealth player) => SpawnSafety2D.IsSafe(player, transform.position, blockingLayers);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(.7f, 1.1f, 0));
        }
    }
}
