#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using UnityEngine;
namespace Roguelite.QA
{
    internal static class PlaytestNavigation
    {
        // Test-driver path planning only. Movement still goes through WASD and PlayerMotor2D.
        public static Vector2 Direction(Vector2 position, Vector2 destination)
        {
            int world = LayerMask.GetMask("World"); Vector2 delta = destination - position;
            var filter = new ContactFilter2D { useLayerMask = true, layerMask = world, useTriggers = false };
            var casts = new RaycastHit2D[1]; var overlaps = new Collider2D[1];
            bool Blocked(Vector2 from, Vector2 to, bool allowEscape = false)
            {
                Vector2 step = to - from;
                if (Physics2D.CircleCast(from, 0.35f, step.normalized, filter, casts, step.magnitude) == 0) return false;
                // The planning radius includes a small margin beyond the player's physical half-width.
                // Knockback can put that margin inside a wall. Permit a short first step to free space.
                return !(allowEscape && casts[0].distance <= 0.001f && Physics2D.OverlapCircle(to, 0.35f, filter, overlaps) == 0);
            }
            if (!Blocked(position, destination)) return delta;
            const int width = 31, height = 17;
            Vector2 Point(int index) => new Vector2((index % width) * 0.5f - 7.5f, (index / width) * 0.5f - 4);
            int start = Mathf.Clamp(Mathf.RoundToInt((position.x + 7.5f) * 2), 0, width - 1) +
                Mathf.Clamp(Mathf.RoundToInt((position.y + 4) * 2), 0, height - 1) * width;
            var parents = new int[width * height]; for (int i = 0; i < parents.Length; i++) parents[i] = -1;
            var queue = new Queue<int>(); queue.Enqueue(start); parents[start] = start;
            int best = start; float bestDistance = (Point(start) - destination).sqrMagnitude;
            while (queue.Count > 0)
            {
                int cell = queue.Dequeue(); int x = cell % width, y = cell / width;
                foreach (var offset in new[] { Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down })
                {
                    int nx = x + offset.x, ny = y + offset.y;
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                    int next = ny * width + nx; if (parents[next] != -1) continue;
                    Vector2 from = cell == start ? position : Point(cell), to = Point(next), step = to - from;
                    if (Physics2D.OverlapCircle(to, 0.35f, filter, overlaps) > 0 || Blocked(from, to, cell == start)) continue;
                    parents[next] = cell; queue.Enqueue(next);
                    float distance = (to - destination).sqrMagnitude;
                    if (distance < bestDistance) { bestDistance = distance; best = next; }
                }
            }
            if (best == start) return Vector2.zero;
            while (parents[best] != start) best = parents[best];
            return Point(best) - position;
        }
    }
}
#endif
