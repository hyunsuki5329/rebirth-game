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
        public ItemData[] rewardItems;
        public int cappedItemGold = 5;
        public ShortcutRules shortcutRules;
        public int PendingRewardStacks { get; private set; } = 1;
        public bool? ChosenShortcut { get; private set; }
        public ItemData[] PendingRewards { get; private set; } = Array.Empty<ItemData>();
        public RunState Run { get; private set; }
        public PlayerHealth Player { get; private set; }
        public RoomController Room { get; private set; }
        public RoomGraph Graph { get; private set; }
        public bool PathComplete { get; private set; }
        public BossController Boss { get; private set; }
        private RunConclusion conclusion = new RunConclusion();
        public RunOutcome Outcome => conclusion.Outcome;
        public event Action<RunOutcome> RunEnded;
        public RewardService Rewards { get; } = new RewardService();
        public event Action PathCompleted;
        private bool rewardPending;
        private void LateUpdate()
        {
            SettleRoomReward();
            if (Player == null || Run == null) return;
            bool bossDead = Room != null && Graph.Nodes[Run.RoomIndex].Kind == RoomKind.Boss && Room.IsClear && Room.Enemies.Count > 0;
            if (!conclusion.Resolve(Player.IsDead, bossDead)) return;
            PathComplete = true;
            Player.GetComponent<PlayerInputReader>().SetBlocked(true);
            Player.GetComponent<PlayerMotor2D>().CanMove = false;
            Player.GetComponent<PlayerCombat>().enabled = false;
            Player.GetComponent<CombatEffects>().Clear();
            foreach (var enemy in Room.Enemies)
                if (enemy != null)
                {
                    enemy.GetComponent<EnemyAI>().enabled = false; enemy.GetComponent<EnemyAttack>().enabled = false;
                    enemy.GetComponent<CombatEffects>().Clear();
                }
            RunEnded?.Invoke(Outcome);
        }
        private void Start() { if (Run == null) BeginRun(initialSeed); }
        public void BeginRun(int seed)
        {
            rewardPending = false;
            PendingRewards = Array.Empty<ItemData>();
            PendingRewardStacks = 1; ChosenShortcut = null;
            conclusion = new RunConclusion();
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
            if (Room == null || !Room.IsClear || Player.IsDead || PathComplete || PendingRewards.Length > 0) return false;
            SettleRoomReward();
            if (PendingRewards.Length > 0) return false;
            if (Run.RoomIndex == 3) ChosenShortcut = shortcut;
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
            if (node.Kind == RoomKind.Reward && rewardItems != null && rewardItems.Length > 0)
            {
                var choices = WeaponStats.RewardChoices(Run.Seed, node.Id, 2, rewardItems.Length);
                PendingRewards = new ItemData[choices.Length];
                for (int i = 0; i < choices.Length; i++) PendingRewards[i] = rewardItems[choices[i]];
            }
            Player.GetComponent<PlayerMotor2D>().Teleport(Room.entry);
            Room.Cleared += GrantReward;
            Room.exit.Entered += OnExit;
            if (Player.TryGetComponent<MomentumSystem>(out var momentum)) momentum.BeginRoom();
            Room.Initialize(Player);
            Boss = Room.GetComponentInChildren<BossController>();
            if (Room.TryGetComponent<ShortcutController>(out var choice)) choice.Bind(this);
            if (node.Kind == RoomKind.Elite && shortcutRules != null)
                foreach (var enemy in Room.Enemies) enemy.gameObject.AddComponent<EliteModifier>().Initialize(Player, shortcutRules);
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
            bool elite = Graph.Nodes[Run.RoomIndex].Kind == RoomKind.Elite;
            int gold = Room.definition.gold;
            if (elite && shortcutRules != null) gold = RewardCalculator.Gold(gold, shortcutRules.eliteGoldMultiplier);
            bool granted = Rewards.GrantRoom(Run, Run.RoomIndex, gold);
            if (granted && shortcutRules != null && (Run.RoomIndex == 5 || elite) && rewardItems.Length > 0)
            {
                PendingRewardStacks = shortcutRules.IsRare(Run.Seed, Run.RoomIndex, elite) ? shortcutRules.rareStacks : 1;
                var choices = WeaponStats.RewardChoices(Run.Seed, Run.RoomIndex, 2, rewardItems.Length);
                PendingRewards = new ItemData[choices.Length];
                for (int i = 0; i < choices.Length; i++) PendingRewards[i] = rewardItems[choices[i]];
            }
        }
        private void OnExit() => Advance();
        public bool ChooseReward(int choice)
        {
            if (choice < 0 || choice >= PendingRewards.Length || Player.IsDead) return false;
            for (int i = 0; i < PendingRewardStacks; i++)
                if (!Player.GetComponent<RunInventory>().Add(PendingRewards[choice])) Run.Award("item-cap-" + Run.RoomIndex + "-" + i, cappedItemGold);
            PendingRewards = Array.Empty<ItemData>(); PendingRewardStacks = 1; return true;
        }
        private void RemoveRoom()
        {
            if (Room == null) return;
            Room.Cleared -= GrantReward; Room.exit.Entered -= OnExit;
            Room.gameObject.SetActive(false); Destroy(Room.gameObject); Room = null;
            Boss = null;
        }
        private void OnDestroy() { RemoveRoom(); if (Player != null) Destroy(Player.gameObject); }
    }
}
