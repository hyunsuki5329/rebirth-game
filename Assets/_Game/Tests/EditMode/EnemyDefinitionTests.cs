using NUnit.Framework;
using Roguelite.Core;
using UnityEditor;
using UnityEngine;
namespace Roguelite.Tests
{
    public class EnemyDefinitionTests
    {
        [TestCase(1, -1)]
        [TestCase(3.5f, 0)]
        [TestCase(4, 0)]
        [TestCase(4.5f, 0)]
        [TestCase(6, 1)]
        public void RangedEnemyRetreatsHoldsAndApproaches(float distance, int expected)
            => Assert.That(EnemySteering.RangedAxis(distance, 4, 0.5f), Is.EqualTo(expected));

        [TestCase("Chaser")]
        [TestCase("Ranged")]
        [TestCase("Charger")]
        public void EachEnemyPrefabHasItsOwnBehaviorAndCombatComponents(string archetype)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Enemies/" + archetype + ".prefab");
            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent(archetype + "AI"), Is.Not.Null);
            Assert.That(prefab.GetComponent("EnemyAttack"), Is.Not.Null);
            Assert.That(prefab.GetComponent("EnemyHealth"), Is.Not.Null);
        }
    }
}
