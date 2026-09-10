using NUnit.Framework;
using UnityEditor;
namespace Roguelite.Tests
{
    public class RunLoopAssetTests
    {
        [Test]
        public void MainMenuHubAndDungeonScenesExist()
        {
            foreach (string name in new[] { "MainMenu", "Hub", "Dungeon" })
                Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>($"Assets/_Game/Scenes/{name}.unity"), Is.Not.Null, name);
        }
    }
}
