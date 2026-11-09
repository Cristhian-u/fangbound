using UnityEngine;

namespace Fangbound.World
{
    [CreateAssetMenu(menuName = "Fangbound/World/Room Definition")]
    public sealed class RoomDefinition : ScriptableObject
    {
        [SerializeField] private string roomId;
        [SerializeField] private string scenePath;
        [SerializeField] private string initialEntranceId = "start";
        [SerializeField] private RoomDefinition recoveryRoom;
        [SerializeField] private bool startWithoutFang;
        public string RoomId => roomId;
        public string ScenePath => scenePath;
        public string InitialEntranceId => initialEntranceId;
        public RoomDefinition RecoveryRoom => recoveryRoom;
        public bool StartWithoutFang => startWithoutFang;
    }
}
