using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Roguelite.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
namespace Roguelite.Tests
{
    public class VerticalSliceTests
    {
        private Keyboard keyboard;
        private Mouse mouse;
        private float scale;
        private InputSettings.BackgroundBehavior background;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode editorBehavior;
#endif
        private bool evolved;
        [UnitySetUp] public IEnumerator Setup()
        {
            evolved = false;
            scale = Time.timeScale; Time.timeScale = 3;
            background = InputSystem.settings.backgroundBehavior; InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            editorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            keyboard = InputSystem.AddDevice<Keyboard>(); mouse = InputSystem.AddDevice<Mouse>();
            yield return SaveSessionFixture.Load();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(mouse);
            Time.timeScale = scale; InputSystem.settings.backgroundBehavior = background;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = editorBehavior;
#endif
            foreach (var session in Object.FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None)) Object.Destroy(session.gameObject);
            yield return null; SaveSessionFixture.Cleanup();
        }
        private IEnumerator Ready(GameBootstrap session)
        {
            float deadline = Time.realtimeSinceStartup + 15;
            while (session.IsLoading && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(session.IsLoading, Is.False); yield return null;
        }
        private void Move(Vector2 direction)
        {
            var keys = new List<Key>();
            if (direction.x > 0.12f) keys.Add(Key.D); else if (direction.x < -0.12f) keys.Add(Key.A);
            if (direction.y > 0.12f) keys.Add(Key.W); else if (direction.y < -0.12f) keys.Add(Key.S);
            if (keys.Count > 0) keys.Add(Key.LeftShift);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys.ToArray()));
        }
        private IEnumerator PlayRun(GameBootstrap session, bool dangerous, bool dagger = false)
        {
            Assert.That(session.StartRun(), Is.True); yield return Ready(session);
            var visited = new HashSet<int>(); int stage = 0, lastRoom = -1;
            float started = Time.time, deadline = started + 180, dashAt = 0;
            while (session.Flow.Phase == RunFlowPhase.Dungeon && Time.time < deadline)
            {
                var dungeon = session.Dungeon; var player = dungeon.Player;
                var combat = player.GetComponent<PlayerCombat>(); var dash = player.GetComponent<PlayerDash>();
                evolved |= player.GetComponent<WeaponController>().Progress.Evolved;
                if (lastRoom != dungeon.Run.RoomIndex) { lastRoom = dungeon.Run.RoomIndex; visited.Add(lastRoom); stage = 0; }
                Vector2 position = player.transform.position;
                var target = dungeon.Room.Enemies.Where(e => e != null && !e.IsDead)
                    .OrderBy(e => (e.transform.position - player.transform.position).sqrMagnitude).FirstOrDefault();
                Vector2 delta = target != null ? (Vector2)target.transform.position - position : Vector2.right;
                if (dagger && stage == 2 && !combat.IsAttacking && dash.RequestDash(new Vector2(-delta.normalized.y, delta.normalized.x))) { stage = 3; dashAt = Time.time; }
                if (dagger && stage == 3 && Time.time - dashAt > 0.18f && combat.RequestAttack(delta)) stage = 0;
                if (dungeon.Room.IsClear)
                {
                    if (dagger && stage == 1 && combat.RequestAttack(delta)) stage = 2;
                    if (dagger && stage != 0) { Move(Vector2.zero); yield return null; continue; }
                    if (dungeon.PendingRewards.Length > 0) Assert.That(dungeon.ChooseReward(0), Is.True);
                    var exit = dungeon.Room.exit;
                    if (dungeon.Room.TryGetComponent<ShortcutController>(out var choice) && dangerous) exit = choice.dangerousExit;
                    Vector2 destination = exit.transform.position;
                    if (lastRoom == 3 && position.x < 3.75f) destination = new Vector2(4, 0);
                    Move(PlaytestNavigation.Direction(position, destination)); yield return null; continue;
                }
                if (target == null) { yield return null; continue; }
                Vector2 movement = dagger ? (delta.magnitude > 1.3f ? delta.normalized : Vector2.zero) :
                    delta.magnitude > 4.2f ? delta.normalized : delta.magnitude < 2.8f ? -delta.normalized : Vector2.zero;
                foreach (var hazard in Object.FindObjectsByType<TelegraphedHazard>(FindObjectsSortMode.None))
                {
                    Vector2 away = position - (Vector2)hazard.transform.position;
                    if (away.magnitude < 2) movement = away.sqrMagnitude > 0.01f ? away.normalized : Vector2.up;
                }
                if (dungeon.Boss != null && dungeon.Boss.Phase == BossPhase.ChargeWarning)
                {
                    Vector2 facing = dungeon.Boss.GetComponent<EnemyAttack>().Direction;
                    movement = new Vector2(-facing.y, facing.x);
                    Vector2 landing = position + movement * 3;
                    if (Mathf.Abs(landing.x) > 7 || Mathf.Abs(landing.y) > 3.5f) movement = -movement;
                    if (!dagger) dash.RequestDash(movement);
                }
                if (Mathf.Abs(position.x) > 7 && Mathf.Sign(movement.x) == Mathf.Sign(position.x)) movement.x = -Mathf.Sign(position.x);
                if (Mathf.Abs(position.y) > 3.6f && Mathf.Sign(movement.y) == Mathf.Sign(position.y)) movement.y = -Mathf.Sign(position.y);
                Vector2 movementTarget = dagger && delta.magnitude > 1.3f ? (Vector2)target.transform.position : position + movement * 2;
                Move(movement.sqrMagnitude > 0 ? PlaytestNavigation.Direction(position, movementTarget) : Vector2.zero);
                InputSystem.QueueStateEvent(mouse, new MouseState { position = Camera.main.WorldToScreenPoint(target.transform.position) });
                if (!dagger) combat.RequestAttack(delta);
                else if (stage < 2 && delta.magnitude < 1.7f && combat.RequestAttack(delta)) stage++;
                yield return null;
            }
            Move(Vector2.zero); yield return null;
            Assert.That(session.Flow.Phase, Is.EqualTo(RunFlowPhase.Result), $"Run timed out in room {session.Dungeon.Run.RoomIndex} at {session.Dungeon.Player.transform.position}; stage={stage}");
            TestContext.WriteLine($"SLICE_RUN route={(dangerous ? "danger" : "safe")} weapon={(dagger ? "dagger" : "bow")} outcome={session.Flow.Result.Outcome} gold={session.Flow.Result.EarnedGold} hp={session.Dungeon.Player.Current} evolved={evolved} rooms={string.Join(",", visited)} seconds={Time.time - started:F2}");
            if (!dagger) Assert.That(visited, Does.Contain(dangerous ? 7 : 5));
        }
        [UnityTest, Timeout(600000)]
        public IEnumerator PhysicalSafeAndShortcutVictoriesRebirthNextRunAndReload()
        {
            var session = Object.FindFirstObjectByType<GameBootstrap>(); session.OpenHub(); yield return Ready(session); session.SelectWeapon(3);
            yield return PlayRun(session, false); Assert.That(session.Flow.Result.Outcome, Is.EqualTo(RunOutcome.Victory));
            session.OpenHub(); yield return Ready(session);
            yield return PlayRun(session, true); Assert.That(session.Flow.Result.Outcome, Is.EqualTo(RunOutcome.Victory));
            session.OpenHub(); yield return Ready(session); Assert.That(session.Rebirth(), Is.True);
            string directory = Path.GetDirectoryName(session.SavePath); int bank = session.Profile.bankGold;
            session.StartRun(); yield return Ready(session); Assert.That(session.Dungeon.WorldThreat, Is.EqualTo(1));
            Assert.That(session.Dungeon.PermanentGoldMultiplier, Is.EqualTo(2));
            Object.Destroy(session.gameObject); yield return null;
            yield return SaveSessionFixture.Load(directory); session = Object.FindFirstObjectByType<GameBootstrap>();
            Assert.That(session.Profile.rebirthCount, Is.EqualTo(1)); Assert.That(session.Profile.bankGold, Is.EqualTo(bank));
            Assert.That(session.Flow.Phase, Is.EqualTo(RunFlowPhase.MainMenu));
        }
        [UnityTest, Timeout(300000)]
        public IEnumerator DaggerEvolvesThroughCombatActionsAgainstActiveEnemies()
        {
            evolved = false; var session = Object.FindFirstObjectByType<GameBootstrap>(); session.OpenHub(); yield return Ready(session);
            yield return PlayRun(session, false, true);
            Assert.That(evolved, Is.True, "Three qualifying combos must be achievable without freezing enemies, stat edits or teleports.");
            Assert.That(session.Flow.Result.Outcome, Is.EqualTo(RunOutcome.Victory));
            session.OpenHub(); yield return Ready(session); Assert.That(session.Flow.Phase, Is.EqualTo(RunFlowPhase.Hub));
        }
        [UnityTest, Timeout(120000)]
        public IEnumerator ActualEnemyDamageEndsRunAndReturnsToHub()
        {
            var session = Object.FindFirstObjectByType<GameBootstrap>(); session.OpenHub(); yield return Ready(session);
            session.StartRun(); yield return Ready(session); Move(Vector2.zero);
            float deadline = Time.time + 90;
            while (session.Flow.Phase == RunFlowPhase.Dungeon && Time.time < deadline) yield return null;
            Assert.That(session.Flow.Phase, Is.EqualTo(RunFlowPhase.Result)); Assert.That(session.Flow.Result.Outcome, Is.EqualTo(RunOutcome.Death));
            session.OpenHub(); yield return Ready(session); Assert.That(session.Flow.Phase, Is.EqualTo(RunFlowPhase.Hub));
        }
    }
}
