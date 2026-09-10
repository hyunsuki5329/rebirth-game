using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Roguelite.Tests
{
    public class EliteHazardTests
    {
        [UnityTest]
        public IEnumerator WarningDealsNoDamageThenHitsOnceAndCanBeDodged()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            foreach (var enemy in dungeon.Room.Enemies) { enemy.GetComponent<EnemyAI>().enabled = false; enemy.GetComponent<EnemyAttack>().enabled = false; }
            var owner = dungeon.Room.Enemies[0]; var player = dungeon.Player;
            var go = new GameObject("Test hazard", typeof(TelegraphedHazard)); go.transform.position = player.transform.position;
            go.GetComponent<TelegraphedHazard>().Initialize(owner, 1.3f, 0.3f, 14, 0.2f);
            owner.GetComponent<CombatEffects>().Track(go);
            float before = player.Current;
            yield return new WaitForSeconds(0.15f); Assert.That(player.Current, Is.EqualTo(before));
            yield return new WaitForSeconds(0.22f); Assert.That(player.Current, Is.EqualTo(before - 14));
            yield return new WaitForSeconds(0.2f); Assert.That(player.Current, Is.EqualTo(before - 14));
            go = new GameObject("Dodge hazard", typeof(TelegraphedHazard)); go.transform.position = player.transform.position;
            go.GetComponent<TelegraphedHazard>().Initialize(owner, 1.3f, 0.3f, 14, 0.2f); owner.GetComponent<CombatEffects>().Track(go);
            player.GetComponent<PlayerMotor2D>().Teleport(player.transform.position + Vector3.up * 2);
            yield return new WaitForSeconds(0.55f); Assert.That(player.Current, Is.EqualTo(before - 14));
        }
        [UnityTest]
        public IEnumerator EliteHazardCancelsOnOwnerDeathAndRestart()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            var enemy = dungeon.Room.Enemies[0]; enemy.GetComponent<EnemyAI>().enabled = false;
            enemy.GetComponent<EnemyAttack>().enabled = false;
            var config = ScriptableObject.CreateInstance<ShortcutRules>(); config.firstHazardDelay = 0;
            enemy.gameObject.AddComponent<EliteModifier>().Initialize(dungeon.Player, config);
            yield return null; yield return null;
            Assert.That(Object.FindFirstObjectByType<TelegraphedHazard>(), Is.Not.Null);
            float hp = dungeon.Player.Current; enemy.Receive(10000, Vector2.zero);
            yield return new WaitForSeconds(0.8f);
            Assert.That(Object.FindFirstObjectByType<TelegraphedHazard>(), Is.Null);
            Assert.That(dungeon.Player.Current, Is.EqualTo(hp));
            var other = dungeon.Room.Enemies[1]; other.GetComponent<EnemyAI>().enabled = false;
            other.gameObject.AddComponent<EliteModifier>().Initialize(dungeon.Player, config);
            yield return null; yield return null;
            var hazard = Object.FindFirstObjectByType<TelegraphedHazard>(); Assert.That(hazard, Is.Not.Null);
            dungeon.BeginRun(123); Assert.That(hazard.gameObject.activeInHierarchy, Is.False);
            yield return null; Assert.That(hazard == null, Is.True); Object.Destroy(config);
        }
    }
}
