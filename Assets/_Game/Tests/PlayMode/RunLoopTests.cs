using System.Collections;
using NUnit.Framework;
using Roguelite.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Roguelite.Tests
{
    public class RunLoopTests
    {
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var session in Object.FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None)) Object.Destroy(session.gameObject);
            yield return null;
        }
        private static IEnumerator Ready(GameBootstrap session, RunFlowPhase phase)
        {
            float deadline = Time.realtimeSinceStartup + 10;
            while ((session.IsLoading || session.Flow.Phase != phase) && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(session.IsLoading, Is.False); Assert.That(session.Flow.Phase, Is.EqualTo(phase)); yield return null;
        }
        [UnityTest]
        public IEnumerator MenuVictoryHubSecondRunDeathHubRetainsBankAndResetsRunState()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu"); yield return null; yield return null;
            var session = Object.FindFirstObjectByType<GameBootstrap>(); Assert.That(session, Is.Not.Null);
            Assert.That(session.Flow.Phase, Is.EqualTo(RunFlowPhase.MainMenu)); Assert.That(session.SelectWeapon(0), Is.False);
            Assert.That(session.OpenHub(), Is.True); Assert.That(session.OpenHub(), Is.False);
            yield return Ready(session, RunFlowPhase.Hub); Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Hub"));
            Assert.That(session.SelectWeapon(0), Is.True); Assert.That(session.StartRun(), Is.True); Assert.That(session.StartRun(), Is.False);
            yield return Ready(session, RunFlowPhase.Dungeon); Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Dungeon"));
            var first = session.Dungeon; var oldPlayer = first.Player; string firstId = first.Run.Id;
            first.Player.Receive(10, Vector2.zero);
            first.Player.GetComponent<RunInventory>().Add(first.rewardItems[0]);
            var progress = first.Player.GetComponent<WeaponController>().Progress;
            progress.RecordCombo(true); progress.RecordCombo(true); progress.RecordCombo(true); Assert.That(progress.Evolved, Is.True);
            while (session.Flow.Phase == RunFlowPhase.Dungeon)
            {
                foreach (var enemy in first.Room.Enemies) if (enemy != null) enemy.Receive(10000, Vector2.zero);
                yield return null;
                if (session.Flow.Phase != RunFlowPhase.Dungeon) break;
                if (first.PendingRewards.Length > 0) first.ChooseReward(0);
                Assert.That(first.Advance(true), Is.True); yield return null;
            }
            Assert.That(session.Flow.Result.Outcome, Is.EqualTo(RunOutcome.Victory));
            int bank = session.Profile.bankGold; Assert.That(bank, Is.EqualTo(first.Run.Gold)); Assert.That(bank, Is.GreaterThan(0));
            Assert.That(session.Flow.Finish(first.Run, RunOutcome.Victory), Is.False); Assert.That(session.Profile.bankGold, Is.EqualTo(bank));
            Assert.That(session.OpenHub(), Is.True); Assert.That(session.OpenHub(), Is.False); yield return Ready(session, RunFlowPhase.Hub);
            Assert.That(oldPlayer == null, Is.True); Assert.That(session.Profile.bankGold, Is.EqualTo(bank));
            Assert.That(session.SelectWeapon(3), Is.True); Assert.That(session.StartRun(), Is.True); yield return Ready(session, RunFlowPhase.Dungeon);
            var second = session.Dungeon; var player = second.Player;
            Assert.That(second.Run.Id, Is.Not.EqualTo(firstId)); Assert.That(second.Run.Gold, Is.Zero);
            Assert.That(player.Current, Is.EqualTo(player.Maximum)); Assert.That(player.GetComponent<MomentumSystem>().Model.Value, Is.Zero);
            Assert.That(player.GetComponent<WeaponController>().Current.id, Is.EqualTo("bow"));
            foreach (var item in second.rewardItems) Assert.That(player.GetComponent<RunInventory>().Count(item.id), Is.Zero);
            player.GetComponent<WeaponController>().Select(0); Assert.That(player.GetComponent<WeaponController>().Progress.Evolved, Is.False);
            Assert.That(player.GetComponent<WeaponController>().Progress.ComboCount, Is.Zero);
            foreach (var enemy in second.Room.Enemies) enemy.Receive(10000, Vector2.zero); yield return null;
            int earned = second.Run.Gold; Assert.That(earned, Is.GreaterThan(0));
            player.Receive(10000, Vector2.zero); yield return null;
            Assert.That(session.Flow.Result.Outcome, Is.EqualTo(RunOutcome.Death));
            Assert.That(session.Profile.bankGold, Is.EqualTo(bank + Mathf.FloorToInt(earned * 0.5f)));
            Assert.That(session.OpenHub(), Is.True); yield return Ready(session, RunFlowPhase.Hub);
            Assert.That(session.Profile.totalClears, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
        }
    }
}
