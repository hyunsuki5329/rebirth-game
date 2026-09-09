using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Roguelite.Tests
{
    public class WeaponPlayTests
    {
        [UnityTest]
        public IEnumerator WeaponSelectionChangesCombatAndItemsRespectCapAndReset()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            var controller = dungeon.Player.GetComponent<WeaponController>();
            var combat = dungeon.Player.GetComponent<PlayerCombat>();
            var dagger = combat.attack;
            Assert.That(controller.Select(1), Is.True); var sword = combat.attack;
            Assert.That(controller.Select(2), Is.True); var greatsword = combat.attack;
            Assert.That(dagger.windup, Is.LessThan(sword.windup));
            Assert.That(sword.range, Is.LessThan(greatsword.range));
            Assert.That(greatsword.knockback, Is.GreaterThan(sword.knockback));
            Assert.That(greatsword.movementMultiplier, Is.LessThan(dagger.movementMultiplier));
            var inventory = dungeon.Player.GetComponent<RunInventory>();
            var item = dungeon.rewardItems[0];
            for (int i = 0; i < 3; i++) Assert.That(inventory.Add(item), Is.True);
            Assert.That(inventory.Add(item), Is.False);
            Assert.That(combat.DamageMultiplier, Is.EqualTo(1.45f).Within(0.001));
            Assert.That(dagger.damage, Is.EqualTo(12), "Runtime items must not mutate shared weapon assets.");
            dungeon.BeginRun(123); yield return null;
            Assert.That(dungeon.Player.GetComponent<RunInventory>().Count(item.id), Is.Zero);
            Assert.That(dungeon.Player.GetComponent<PlayerCombat>().DamageMultiplier, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ThreeCombatCombosEvolveDaggerAndSpawnPiercingWaveInOneRun()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            var player = dungeon.Player;
            var controller = player.GetComponent<WeaponController>();
            int evolutions = 0; controller.Evolved += () => evolutions++;
            for (int combo = 0; combo < 3; combo++)
            {
                foreach (var enemy in dungeon.Room.Enemies) enemy.GetComponent<EnemyAI>().enabled = false;
                var target = dungeon.Room.Enemies[0];
                yield return Attack(player, target); yield return Attack(player, target);
                Assert.That(player.GetComponent<PlayerDash>().RequestDash(Vector2.up), Is.True);
                yield return new WaitForSeconds(0.2f);
                target.transform.position = player.transform.position + Vector3.right; Physics2D.SyncTransforms();
                Assert.That(player.GetComponent<PlayerCombat>().RequestAttack(Vector2.right), Is.True);
                Assert.That(controller.Progress.ComboCount, Is.EqualTo(combo + 1));
                if (combo == 2)
                {
                    Assert.That(controller.Progress.Evolved, Is.True);
                    Assert.That(Object.FindFirstObjectByType<PiercingWave>(), Is.Not.Null);
                    Assert.That(Object.FindFirstObjectByType<PiercingWave>().GetComponent<SpriteRenderer>().sprite, Is.EqualTo(controller.Current.waveSprite));
                    Assert.That(controller.Current.waveSprite, Is.Not.Null);
                    Assert.That(controller.Current.waveSprite, Is.Not.EqualTo(player.GetComponent<SpriteRenderer>().sprite));
                    Assert.That(evolutions, Is.EqualTo(1));
                    break;
                }
                yield return new WaitForSeconds(0.45f);
                foreach (var enemy in dungeon.Room.Enemies)
                    while (enemy != null && !enemy.IsDead) yield return Attack(player, enemy);
                do
                {
                    if (dungeon.PendingRewards.Length > 0)
                    {
                        Assert.That(dungeon.Advance(), Is.False);
                        Assert.That(dungeon.ChooseReward(0), Is.True);
                        Assert.That(dungeon.ChooseReward(0), Is.False);
                    }
                    Assert.That(dungeon.Advance(), Is.True); yield return null;
                } while (dungeon.Room.Enemies.Count == 0);
            }
            dungeon.BeginRun(123); yield return null;
            Assert.That(dungeon.Player.GetComponent<WeaponController>().Progress.Evolved, Is.False);
        }
        private static IEnumerator Attack(PlayerHealth player, EnemyHealth target)
        {
            target.transform.position = player.transform.position + Vector3.right; Physics2D.SyncTransforms();
            Assert.That(player.GetComponent<PlayerCombat>().RequestAttack(Vector2.right), Is.True);
            yield return new WaitForSeconds(0.45f);
        }
    }
}
