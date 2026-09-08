using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Roguelite.Tests
{
    public class AssetReferenceTests
    {
        private static void Check(GameObject root)
        {
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject), Is.Zero,
                    "Missing Script: " + transform.name);
        }
        [Test]
        public void AllGameScenesAndPrefabsHaveResolvableScripts()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Game" }))
                Check(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)));
            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/_Game" }))
            {
                var scene = EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guid), OpenSceneMode.Additive);
                try { foreach (var root in scene.GetRootGameObjects()) Check(root); }
                finally { EditorSceneManager.CloseScene(scene, true); }
            }
        }
    }
}
