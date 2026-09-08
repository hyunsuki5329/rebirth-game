using System;
using Roguelite.Core;
using UnityEngine;
namespace Roguelite
{
    public sealed class DungeonManager : MonoBehaviour
    {
        public PlayerHealth playerPrefab;
        public RoomController[] combatRooms;
        public RoomController rewardRoom, shortcutRoom, eliteRoom, bossRoom;
        public int initialSeed = 123;
        public RunState Run { get; private set; }
        public PlayerHealth Player { get; private set; }
        public RoomController Room { get; private set; }
        public RoomGraph Graph { get; private set; }
        public bool PathComplete { get; private set; }
        public RewardService Rewards { get; } = new RewardService();
        public event Action PathCompleted;
        private bool rewardPending;
        private void LateUpdate() => SettleRoomReward();
        private void Start() { if (Run == null) BeginRun(initialSeed); }
        public void BeginRun(int seed)
        {
            rewardPending = false;
            RemoveRoom();
            if (Player != null) { Player.gameObject.SetActive(false); Destroy(Player.gameObject); }
            Run = new RunState(seed); Graph = new RoomGraph(seed); PathComplete = false;
            Player = Instantiate(playerPrefab);
            var camera = Camera.main;
            if (camera != null && camera.TryGetComponent<CameraFollow>(out var follow)) follow.target = Player.transform;
            EnterRoom();
        }
        public bool Advance(bool shortcut = false)
        {
            if (Room == null || !Room.IsClear || Player.IsDead || PathComplete) return false;
            SettleRoomReward();
            int next = Graph.Next(Run.RoomIndex, shortcut);
            if (next < 0) { PathComplete = true; PathCompleted?.Invoke(); return true; }
            RemoveRoom();
            Run.RoomIndex = next;
            Player.gameObject.SetActive(false);
            Player.gameObject.SetActive(true);
            EnterRoom();
            return true;
        }
        private void EnterRoom()
        {
            var node = Graph.Nodes[Run.RoomIndex];
            RoomController prefab;
            switch (node.Kind)
            {
                case RoomKind.Combat: prefab = combatRooms[node.Variant % combatRooms.Length]; break;
                case RoomKind.Reward: prefab = rewardRoom; break;
                case RoomKind.Shortcut: prefab = shortcutRoom; break;
                case RoomKind.Elite: prefab = eliteRoom; break;
                default: prefab = bossRoom; break;
            }
            Room = Instantiate(prefab);
            Player.GetComponent<PlayerMotor2D>().Teleport(Room.entry);
            Room.Cleared += GrantReward;
            Room.exit.Entered += OnExit;
            if (Player.TryGetComponent<MomentumSystem>(out var momentum)) momentum.BeginRoom();
            Room.Initialize(Player);
        }
        private void GrantReward()
        {
            if (Player.TryGetComponent<MomentumSystem>(out var momentum))
                momentum.ClearRoom(Room.Enemies.Count > 0);
            rewardPending = true;
        }
        private void SettleRoomReward()
        {
            if (!rewardPending || Room == null) return;
            rewardPending = false;
            Rewards.GoldMultiplier = Player.TryGetComponent<MomentumSystem>(out var momentum) ? momentum.GoldMultiplier : 1;
            Rewards.GrantRoom(Run, Run.RoomIndex, Room.definition.gold);
        }
        private void OnExit() => Advance();
        private void RemoveRoom()
        {
            if (Room == null) return;
            Room.Cleared -= GrantReward; Room.exit.Entered -= OnExit;
            Room.gameObject.SetActive(false); Destroy(Room.gameObject); Room = null;
        }
        private void OnDestroy() { RemoveRoom(); if (Player != null) Destroy(Player.gameObject); }
    }
}
