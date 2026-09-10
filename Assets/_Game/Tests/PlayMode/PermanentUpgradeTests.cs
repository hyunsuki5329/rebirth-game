using System.Collections;
using NUnit.Framework;
using Roguelite.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Roguelite.Tests
{
    public class PermanentUpgradeTests
    {
        [UnityTearDown] public IEnumerator Cleanup() { foreach (var s in Object.FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None)) Object.Destroy(s.gameObject); yield return null; }
        private IEnumerator Ready(GameBootstrap s) { while (s.IsLoading) yield return null; yield return null; }
        [UnityTest]
        public IEnumerator HubRebirthAppliesBothUpgradesWithoutMutatingPlayerAsset()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu"); yield return null; yield return null;
            var session = Object.FindFirstObjectByType<GameBootstrap>(); session.OpenHub(); yield return Ready(session);
            session.Profile.totalClears = 1; session.Profile.bankGold = 300;
            Assert.That(session.Rebirth(), Is.True); Assert.That(session.BuyUpgrade(0), Is.True); Assert.That(session.BuyUpgrade(0), Is.False);
            Assert.That(session.BuyUpgrade(1), Is.False); Assert.That(session.Rebirth(), Is.True); Assert.That(session.BuyUpgrade(1), Is.True);
            session.StartRun(); yield return Ready(session);
            var player = session.Dungeon.Player;
            Assert.That(player.Maximum, Is.EqualTo(player.stats.maximumHealth + 20)); Assert.That(player.stats.maximumHealth, Is.EqualTo(100));
            Assert.That(player.GetComponent<ComboSystem>().PermanentWindowBonus, Is.EqualTo(0.25f));
            Assert.That(session.Dungeon.WorldThreat, Is.EqualTo(2)); Assert.That(session.Rebirth(), Is.False); Assert.That(session.BuyUpgrade(0), Is.False);
            var combo = player.GetComponent<ComboSystem>(); double now = 0; combo.Clock = () => now;
            Assert.That(player.GetComponent<PlayerDash>().RequestDash(Vector2.up), Is.True);
            now = 1.1; Assert.That(player.GetComponent<PlayerCombat>().RequestAttack(Vector2.right), Is.True);
            yield return new WaitForSeconds(0.5f);
            now = 2.2; Assert.That(player.GetComponent<PlayerCombat>().RequestAttack(Vector2.right), Is.True);
            Assert.That(combo.LastCompleted, Is.EqualTo("dash-chain"));
            player.Receive(10000, Vector2.zero); yield return null;
            Assert.That(session.Flow.Phase, Is.EqualTo(RunFlowPhase.Result)); session.OpenHub(); yield return Ready(session);
            session.StartRun(); yield return Ready(session);
            Assert.That(session.Profile.rebirthCount, Is.EqualTo(2)); Assert.That(session.Profile.upgradeIds.Length, Is.EqualTo(2));
            Assert.That(session.Dungeon.Player.Current, Is.EqualTo(120)); Assert.That(session.Profile.permanentGoldMultiplier, Is.EqualTo(2.7f));
        }
    }
}
