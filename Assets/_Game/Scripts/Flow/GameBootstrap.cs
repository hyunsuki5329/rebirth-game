using System.Collections;
using Roguelite.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Roguelite
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        public RunFlowConfig config;
        public RebirthConfig rebirthConfig;
        public RebirthSystem RebirthRules { get; private set; }
        public RunFlow Flow { get; private set; }
        public ProfileData Profile => Flow.Profile;
        public DungeonManager Dungeon { get; private set; }
        public bool IsLoading { get; private set; }
        public int SelectedWeapon { get; private set; }
        private int nextSeed;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Flow = new RunFlow(new ProfileData(), config.victoryRetention, config.deathRetention); nextSeed = config.firstSeed;
            if (rebirthConfig != null) RebirthRules = rebirthConfig.CreateRules();
        }
        public bool CanRebirth => !IsLoading && Flow.Phase == RunFlowPhase.Hub && RebirthRules != null && RebirthRules.CanRebirth(Profile);
        public bool Rebirth() => CanRebirth && RebirthRules.TryRebirth(Profile);
        public bool BuyUpgrade(int index)
        {
            if (IsLoading || Flow.Phase != RunFlowPhase.Hub || rebirthConfig == null || index < 0 || index >= rebirthConfig.upgrades.Length) return false;
            var upgrade = rebirthConfig.upgrades[index]; return RebirthRules.TryBuy(Profile, upgrade.id, upgrade.pointCost);
        }
        public bool SelectWeapon(int index)
        {
            if (IsLoading || Flow.Phase != RunFlowPhase.Hub || index < 0 || index >= config.weapons.Length) return false;
            SelectedWeapon = index; return true;
        }
        public bool OpenHub()
        {
            if (IsLoading || !Flow.EnterHub()) return false;
            StartCoroutine(LoadHub()); return true;
        }
        private IEnumerator LoadHub()
        {
            IsLoading = true; UnbindDungeon();
            yield return SceneManager.LoadSceneAsync("Hub"); IsLoading = false;
        }
        public bool StartRun()
        {
            if (IsLoading || Flow.Phase != RunFlowPhase.Hub) return false;
            StartCoroutine(LoadDungeon()); return true;
        }
        private IEnumerator LoadDungeon()
        {
            IsLoading = true;
            yield return SceneManager.LoadSceneAsync("Dungeon");
            Dungeon = FindFirstObjectByType<DungeonManager>();
            Dungeon.WorldThreat = WorldThreatSystem.Level(Profile.rebirthCount);
            Dungeon.PermanentGoldMultiplier = Profile.permanentGoldMultiplier;
            if (rebirthConfig != null)
            {
                Dungeon.ThreatGoldMultiplier = WorldThreatSystem.GoldMultiplier(Dungeon.WorldThreat, rebirthConfig.threatGoldMultiplier);
                Dungeon.ThreatProjectileSpread = rebirthConfig.threatProjectileSpread;
            }
            Dungeon.BeginRun(nextSeed++);
            float healthBonus = 0, comboBonus = 0;
            if (rebirthConfig != null)
                foreach (var upgrade in rebirthConfig.upgrades)
                    if (System.Array.IndexOf(Profile.upgradeIds, upgrade.id) >= 0)
                    {
                        if (upgrade.stat == PermanentStat.MaximumHealth) healthBonus += upgrade.value;
                        else if (upgrade.stat == PermanentStat.ComboWindow) comboBonus += upgrade.value;
                    }
            Dungeon.Player.ApplyPermanentHealthBonus(healthBonus);
            Dungeon.Player.GetComponent<ComboSystem>().PermanentWindowBonus = comboBonus;
            Dungeon.Player.GetComponent<WeaponController>().Select(SelectedWeapon);
            Flow.Begin(Dungeon.Run); Dungeon.RunEnded += OnRunEnded;
            IsLoading = false;
        }
        private void OnRunEnded(RunOutcome outcome) => Flow.Finish(Dungeon.Run, outcome);
        private void UnbindDungeon()
        {
            if (Dungeon != null) Dungeon.RunEnded -= OnRunEnded;
            Dungeon = null;
        }
        private void OnDestroy() => UnbindDungeon();
    }
}
