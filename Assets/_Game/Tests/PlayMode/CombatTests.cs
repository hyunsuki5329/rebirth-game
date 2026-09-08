using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Roguelite.Tests
{
    public class CombatTests
    {
        [UnitySetUp]
        public IEnumerator SetUp() { yield return SceneManager.LoadSceneAsync("CombatArena"); }

        [UnityTest]
        public IEnumerator AttackHitsMultipleHurtboxesOnlyOnceAndCannotHitSelf()
        {
            var player = GameObject.Find("Player");
            var combat = player.GetComponent<PlayerCombat>();
            var target = GameObject.Find("TrainingDummy").GetComponent<HealthBody>();
            var extra = new GameObject("Extra hurtbox", typeof(BoxCollider2D), typeof(Hurtbox));
            extra.transform.SetParent(target.transform, false);
            extra.GetComponent<Hurtbox>().health = target;
            extra.GetComponent<BoxCollider2D>().size = Vector2.one * 0.5f;
            int attacks = 0, hits = 0;
            combat.AttackStarted += () => attacks++;
            combat.Hit += (_, __) => hits++;
            Assert.That(combat.RequestAttack(Vector2.right), Is.True);
            Assert.That(combat.RequestAttack(Vector2.right), Is.False);
            yield return new WaitForSeconds(0.14f);
            Assert.That(target.Current, Is.EqualTo(28));
            Assert.That(hits, Is.EqualTo(1));
            Assert.That(attacks, Is.EqualTo(1));
            Assert.That(player.GetComponent<PlayerHealth>().Current, Is.EqualTo(100));
            Assert.That(target.GetComponent<SpriteRenderer>().color, Is.EqualTo(Color.white));
            yield return new WaitForSeconds(0.4f);
            Assert.That(combat.RequestAttack(Vector2.right), Is.True);
        }

        [UnityTest]
        public IEnumerator AttackRejectsTargetBehindAimAndCleansUpHitbox()
        {
            var target = GameObject.Find("TrainingDummy").GetComponent<HealthBody>();
            target.transform.position = new Vector3(-1.2f, 0, 0);
            GameObject.Find("Player").GetComponent<PlayerCombat>().RequestAttack(Vector2.right);
            yield return new WaitForSeconds(0.3f);
            Assert.That(target.Current, Is.EqualTo(40));
            Assert.That(Object.FindObjectsByType<Hitbox>(FindObjectsSortMode.None), Is.Empty);
        }

        [UnityTest]
        public IEnumerator RealAttackKnocksTargetBack()
        {
            var target = GameObject.Find("TrainingDummy");
            GameObject.Find("Player").GetComponent<PlayerCombat>().RequestAttack(Vector2.right);
            yield return new WaitForSeconds(0.3f);
            Assert.That(target.transform.position.x, Is.GreaterThan(1.4f));
            Assert.That(target.GetComponent<HealthBody>().Current, Is.EqualTo(28));
        }

        [UnityTest]
        public IEnumerator DisablingCombatCancelsWindupAndRestoresMovement()
        {
            var player = GameObject.Find("Player");
            var combat = player.GetComponent<PlayerCombat>();
            combat.RequestAttack(Vector2.right);
            yield return null;
            combat.enabled = false;
            Assert.That(player.GetComponent<PlayerMovement>().ActionMultiplier, Is.EqualTo(1));
            yield return new WaitForSeconds(0.2f);
            combat.enabled = true;
            yield return new WaitForSeconds(0.2f);
            Assert.That(GameObject.Find("TrainingDummy").GetComponent<HealthBody>().Current, Is.EqualTo(40));
        }

        [UnityTest]
        public IEnumerator PlayerKnockbackStopsAtWallAndDeathStopsAttacks()
        {
            var player = GameObject.Find("Player");
            var body = player.GetComponent<Rigidbody2D>();
            var health = player.GetComponent<PlayerHealth>();
            body.position = new Vector2(6.4f, 0);
            health.Receive(1, Vector2.right * 10);
            yield return new WaitForSeconds(0.3f);
            Assert.That(body.position.x, Is.GreaterThan(6.4f));
            Assert.That(body.position.x, Is.LessThan(7));
            int deaths = 0;
            health.Died += () => deaths++;
            health.Receive(1000, Vector2.zero);
            health.Receive(1000, Vector2.zero);
            yield return new WaitForFixedUpdate();
            Assert.That(deaths, Is.EqualTo(1));
            Assert.That(body.linearVelocity, Is.EqualTo(Vector2.zero));
            Assert.That(player.GetComponent<PlayerCombat>().RequestAttack(Vector2.right), Is.False);
        }

        [UnityTest]
        public IEnumerator AttackSpeedScalesHitboxLifetimeAlongWithWindup()
        {
            var combat = GameObject.Find("Player").GetComponent<PlayerCombat>();
            combat.SpeedMultiplier = 2;
            combat.RequestAttack(Vector2.right);
            yield return new WaitForSeconds(0.12f);
            Assert.That(Object.FindObjectsByType<Hitbox>(FindObjectsSortMode.None), Is.Empty);
        }
    }
}
