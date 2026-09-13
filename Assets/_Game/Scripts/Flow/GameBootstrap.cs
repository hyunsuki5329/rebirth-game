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
        [Tooltip("Empty uses Application.persistentDataPath. An explicit directory supports isolated QA profiles.")]
        public string profileDirectory = "";
        public string SavePath => store.FilePath;
        public string SaveNotice { get; private set; } = "";
        public string SaveError { get; private set; } = "";
        private ProfileFileStore store;
        public RebirthSystem RebirthRules { get; private set; }
        public RunFlow Flow { get; private set; }
        public ProfileData Profile => Flow.Profile;
        public DungeonManager Dungeon { get; private set; }
        public bool IsLoading { get; private set; }
        public GamePause Pause { get; private set; }
        public int SelectedWeapon { get; private set; }
        private int nextSeed;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            var weaponIds = System.Array.ConvertAll(config.weapons, weapon => weapon.id);
            var upgradeIds = rebirthConfig == null ? System.Array.Empty<string>() : System.Array.ConvertAll(rebirthConfig.upgrades, upgrade => upgrade.id);
            store = new ProfileFileStore(string.IsNullOrEmpty(profileDirectory) ? Application.persistentDataPath : profileDirectory,
                new SaveCodec(weaponIds, upgradeIds));
            var loaded = store.Load(); SaveNotice = loaded.Notice;
            Flow = new RunFlow(loaded.Profile, config.victoryRetention, config.deathRetention); nextSeed = config.firstSeed;
            AudioListener.volume = Profile.settings.masterVolume;
            if (rebirthConfig != null) RebirthRules = rebirthConfig.CreateRules();
            Pause = gameObject.AddComponent<GamePause>(); Pause.Initialize(this);
            RetrySave();
        }
        public bool CanRebirth => !IsLoading && Flow.Phase == RunFlowPhase.Hub && RebirthRules != null && RebirthRules.CanRebirth(Profile);
        public bool Rebirth()
        {
            if (!CanRebirth || !RebirthRules.TryRebirth(Profile)) return false;
            RetrySave(); return true;
        }
        public bool RetrySave()
        {
            bool success = store.TrySave(Profile, out string error); SaveError = error; return success;
        }
        public bool SetMasterVolume(float volume)
        {
            if (float.IsNaN(volume) || float.IsInfinity(volume) || volume < 0 || volume > 1) return false;
            Profile.settings.masterVolume = volume; AudioListener.volume = volume; RetrySave(); return true;
        }
        public bool BuyUpgrade(int index)
        {
            if (IsLoading || Flow.Phase != RunFlowPhase.Hub || rebirthConfig == null || index < 0 || index >= rebirthConfig.upgrades.Length) return false;
            var upgrade = rebirthConfig.upgrades[index];
            if (!RebirthRules.TryBuy(Profile, upgrade.id, upgrade.pointCost)) return false;
            RetrySave(); return true;
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
        public bool OpenMenu()
        {
            if (IsLoading || !Flow.EnterMenu()) return false;
            StartCoroutine(LoadMenu()); return true;
        }
        private IEnumerator LoadMenu()
        {
            IsLoading = true; Pause.Resume(); UnbindDungeon();
            yield return SceneManager.LoadSceneAsync("MainMenu"); IsLoading = false;
        }
        private IEnumerator LoadHub()
        {
            IsLoading = true; Pause.Resume(); UnbindDungeon();
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
        private void OnRunEnded(RunOutcome outcome)
        { if (Flow.Finish(Dungeon.Run, outcome)) RetrySave(); }
        private void UnbindDungeon()
        {
            if (Dungeon != null) Dungeon.RunEnded -= OnRunEnded;
            Dungeon = null;
        }
        private void OnDestroy() => UnbindDungeon();
    }
}
