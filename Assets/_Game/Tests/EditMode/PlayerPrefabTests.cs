using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Roguelite.Tests
{
    public class PlayerPrefabTests
    {
        [Test]
        public void PlayerPrefabHasSeparatedInputAndSinglePhysicsMotor()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Player.prefab");
            Assert.That(prefab, Is.Not.Null, "M1 must provide a playable player prefab.");
            foreach (string component in new[] { "PlayerInputReader", "PlayerMovement", "PlayerSprint", "PlayerDash", "PlayerMotor2D" })
                Assert.That(prefab.GetComponent(component), Is.Not.Null, component);
            Assert.That(prefab.GetComponents<Rigidbody2D>().Length, Is.EqualTo(1));
        }
    }
}
