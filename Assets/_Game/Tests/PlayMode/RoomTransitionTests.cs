using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Roguelite.Tests
{
    public class RoomTransitionTests
    {
        [UnityTest]
        public IEnumerator RestartRemovesEnemyProjectileAndOldActors()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena");
            yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            var oldPlayer = dungeon.Player;
            oldPlayer.Receive(10, Vector2.zero);
            var enemy = Object.Instantiate(dungeon.combatRooms[0].definition.enemies[1].prefab, dungeon.Room.transform);
            enemy.enabled = false;
            enemy.GetComponent<EnemyAttack>().TryBegin(Vector2.up);
            yield return new WaitForSeconds(enemy.data.attack.windup + 0.05f);
            var projectile = Object.FindFirstObjectByType<Projectile>();
            Assert.That(projectile, Is.Not.Null);
            var projectileObject = projectile.gameObject;
            dungeon.BeginRun(123);
            Assert.That(projectileObject.activeSelf, Is.False);
            yield return null;
            Assert.That(projectileObject == null, Is.True);
            Assert.That(enemy == null, Is.True);
            Assert.That(oldPlayer == null, Is.True);
            Assert.That(dungeon.Player.Current, Is.EqualTo(dungeon.Player.Maximum));
        }

        [UnityTest]
        public IEnumerator ExitTriggerAdvancesOnlyAfterClearAndDeathPreventsAdvance()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena");
            yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            var motor = dungeon.Player.GetComponent<PlayerMotor2D>();
            motor.Teleport(dungeon.Room.exit.transform.position);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.That(dungeon.Run.RoomIndex, Is.Zero);
            motor.Teleport(Vector2.zero);
            yield return new WaitForFixedUpdate();
            foreach (var enemy in dungeon.Room.Enemies) enemy.Receive(10000, Vector2.zero);
            motor.Teleport(dungeon.Room.exit.transform.position);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.That(dungeon.Run.RoomIndex, Is.EqualTo(1));
            foreach (var enemy in dungeon.Room.Enemies) enemy.Receive(10000, Vector2.zero);
            dungeon.Player.Receive(10000, Vector2.zero);
            Assert.That(dungeon.Advance(), Is.False);
        }

        [UnityTest]
        public IEnumerator ThreeCombatRoomsRequireKillsPayOnceAndRestartResets()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena");
            yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            int combatRooms = 0;
            while (combatRooms < 3)
            {
                var room = dungeon.Room;
                if (room.Enemies.Count > 0)
                {
                    Assert.That(dungeon.Advance(), Is.False);
                    Assert.That(room.exit.IsOpen, Is.False);
                    int before = dungeon.Run.Gold, clears = 0;
                    room.Cleared += () => clears++;
                    foreach (var enemy in room.Enemies) enemy.Receive(10000, Vector2.zero);
                    Assert.That(room.IsClear, Is.True);
                    Assert.That(room.exit.IsOpen, Is.True);
                    Assert.That(clears, Is.EqualTo(1));
                    yield return null; // Reward settles after final hit/kill event callbacks.
                    Assert.That(dungeon.Run.Gold, Is.EqualTo(before + Roguelite.Core.RewardCalculator.Gold(room.definition.gold, dungeon.Rewards.GoldMultiplier)));
                    Assert.That(dungeon.Rewards.GrantRoom(dungeon.Run, dungeon.Run.RoomIndex, 999), Is.False);
                    combatRooms++;
                }
                Assert.That(dungeon.Advance(), Is.True);
                yield return null;
                Assert.That(room == null, Is.True, "Previous room and its enemies must be destroyed.");
            }
            string oldRun = dungeon.Run.Id;
            dungeon.BeginRun(123);
            yield return null;
            Assert.That(dungeon.Run.Gold, Is.Zero);
            Assert.That(dungeon.Run.RoomIndex, Is.Zero);
            Assert.That(dungeon.Run.Id, Is.Not.EqualTo(oldRun));
            Assert.That(Object.FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
        }
        [UnityTest]
        public IEnumerator TransitionDisablesPlayerAttackEffectsImmediately()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena");
            yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            foreach (var enemy in dungeon.Room.Enemies) enemy.Receive(10000, Vector2.zero);
            dungeon.Player.GetComponent<PlayerCombat>().RequestAttack(Vector2.right);
            Assert.That(Object.FindObjectsByType<AttackVisual>(FindObjectsSortMode.None).Length, Is.GreaterThan(0));
            Assert.That(dungeon.Advance(), Is.True);
            Assert.That(Object.FindObjectsByType<AttackVisual>(FindObjectsSortMode.None).Length, Is.Zero);
            yield return null;
        }
    }
}
