using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Roguelite.Tests
{
    public class FoundationTests
    {
        [Test]
        public void PlayerTestSceneSupportsVisibleZeroGravity2DPhysics()
        {
            const string path = "Assets/_Game/Scenes/PlayerTest.unity";
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(path), Is.Not.Null,
                "The foundation scene must exist before player implementation.");
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                Camera camera = null;
                Rigidbody2D body = null;
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root.TryGetComponent<Camera>(out var c)) camera = c;
                    if (root.TryGetComponent<Rigidbody2D>(out var b)) body = b;
                }
                Assert.That(camera, Is.Not.Null);
                Assert.That(camera.orthographic, Is.True);
                Assert.That(body, Is.Not.Null);
                Assert.That(body.gravityScale, Is.Zero);
                Assert.That(body.transform.position.z, Is.Zero);
                Assert.That(body.GetComponent<Collider2D>(), Is.Not.Null);
                Assert.That(body.GetComponent<SpriteRenderer>().sprite, Is.Not.Null);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                SceneManager.SetActiveScene(previous);
            }
        }
    }
}
