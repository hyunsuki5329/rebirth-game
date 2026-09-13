using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Roguelite.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Roguelite.Tests
{
    public class RebirthSessionTests
    {
        private Keyboard keyboard;
        private Mouse mouse;
        private float timeScale;
        private InputSettings.BackgroundBehavior background;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode editorBehavior;
#endif
        [UnitySetUp]
        public IEnumerator Setup()
        {
            timeScale = Time.timeScale; Time.timeScale = 3;
            background = InputSystem.settings.backgroundBehavior; InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            editorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            keyboard = InputSystem.AddDevice<Keyboard>(); mouse = InputSystem.AddDevice<Mouse>(); yield return null;
        }
        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(mouse); Time.timeScale = timeScale;
            InputSystem.settings.backgroundBehavior = background;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = editorBehavior;
#endif
            foreach (var session in Object.FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None)) Object.Destroy(session.gameObject);
            yield return null; SaveSessionFixture.Cleanup();
        }
        private IEnumerator Ready(GameBootstrap s)
        {
            float until = Time.realtimeSinceStartup + 15;
            while (s.IsLoading && Time.realtimeSinceStartup < until) yield return null;
            Assert.That(s.IsLoading, Is.False); yield return null;
        }
        private void Move(Vector2 direction, Vector2 position)
        {
            if (position.x < -7 && direction.x < 0 || position.x > 7 && direction.x > 0) direction.x = -Mathf.Sign(position.x);
            if (position.y < -3.6f && direction.y < 0 || position.y > 3.6f && direction.y > 0) direction.y = -Mathf.Sign(position.y);
            var keys = new List<Key>();
            if (direction.x > 0.2f) keys.Add(Key.D); else if (direction.x < -0.2f) keys.Add(Key.A);
            if (direction.y > 0.2f) keys.Add(Key.W); else if (direction.y < -0.2f) keys.Add(Key.S);
            if (keys.Count > 0) keys.Add(Key.LeftShift);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys.ToArray()));
        }
        private IEnumerator FightRun(GameBootstrap session)
        {
            Assert.That(session.StartRun(), Is.True); yield return Ready(session);
            float start = Time.time, until = start + 180;
            while (session.Flow.Phase == RunFlowPhase.Dungeon && Time.time < until)
            {
                var dungeon = session.Dungeon; var player = dungeon.Player;
                if (dungeon.Room.IsClear)
                {
                    Move(Vector2.zero, player.transform.position);
                    if (dungeon.PendingRewards.Length > 0) dungeon.ChooseReward(0);
                    dungeon.Advance(false); yield return null; continue;
                }
                var target = dungeon.Room.Enemies.Where(e => e != null && !e.IsDead)
                    .OrderBy(e => (e.transform.position - player.transform.position).sqrMagnitude).FirstOrDefault();
                if (target == null) { yield return null; continue; }
                Vector2 position = player.transform.position, delta = target.transform.position - player.transform.position;
                Vector2 move = delta.magnitude > 4.2f ? delta.normalized : delta.magnitude < 2.8f ? -delta.normalized : Vector2.zero;
                foreach (var hazard in Object.FindObjectsByType<TelegraphedHazard>(FindObjectsSortMode.None))
                {
                    Vector2 away = position - (Vector2)hazard.transform.position;
                    if (away.magnitude < 2) move = away.sqrMagnitude > 0.01f ? away.normalized : position.sqrMagnitude > 0.01f ? -position.normalized : Vector2.up;
                }
                if (dungeon.Boss != null && dungeon.Boss.Phase == BossPhase.ChargeWarning)
                {
                    var direction = dungeon.Boss.GetComponent<EnemyAttack>().Direction;
                    move = new Vector2(-direction.y, direction.x);
                    var landing = position + move * 3;
                    if (Mathf.Abs(landing.x) > 7 || Mathf.Abs(landing.y) > 3.5f) move = -move;
                    player.GetComponent<PlayerDash>().RequestDash(move);
                }
                Move(move, position);
                InputSystem.QueueStateEvent(mouse, new MouseState { position = Camera.main.WorldToScreenPoint(target.transform.position) });
                player.GetComponent<PlayerCombat>().RequestAttack(delta);
                yield return null;
            }
            Move(Vector2.zero, Vector2.zero); yield return null;
            Assert.That(session.Flow.Phase, Is.EqualTo(RunFlowPhase.Result), "Combat strategy exceeded time limit.");
            TestContext.WriteLine($"BALANCE_RUN threat={session.Profile.rebirthCount} outcome={session.Flow.Result.Outcome} seconds={Time.time - start:F2} baseGold={session.Flow.Result.BaseGold} earned={session.Flow.Result.EarnedGold} hp={session.Dungeon.Player.Current:F0}");
        }
        [UnityTest, Timeout(600000)]
        public IEnumerator ThreeFreshProfilesReachFirstRebirthThenEarnFasterOnNextRunWithoutRewardCheats()
        {
            for (int sample = 0; sample < 3; sample++)
            {
                yield return SaveSessionFixture.Load();
                var session = Object.FindFirstObjectByType<GameBootstrap>();
                Assert.That(session.Profile.bankGold, Is.Zero); session.OpenHub(); yield return Ready(session); session.SelectWeapon(3);
                int successes = 0, deaths = 0, baseline = 0;
                for (int attempt = 0; attempt < 4 && !session.CanRebirth; attempt++)
                {
                    yield return FightRun(session);
                    if (session.Flow.Result.Outcome == RunOutcome.Victory) { successes++; baseline = session.Flow.Result.EarnedGold; } else deaths++;
                    session.OpenHub(); yield return Ready(session);
                }
                Assert.That(session.CanRebirth, Is.True); Assert.That(successes, Is.InRange(1, 2));
                Assert.That(session.Rebirth(), Is.True); Assert.That(session.Profile.rebirthCount, Is.EqualTo(1));
                int targetCost = session.RebirthRules.Cost(session.Profile);
                double beforeRuns = System.Math.Ceiling((double)targetCost / baseline);
                double afterRuns = System.Math.Ceiling(targetCost / (baseline * (double)session.Profile.permanentGoldMultiplier));
                Assert.That(afterRuns, Is.LessThan(beforeRuns));
                yield return FightRun(session);
                Assert.That(session.Flow.Result.Outcome, Is.EqualTo(RunOutcome.Victory));
                Assert.That(session.Flow.Result.EarnedGold, Is.GreaterThan(baseline));
                TestContext.WriteLine($"BALANCE_SESSION sample={sample + 1} successesToFirstRebirth={successes} deaths={deaths} beforeEarned={baseline} afterEarned={session.Flow.Result.EarnedGold} commonTarget={targetCost} expectedRunsBefore={beforeRuns} expectedRunsAfter={afterRuns}");
                Object.Destroy(session.gameObject); yield return null;
            }
        }
    }
}
