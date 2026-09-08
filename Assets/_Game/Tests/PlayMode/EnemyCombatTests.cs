using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Roguelite.Tests
{
    public class EnemyCombatTests
    {
        private GameObject player;
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("EnemyArena");
            player = GameObject.Find("Player");
        }
        private GameObject Only(string name)
        {
            GameObject result = null;
            foreach (var enemy in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
                if (enemy.name == name) result = enemy.gameObject; else enemy.gameObject.SetActive(false);
            return result;
        }
        [UnityTest]
        public IEnumerator ChaserApproachesAndStopsWhenTargetDies()
        {
            var enemy = Only("Chaser");
            float initial = Vector2.Distance(enemy.transform.position, player.transform.position);
            yield return new WaitForSeconds(0.5f);
            Assert.That(Vector2.Distance(enemy.transform.position, player.transform.position), Is.LessThan(initial - 0.5f));
            player.GetComponent<PlayerHealth>().Receive(10000, Vector2.zero);
            yield return new WaitForSeconds(0.1f);
            Assert.That(enemy.GetComponent<Rigidbody2D>().linearVelocity.magnitude, Is.LessThan(0.01f));
        }
        [UnityTest]
        public IEnumerator RangedRetreatsWhenPlayerIsTooClose()
        {
            var enemy = Only("Ranged");
            enemy.transform.position = new Vector3(1.5f, 0, 0);
            Physics2D.SyncTransforms();
            enemy.GetComponent<EnemyAttack>().Cancel();
            yield return new WaitForSeconds(0.4f);
            Assert.That(enemy.transform.position.x, Is.GreaterThan(2), "Ranged behavior must retreat rather than continuously restart attacks.");
        }
        [UnityTest]
        public IEnumerator RangedTelegraphsAndOneProjectileHitsMultipleHurtboxesOnce()
        {
            var enemy = Only("Ranged");
            var extra = new GameObject("Extra player hurtbox", typeof(BoxCollider2D), typeof(Hurtbox));
            extra.transform.SetParent(player.transform, false);
            extra.GetComponent<BoxCollider2D>().size = Vector2.one;
            extra.GetComponent<Hurtbox>().health = player.GetComponent<PlayerHealth>();
            yield return new WaitForSeconds(0.2f);
            Assert.That(Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None), Is.Empty);
            yield return new WaitForSeconds(1.1f);
            Assert.That(player.GetComponent<PlayerHealth>().Current, Is.EqualTo(92));
        }
        [UnityTest]
        public IEnumerator ChargerTelegraphsLocksDirectionThenRecovers()
        {
            var enemy = Only("Charger");
            var body = enemy.GetComponent<Rigidbody2D>();
            var start = body.position;
            yield return new WaitForSeconds(0.25f);
            Assert.That(Vector2.Distance(start, body.position), Is.LessThan(0.1f));
            player.GetComponent<Rigidbody2D>().position = new Vector2(3, 0);
            yield return new WaitForSeconds(0.55f);
            Assert.That(body.linearVelocity.y, Is.LessThan(-8));
            Assert.That(Mathf.Abs(body.linearVelocity.x), Is.LessThan(0.1f));
            var activeVisual = enemy.GetComponentInChildren<AttackVisual>();
            Assert.That(activeVisual, Is.Not.Null, "Charge range indicator must travel with the damaging body.");
            Assert.That(Vector2.Distance(activeVisual.transform.position, body.position), Is.LessThan(0.1f));
            yield return new WaitForSeconds(0.6f);
            Assert.That(body.linearVelocity.magnitude, Is.LessThan(0.1f));
        }
        [UnityTest]
        public IEnumerator EnemyDeathRemovesItsProjectilesAndEmitsDeathOnce()
        {
            var enemy = Only("Ranged");
            yield return new WaitForSeconds(0.6f);
            Assert.That(Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Length, Is.GreaterThan(0));
            int deaths = 0;
            var health = enemy.GetComponent<EnemyHealth>(); health.Died += () => deaths++;
            health.Receive(1000, Vector2.zero); health.Receive(1000, Vector2.zero);
            yield return new WaitForSeconds(0.2f);
            Assert.That(deaths, Is.EqualTo(1));
            Assert.That(enemy == null, Is.True);
            Assert.That(Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None), Is.Empty);
        }

        [UnityTest]
        public IEnumerator ProjectileStopsAtWallsAndExpiresAwayFromTargets()
        {
            var enemy = Only("Ranged");
            enemy.GetComponent<EnemyAI>().enabled = false;
            var source = enemy.GetComponent<EnemyHealth>();
            var shot = new GameObject("Wall test projectile", typeof(CircleCollider2D), typeof(Rigidbody2D), typeof(Projectile));
            shot.transform.position = new Vector3(6, 0, 0);
            shot.GetComponent<CircleCollider2D>().isTrigger = true;
            shot.GetComponent<CircleCollider2D>().radius = 0.12f;
            shot.GetComponent<Projectile>().Initialize(source, Vector2.right, 8, 8, 2, 3);
            yield return new WaitForSeconds(0.3f);
            Assert.That(shot == null, Is.True, "World collision must consume the projectile before its lifetime.");
            var timeout = new GameObject("Timeout projectile", typeof(CircleCollider2D), typeof(Rigidbody2D), typeof(Projectile));
            timeout.transform.position = new Vector3(20, 20, 0);
            timeout.GetComponent<CircleCollider2D>().isTrigger = true;
            timeout.GetComponent<Projectile>().Initialize(source, Vector2.right, 1, 8, 2, 0.1f);
            yield return new WaitForSeconds(0.2f);
            Assert.That(timeout == null, Is.True);
        }
    }
}
