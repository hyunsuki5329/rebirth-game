using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Roguelite.Tests
{
    public class VerticalSliceReferenceTests
    {
        [Test]
        public void GamePrefabsAndDataHaveNoMissingScriptsOrBrokenSerializedReferences()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Game" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid); var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                foreach (var transform in prefab.GetComponentsInChildren<Transform>(true))
                    Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject), Is.Zero, path + "/" + transform.name);
                foreach (var component in prefab.GetComponentsInChildren<Component>(true)) Inspect(component, path);
            }
            foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/_Game/Data" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid); var data = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                Inspect(data, path);
                if (data is RoomDefinition room) Assert.That(room.IsValid, Is.True, path);
                if (data is WeaponData weapon)
                {
                    Assert.That(weapon.attack, Is.Not.Null, path); Assert.That(weapon.heldSprite, Is.Not.Null, path);
                    Assert.That(weapon.combos, Is.Not.Empty, path);
                }
            }
        }
        private static void Inspect(Object target, string path)
        {
            Assert.That(target, Is.Not.Null, path);
            var serialized = new SerializedObject(target); var property = serialized.GetIterator();
            while (property.Next(true))
                if (property.propertyType == SerializedPropertyType.ObjectReference)
                    Assert.That(property.objectReferenceValue == null && property.objectReferenceInstanceIDValue != 0, Is.False, path + ": " + property.propertyPath);
        }
    }
}
