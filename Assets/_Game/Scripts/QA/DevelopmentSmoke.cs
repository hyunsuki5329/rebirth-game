#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Roguelite.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Roguelite.QA
{
    // Opt-in verification for the Development Build only; never installed by a normal launch.
    public sealed class DevelopmentSmoke : MonoBehaviour
    {
        private GameBootstrap session;
        private Keyboard keyboard;
        private Mouse mouse;
        private readonly List<string> checks = new List<string>();
        private string directory, mode;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (!Debug.isDebugBuild || !args.Contains("--smoke-run") && !args.Contains("--smoke-reload")) return;
            if (!args.Contains("--qa-profile")) { Debug.LogError("SMOKE_FAIL: an isolated --qa-profile is required."); Application.Quit(1); return; }
            var root = new GameObject("Development smoke verifier"); DontDestroyOnLoad(root); root.AddComponent<DevelopmentSmoke>();
        }
        private IEnumerator Start()
        {
            mode = Environment.GetCommandLineArgs().Contains("--smoke-reload") ? "reload" : "run";
            directory = QaProfilePath.Resolve(Application.persistentDataPath, Environment.GetCommandLineArgs(), true);
            Directory.CreateDirectory(directory); Application.logMessageReceived += OnLog;
            Application.runInBackground = true; InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            float deadline = Time.realtimeSinceStartup + 20;
            while (session == null && Time.realtimeSinceStartup < deadline) { session = FindFirstObjectByType<GameBootstrap>(); yield return null; }
            Require(session != null, "MainMenu bootstrap exists"); Require(session.Flow.Phase == RunFlowPhase.MainMenu, "MainMenu reached");
            Require(session.SaveError.Length == 0, "initial save succeeds");
            if (mode == "reload")
            {
                Require(File.Exists(Path.Combine(directory, "expected-profile.json")), "previous-process expected profile exists");
                Require(JsonUtility.ToJson(session.Profile) == File.ReadAllText(Path.Combine(directory, "expected-profile.json")), "all Profile fields survive process restart");
                Require(session.Dungeon == null, "restart does not resume a run");
                Complete(); yield break;
            }
            Require(session.Profile.bankGold == 0 && session.Profile.rebirthCount == 0 && session.Profile.totalClears == 0, "fresh QA profile");
            keyboard = InputSystem.AddDevice<Keyboard>(); mouse = InputSystem.AddDevice<Mouse>();
            Require(session.OpenHub(), "MainMenu to Hub"); yield return Ready(); Require(session.SelectWeapon(3), "Bow selected in Hub");
            for (int run = 0; run < 2 && !session.CanRebirth; run++)
            {
                yield return CombatRun();
                Require(session.Flow.Result.Outcome == RunOutcome.Victory, "Boss victory and result");
                Require(session.OpenHub(), "Result to Hub"); yield return Ready();
            }
            Require(session.Rebirth(), "first Rebirth in at most two successful runs");
            Require(session.BuyUpgrade(0), "permanent upgrade purchased"); Require(session.SetMasterVolume(0.6f), "setting changed");
            Require(session.StartRun(), "next run starts"); yield return Ready();
            Require(session.Dungeon.WorldThreat == 1 && session.Dungeon.PermanentGoldMultiplier == 2, "next run applies Threat and permanent growth");
            Require(session.Dungeon.Player.Maximum == 120, "next run applies purchased HP upgrade");
            Require(session.SaveError.Length == 0, "all progress saved");
            File.WriteAllText(Path.Combine(directory, "expected-profile.json"), JsonUtility.ToJson(session.Profile));
            session.Pause.Pause(); Complete();
        }
        private IEnumerator Ready()
        {
            float deadline = Time.realtimeSinceStartup + 20;
            while (session.IsLoading && Time.realtimeSinceStartup < deadline) yield return null;
            Require(!session.IsLoading, "scene transition completed"); yield return null;
        }
        private IEnumerator CombatRun()
        {
            Require(session.StartRun(), "Hub to Dungeon"); yield return Ready();
            float deadline = Time.realtimeSinceStartup + 180; int previousRoom = -1;
            while (session.Flow.Phase == RunFlowPhase.Dungeon && Time.realtimeSinceStartup < deadline)
            {
                var dungeon = session.Dungeon; var player = dungeon.Player; Vector2 position = player.transform.position;
                // This explicit background QA mode owns synthetic devices; normal focus policy is unchanged.
                player.GetComponent<PlayerInputReader>().SetBlocked(false);
                if (previousRoom != dungeon.Run.RoomIndex)
                { previousRoom = dungeon.Run.RoomIndex; Record("room=" + previousRoom + " kind=" + dungeon.Graph.Nodes[previousRoom].Kind); }
                if (dungeon.Room.IsClear)
                {
                    if (dungeon.PendingRewards.Length > 0) Require(dungeon.ChooseReward(0), "reward selected");
                    Vector2 destination = dungeon.Room.exit.transform.position;
                    if (previousRoom == 3 && position.x < 3.75f) destination = new Vector2(4, 0);
                    Move(PlaytestNavigation.Direction(position, destination)); yield return null; continue;
                }
                var target = dungeon.Room.Enemies.Where(e => e != null && !e.IsDead).OrderBy(e => (e.transform.position - player.transform.position).sqrMagnitude).FirstOrDefault();
                if (target == null) { yield return null; continue; }
                Vector2 delta = (Vector2)target.transform.position - position;
                Vector2 movement = delta.magnitude > 4.2f ? delta.normalized : delta.magnitude < 2.8f ? -delta.normalized : Vector2.zero;
                foreach (var hazard in FindObjectsByType<TelegraphedHazard>(FindObjectsSortMode.None))
                {
                    Vector2 away = position - (Vector2)hazard.transform.position;
                    if (away.magnitude < 2) movement = away.sqrMagnitude > 0.01f ? away.normalized : Vector2.up;
                }
                if (dungeon.Boss != null && dungeon.Boss.Phase == BossPhase.ChargeWarning)
                {
                    Vector2 facing = dungeon.Boss.GetComponent<EnemyAttack>().Direction; movement = new Vector2(-facing.y, facing.x);
                    Vector2 landing = position + movement * 3;
                    if (Mathf.Abs(landing.x) > 7 || Mathf.Abs(landing.y) > 3.5f) movement = -movement;
                    player.GetComponent<PlayerDash>().RequestDash(movement);
                }
                if (Mathf.Abs(position.x) > 7 && Mathf.Sign(movement.x) == Mathf.Sign(position.x)) movement.x = -Mathf.Sign(position.x);
                if (Mathf.Abs(position.y) > 3.6f && Mathf.Sign(movement.y) == Mathf.Sign(position.y)) movement.y = -Mathf.Sign(position.y);
                Move(movement.sqrMagnitude > 0 ? PlaytestNavigation.Direction(position, position + movement * 2) : Vector2.zero);
                InputSystem.QueueStateEvent(mouse, new MouseState { position = Camera.main.WorldToScreenPoint(target.transform.position) });
                player.GetComponent<PlayerCombat>().RequestAttack(delta); yield return null;
            }
            Move(Vector2.zero); yield return null;
            Require(session.Flow.Phase == RunFlowPhase.Result, "combat run finishes within timeout");
            Record($"result={session.Flow.Result.Outcome} gold={session.Flow.Result.EarnedGold} hp={session.Dungeon.Player.Current}");
        }
        private void Move(Vector2 direction)
        {
            var keys = new List<Key>();
            if (direction.x > 0.12f) keys.Add(Key.D); else if (direction.x < -0.12f) keys.Add(Key.A);
            if (direction.y > 0.12f) keys.Add(Key.W); else if (direction.y < -0.12f) keys.Add(Key.S);
            if (keys.Count > 0) keys.Add(Key.LeftShift);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys.ToArray()));
        }
        private void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException("SMOKE_FAIL: " + message); Record(message); }
        private void Record(string value) { checks.Add(value); Debug.Log("SMOKE_EVENT: " + value); }
        private void Complete()
        {
            Record("PASS"); WriteReport(true, "");
            if (keyboard != null) { InputSystem.RemoveDevice(keyboard); keyboard = null; }
            if (mouse != null) { InputSystem.RemoveDevice(mouse); mouse = null; }
            if (!Environment.GetCommandLineArgs().Contains("--smoke-hold")) Application.Quit(0);
        }
        private void OnLog(string message, string trace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            Application.logMessageReceived -= OnLog;
            try { WriteReport(false, message); }
            finally { Application.Quit(1); }
        }
        private void WriteReport(bool passed, string error)
        {
            File.WriteAllText(Path.Combine(directory, "smoke-" + mode + ".json"), JsonUtility.ToJson(new Report
            { passed = passed, mode = mode, error = error, unity = Application.unityVersion, graphics = SystemInfo.graphicsDeviceName, profilePath = session != null ? session.SavePath : "", checks = checks.ToArray() }, true));
        }
        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLog;
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            if (mouse != null) InputSystem.RemoveDevice(mouse);
        }
        [Serializable] private sealed class Report
        { public bool passed; public string mode, error, unity, graphics, profilePath; public string[] checks; }
    }
}
#endif
