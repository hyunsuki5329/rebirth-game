using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Roguelite.Tests
{
    public class MomentumComboPlayTests
    {
        [UnityTest]
        public IEnumerator RoomTransitionCancelsActiveDashWithoutResettingCooldown()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            foreach (var enemy in dungeon.Room.Enemies) enemy.Receive(10000, Vector2.zero);
            var dash = dungeon.Player.GetComponent<PlayerDash>();
            dash.RequestDash(Vector2.right);
            Assert.That(dash.IsActive, Is.True);
            dungeon.Advance();
            Assert.That(dash.IsActive, Is.False);
            Assert.That(dash.CooldownRemaining, Is.GreaterThan(0));
        }

        [UnityTest]
        public IEnumerator FinalRealHitContributesToExactRoomPayout()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            foreach (var enemy in dungeon.Room.Enemies) enemy.GetComponent<EnemyAI>().enabled = false;
            dungeon.Room.Enemies[1].Receive(10000, Vector2.zero);
            var target = dungeon.Room.Enemies[0];
            target.Receive(target.Current - 1, Vector2.zero);
            target.transform.position = dungeon.Player.transform.position + Vector3.right;
            Physics2D.SyncTransforms();
            var momentum = dungeon.Player.GetComponent<MomentumSystem>();
            momentum.Model.Add(60, Time.timeAsDouble);
            dungeon.Player.GetComponent<PlayerCombat>().RequestAttack(Vector2.right);
            yield return new WaitForSeconds(0.2f);
            Assert.That(momentum.Model.Value, Is.EqualTo(84).Within(0.01));
            Assert.That(dungeon.Run.Gold, Is.EqualTo(14), "60 initial + fast clear 10 + final hit 4 + kill 10 produces x1.42, flooring 10 gold to 14.");
        }

        [UnityTest]
        public IEnumerator BothCombosCompleteAlongsideRealEnemyHits()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            foreach (var enemy in dungeon.Room.Enemies) enemy.GetComponent<EnemyAI>().enabled = false;
            var first = dungeon.Room.Enemies[0]; var second = dungeon.Room.Enemies[1];
            var player = dungeon.Player; var dash = player.GetComponent<PlayerDash>();
            var combo = player.GetComponent<ComboSystem>();
            yield return AttackTarget(player, first);
            yield return AttackTarget(player, first);
            Assert.That(dash.RequestDash(Vector2.up), Is.True);
            yield return new WaitForSeconds(0.2f);
            yield return AttackTarget(player, first);
            Assert.That(combo.LastCompleted, Is.EqualTo("dagger-chain"));
            yield return new WaitForSeconds(0.4f);
            Assert.That(dash.RequestDash(Vector2.down), Is.True);
            yield return new WaitForSeconds(0.2f);
            yield return AttackTarget(player, second);
            yield return AttackTarget(player, second);
            Assert.That(combo.LastCompleted, Is.EqualTo("dash-chain"));
            Assert.That(combo.CompletedCount, Is.EqualTo(2));
            Assert.That(second == null || second.Current < second.Maximum, Is.True);
        }
        private static IEnumerator AttackTarget(PlayerHealth player, EnemyHealth target)
        {
            target.transform.position = player.transform.position + Vector3.right;
            Physics2D.SyncTransforms();
            Assert.That(player.GetComponent<PlayerCombat>().RequestAttack(Vector2.right), Is.True);
            yield return new WaitForSeconds(0.45f);
        }

        [UnityTest]
        public IEnumerator InjectedClockFreezesAndResumesMomentumDecay()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            foreach (var enemy in dungeon.Room.Enemies) enemy.gameObject.SetActive(false);
            var momentum = dungeon.Player.GetComponent<MomentumSystem>();
            double now = Time.timeAsDouble;
            momentum.Clock = () => now;
            momentum.Model.Add(100, now);
            now += momentum.config.grace + 1;
            yield return null;
            float afterDecay = momentum.Model.Value;
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.That(momentum.Model.Value, Is.EqualTo(afterDecay));
            now += 1; yield return null;
            Assert.That(momentum.Model.Value, Is.EqualTo(afterDecay - momentum.config.decay).Within(0.01));
            var combo = dungeon.Player.GetComponent<ComboSystem>(); combo.Clock = () => now;
            dungeon.Player.GetComponent<PlayerDash>().RequestDash(Vector2.up);
            yield return new WaitForSeconds(1.1f);
            var combat = dungeon.Player.GetComponent<PlayerCombat>();
            combat.RequestAttack(Vector2.left);
            yield return new WaitForSeconds(0.45f);
            combat.RequestAttack(Vector2.left);
            Assert.That(combo.LastCompleted, Is.EqualTo("dash-chain"), "Injected combat time did not advance despite wall-clock delay.");
        }

        [UnityTest]
        public IEnumerator DashBonusRequiresPostDashHitAndIsConsumedOnce()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            foreach (var enemy in dungeon.Room.Enemies) enemy.GetComponent<EnemyAI>().enabled = false;
            var target = dungeon.Room.Enemies[0];
            target.transform.position = new Vector3(5, 3, 0);
            var player = dungeon.Player;
            var momentum = player.GetComponent<MomentumSystem>();
            var combat = player.GetComponent<PlayerCombat>();
            Assert.That(player.GetComponent<PlayerDash>().RequestDash(Vector2.right), Is.True);
            yield return new WaitForSeconds(0.2f);
            Assert.That(momentum.Model.Value, Is.Zero);
            target.transform.position = player.transform.position + Vector3.right;
            Physics2D.SyncTransforms();
            combat.RequestAttack(Vector2.right);
            yield return new WaitForSeconds(0.2f);
            Assert.That(momentum.Model.Value, Is.EqualTo(12).Within(0.01), "Hit 4 + one post-dash bonus 8.");
            yield return new WaitForSeconds(0.25f);
            target.transform.position = player.transform.position + Vector3.right;
            Physics2D.SyncTransforms();
            Assert.That(combat.RequestAttack(Vector2.right), Is.True);
            yield return new WaitForSeconds(0.15f);
            Assert.That(momentum.Model.Value, Is.EqualTo(28).Within(0.01), "Second hit 4 + combo 12; no second dash bonus.");
        }

        [UnityTest]
        public IEnumerator AcceptedActionsRecognizeBothCombosButAirSpamGivesNoMomentum()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            foreach (var enemy in dungeon.Room.Enemies) enemy.gameObject.SetActive(false);
            var combat = dungeon.Player.GetComponent<PlayerCombat>();
            var dash = dungeon.Player.GetComponent<PlayerDash>();
            var combo = dungeon.Player.GetComponent<ComboSystem>();
            var momentum = dungeon.Player.GetComponent<MomentumSystem>();
            Assert.That(combat.RequestAttack(Vector2.left), Is.True);
            Assert.That(combat.RequestAttack(Vector2.left), Is.False);
            yield return new WaitForSeconds(0.45f);
            Assert.That(combat.RequestAttack(Vector2.left), Is.True);
            Assert.That(dash.RequestDash(Vector2.right), Is.True);
            Assert.That(dash.RequestDash(Vector2.right), Is.False);
            yield return new WaitForSeconds(0.45f);
            Assert.That(combat.RequestAttack(Vector2.left), Is.True);
            Assert.That(combo.LastCompleted, Is.EqualTo("dagger-chain"));
            yield return new WaitForSeconds(0.6f);
            Assert.That(dash.RequestDash(Vector2.left), Is.True);
            Assert.That(combat.RequestAttack(Vector2.left), Is.True);
            yield return new WaitForSeconds(0.45f);
            Assert.That(combat.RequestAttack(Vector2.left), Is.True);
            Assert.That(combo.LastCompleted, Is.EqualTo("dash-chain"));
            Assert.That(combo.CompletedCount, Is.EqualTo(2));
            Assert.That(momentum.Model.Value, Is.Zero);
            Assert.That(momentum.GoldMultiplier, Is.EqualTo(1));
        }
        [UnityTest]
        public IEnumerator RealHitsRaiseAttackSpeedAndGoldAndNewRunResets()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            foreach (var enemy in dungeon.Room.Enemies) enemy.GetComponent<EnemyAI>().enabled = false;
            var target = dungeon.Room.Enemies[0];
            target.transform.position = dungeon.Player.transform.position + Vector3.right;
            Physics2D.SyncTransforms();
            var combat = dungeon.Player.GetComponent<PlayerCombat>();
            var momentum = dungeon.Player.GetComponent<MomentumSystem>();
            Assert.That(combat.RequestAttack(Vector2.right), Is.True);
            yield return new WaitForSeconds(0.2f);
            Assert.That(target.Current, Is.LessThan(target.Maximum));
            Assert.That(momentum.Model.Value, Is.GreaterThan(0));
            Assert.That(combat.SpeedMultiplier, Is.GreaterThan(1));
            Assert.That(momentum.GoldMultiplier, Is.GreaterThan(1));
            dungeon.BeginRun(123); yield return null;
            Assert.That(dungeon.Player.GetComponent<MomentumSystem>().Model.Value, Is.Zero);
            Assert.That(dungeon.Player.GetComponent<ComboSystem>().CompletedCount, Is.Zero);
        }
    }
}
