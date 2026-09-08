using UnityEngine;
namespace Roguelite
{
    public sealed class EnemyArena : MonoBehaviour
    {
        public PlayerHealth player;
        public EnemyAI[] enemies;
        private void Start() { foreach (var enemy in enemies) enemy.SetTarget(player); }
    }
}
