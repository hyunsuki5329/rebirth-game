using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Roguelite.Tests
{
    public class BowTests
    {
        [UnityTest]
        public IEnumerator ArrowStopsAtWallAndWeaponSwitchRemovesFlyingArrow()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            foreach (var enemy in dungeon.Room.Enemies) { enemy.GetComponent<EnemyAI>().enabled = false; enemy.transform.position = new Vector3(0, 3, 0); }
            var target = dungeon.Room.Enemies[0]; target.transform.position = new Vector3(-10, 0, 0);
            var weapons = dungeon.Player.GetComponent<WeaponController>(); weapons.Select(3);
            var combat = dungeon.Player.GetComponent<PlayerCombat>(); float hp = target.Current;
            combat.RequestAttack(Vector2.left);
            yield return new WaitForSeconds(0.55f);
            Assert.That(target.Current, Is.EqualTo(hp));
            Assert.That(Object.FindFirstObjectByType<Projectile>(), Is.Null);
            combat.RequestAttack(Vector2.right);
            yield return new WaitForSeconds(0.52f);
            var arrow = Object.FindFirstObjectByType<Projectile>(); Assert.That(arrow, Is.Not.Null);
            Assert.That(weapons.Select(0), Is.True);
            Assert.That(arrow.gameObject.activeSelf, Is.False);
            yield return null; Assert.That(arrow == null, Is.True);
        }
        [UnityTest]
        public IEnumerator ArrowLifetimeAndNewRunCleanupAreBounded()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            foreach (var enemy in dungeon.Room.Enemies) enemy.gameObject.SetActive(false);
            dungeon.Player.GetComponent<WeaponController>().Select(3);
            var combat = dungeon.Player.GetComponent<PlayerCombat>();
            var original = combat.attack;
            var shortLived = Object.Instantiate(original); shortLived.projectileSpeed = 1; shortLived.projectileLifetime = 0.05f;
            combat.attack = shortLived; combat.RequestAttack(Vector2.right);
            yield return new WaitForSeconds(0.3f);
            Assert.That(Object.FindFirstObjectByType<Projectile>(), Is.Null);
            yield return new WaitForSeconds(0.25f);
            combat.attack = original; Object.Destroy(shortLived); combat.RequestAttack(Vector2.right);
            yield return new WaitForSeconds(0.2f);
            var arrow = Object.FindFirstObjectByType<Projectile>(); Assert.That(arrow, Is.Not.Null);
            dungeon.BeginRun(123);
            Assert.That(arrow.gameObject.activeSelf, Is.False);
            yield return null; Assert.That(arrow == null, Is.True);
        }

        [UnityTest]
        public IEnumerator BowIsFourthWeaponAndArrowDealsOneHitWithMomentum()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            foreach (var enemy in dungeon.Room.Enemies) enemy.GetComponent<EnemyAI>().enabled = false;
            var weapons = dungeon.Player.GetComponent<WeaponController>();
            Assert.That(weapons.weapons.Length, Is.EqualTo(4));
            Assert.That(weapons.Select(3), Is.True);
            Assert.That(weapons.Current.id, Is.EqualTo("bow"));
            var target = dungeon.Room.Enemies[0];
            target.transform.position = dungeon.Player.transform.position + Vector3.right * 3;
            Physics2D.SyncTransforms();
            float before = target.Current;
            Assert.That(dungeon.Player.GetComponent<PlayerCombat>().RequestAttack(Vector2.right), Is.True);
            yield return new WaitForSeconds(0.45f);
            Assert.That(target.Current, Is.EqualTo(before - weapons.Current.attack.damage).Within(0.001));
            Assert.That(dungeon.Player.GetComponent<MomentumSystem>().Model.Value, Is.GreaterThan(0));
            Assert.That(Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length, Is.Zero);
        }
    }
}
