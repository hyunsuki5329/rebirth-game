using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Roguelite.Tests
{
    public class PixelArtTests
    {
        [Test]
        public void ActorArtAndAllHeldWeaponsAreBoundWithoutChangingColliders()
        {
            var player = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Player.prefab");
            Assert.That(player.GetComponent<SpriteRenderer>().sprite.name, Is.EqualTo("Explorer"));
            Assert.That(player.GetComponent<WeaponController>().tintBodyWithWeapon, Is.False);
            Assert.That(player.GetComponent<SpriteRenderer>().sharedMaterial.shader.name, Is.EqualTo("Roguelite/SpriteHitFlash"));
            Assert.That(ShaderUtil.ShaderHasError(player.GetComponent<SpriteRenderer>().sharedMaterial.shader), Is.False);
            Assert.That(player.GetComponent<BoxCollider2D>().size, Is.EqualTo(Vector2.one));
            foreach (var weapon in player.GetComponent<WeaponController>().weapons) Assert.That(weapon.heldSprite, Is.Not.Null, weapon.id);
            string[] prefabs = { "Chaser", "Ranged", "Charger" }, sprites = { "Slime", "Goblin", "Boar" };
            for (int i = 0; i < prefabs.Length; i++)
            {
                var enemy = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Game/Prefabs/Enemies/{prefabs[i]}.prefab");
                Assert.That(enemy.GetComponent<SpriteRenderer>().sprite.name, Is.EqualTo(sprites[i]));
                Assert.That(enemy.GetComponent<CircleCollider2D>().radius, Is.EqualTo(0.4f));
                var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(enemy.GetComponent<SpriteRenderer>().sprite));
                Assert.That(importer.textureShape, Is.EqualTo(TextureImporterShape.Texture2D));
                Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));
            }
        }
    }
}
