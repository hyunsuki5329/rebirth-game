using System;
using Roguelite.Core;
using UnityEngine;
namespace Roguelite
{
    [Serializable]
    public struct EnemySpawn
    {
        public EnemyAI prefab;
        public Vector2 position;
    }
    [CreateAssetMenu(menuName = "Roguelite/Room")]
    public sealed class RoomDefinition : ScriptableObject
    {
        public string id;
        public RoomKind kind;
        [Min(0)] public int gold = 10;
        public EnemySpawn[] enemies = Array.Empty<EnemySpawn>();
        public bool IsValid => !string.IsNullOrEmpty(id) && enemies != null &&
            Array.TrueForAll(enemies, ValidSpawn) &&
            (kind != RoomKind.Combat && kind != RoomKind.Elite || enemies.Length > 0);
        private static bool ValidSpawn(EnemySpawn spawn)
        {
            if (float.IsNaN(spawn.position.x) || float.IsNaN(spawn.position.y) ||
                float.IsInfinity(spawn.position.x) || float.IsInfinity(spawn.position.y)) return false;
            var prefab = spawn.prefab;
            return prefab != null && prefab.data != null && prefab.data.attack != null &&
                prefab.TryGetComponent<EnemyHealth>(out var health) && health.data != null &&
                prefab.TryGetComponent<EnemyAttack>(out var attack) && attack.data != null && attack.data.attack != null &&
                prefab.GetComponent<EnemyMotor2D>() != null && prefab.GetComponent<Rigidbody2D>() != null &&
                prefab.GetComponent<Collider2D>() != null && prefab.GetComponent<Hurtbox>() != null;
        }
    }
}
