using NUnit.Framework;
using Roguelite.Core;
using UnityEngine;
using UnityEditor;
namespace Roguelite.Tests
{
    public class RoomDefinitionTests
    {
        [Test]
        public void EmptyCombatAndMissingSpawnAreInvalidButEmptyRewardIsValid()
        {
            var data = ScriptableObject.CreateInstance<RoomDefinition>();
            try
            {
                data.id = "test"; data.kind = RoomKind.Combat;
                Assert.That(data.IsValid, Is.False);
                data.kind = RoomKind.Reward;
                Assert.That(data.IsValid, Is.True);
                data.enemies = new[] { new EnemySpawn() };
                Assert.That(data.IsValid, Is.False);
                data.enemies = new[] { new EnemySpawn {
                    prefab = AssetDatabase.LoadAssetAtPath<EnemyAI>("Assets/_Game/Prefabs/Enemies/Chaser.prefab"),
                    position = new Vector2(float.NaN, 0) } };
                Assert.That(data.IsValid, Is.False);
            }
            finally { Object.DestroyImmediate(data); }
        }
    }
}
