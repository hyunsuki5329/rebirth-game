using NUnit.Framework;
using Roguelite.Core;
namespace Roguelite.Tests
{
    public class DamageTests
    {
        [TestCase(10, 1.5f, 2, 13)]
        [TestCase(10, 1, 50, 0)]
        [TestCase(-10, 1, 0, 0)]
        [TestCase(10, -1, 0, 0)]
        [TestCase(10, 1, -5, 10)]
        public void DamageIsNonnegativeAndAppliesMultiplierBeforeArmor(float damage, float multiplier, float armor, float expected)
            => Assert.That(DamageCalculator.Resolve(damage, multiplier, armor), Is.EqualTo(expected));

        [Test]
        public void OverkillClampsHealthAndDeathOccursOnlyOnce()
        {
            var health = new HealthState(30);
            int deaths = 0;
            health.Died += () => deaths++;
            Assert.That(health.Receive(12), Is.EqualTo(12));
            Assert.That(health.Current, Is.EqualTo(18));
            Assert.That(health.Receive(-5), Is.Zero);
            Assert.That(health.Receive(100), Is.EqualTo(18));
            Assert.That(health.Current, Is.Zero);
            Assert.That(health.Receive(10), Is.Zero);
            Assert.That(deaths, Is.EqualTo(1));
        }

        [Test]
        public void PlayerPrefabContainsCombatAndHealthContracts()
        {
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/_Game/Prefabs/Player.prefab");
            Assert.That(prefab.GetComponent("PlayerCombat"), Is.Not.Null);
            Assert.That(prefab.GetComponent("PlayerHealth"), Is.Not.Null);
            Assert.That(prefab.GetComponent("Hurtbox"), Is.Not.Null);
        }
    }
}
