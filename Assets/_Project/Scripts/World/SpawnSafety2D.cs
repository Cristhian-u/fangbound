using Fangbound.Player;
using UnityEngine;

namespace Fangbound.World
{
    public static class SpawnSafety2D
    {
        private static readonly Collider2D[] overlaps = new Collider2D[32];

        public static bool IsSafe(PlayerHealth player, Vector3 position, LayerMask blockingLayers,
            float clearance = .02f, float groundProbe = .2f)
        {
            if (player == null || !player.TryGetComponent(out Collider2D collider)) return false;
            Physics2D.SyncTransforms();
            Bounds bounds = collider.bounds;
            Vector2 center = (Vector2)position + (Vector2)(bounds.center - player.transform.position);
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(blockingLayers);
            filter.useTriggers = false;
            int count = Physics2D.OverlapBox(center, (Vector2)bounds.size + Vector2.one * clearance * 2f, 0, filter, overlaps);
            if (count >= overlaps.Length) return false;
            for (int i = 0; i < count; i++)
                if (!overlaps[i].transform.IsChildOf(player.transform)) return false;
            Vector2 probeSize = new Vector2(bounds.size.x * .8f, groundProbe);
            Vector2 probeCenter = center + Vector2.down * (bounds.size.y * .5f + groundProbe * .5f);
            count = Physics2D.OverlapBox(probeCenter, probeSize, 0, filter, overlaps);
            for (int i = 0; i < count; i++)
                if (!overlaps[i].transform.IsChildOf(player.transform)) return true;
            return false;
        }
    }
}
