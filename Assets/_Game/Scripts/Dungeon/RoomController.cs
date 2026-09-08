using System;
using System.Collections.Generic;
using Roguelite.Core;
using UnityEngine;
namespace Roguelite
{
    public sealed class RoomController : MonoBehaviour
    {
        public RoomDefinition definition;
        public Vector2 entry = new Vector2(-6, 0);
        public RoomExit exit;
        private readonly List<EnemyHealth> enemies = new List<EnemyHealth>();
        private RoomClearState clearState;
        public bool IsClear => clearState != null && clearState.IsClear;
        public IReadOnlyList<EnemyHealth> Enemies => enemies;
        public event Action Cleared;
        public void Initialize(PlayerHealth player)
        {
            if (clearState != null) throw new InvalidOperationException("Room already initialized.");
            if (definition == null || !definition.IsValid) throw new InvalidOperationException("Invalid room spawn data: " + name);
            var ids = new List<int>();
            foreach (var spawn in definition.enemies)
            {
                var enemy = Instantiate(spawn.prefab, transform);
                enemy.transform.localPosition = spawn.position;
                enemy.SetTarget(player);
                var health = enemy.GetComponent<EnemyHealth>();
                enemies.Add(health);
                int id = health.GetInstanceID(); ids.Add(id);
                health.Died += () => clearState.Defeat(id);
            }
            clearState = new RoomClearState(ids);
            clearState.Cleared += OnCleared;
            if (exit != null) exit.SetOpen(false);
            if (IsClear) OnCleared();
        }
        private void OnCleared()
        {
            if (exit != null) exit.SetOpen(true);
            Cleared?.Invoke();
        }
        private void OnDestroy() { if (clearState != null) clearState.Cleared -= OnCleared; }
    }
}
