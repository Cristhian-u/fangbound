using UnityEngine;
using Fangbound.World;

namespace Fangbound.Player
{
    // Value copied between rooms; never retains an unloaded Unity object.
    public readonly struct CheckpointLocation
    {
        public readonly string ScenePath, FlameId;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly RoomDefinition Room;
        public bool IsValid => !string.IsNullOrEmpty(ScenePath) && !string.IsNullOrEmpty(FlameId);

        public CheckpointLocation(string scenePath, string flameId, Vector3 position, Quaternion rotation, RoomDefinition room)
        { ScenePath = scenePath; FlameId = flameId; Position = position; Rotation = rotation; Room = room; }
    }
}
