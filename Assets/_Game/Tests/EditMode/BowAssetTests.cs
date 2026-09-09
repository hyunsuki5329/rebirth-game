using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Roguelite.Tests
{
    public class BowAssetTests
    {
        [Test]
        public void BowHasProjectileDataAndPointFilteredTransparentArt()
        {
            var bow = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/_Game/Data/Weapons/Bow.asset");
            Assert.That(bow, Is.Not.Null); Assert.That(bow.heldSprite, Is.Not.Null);
            Assert.That(bow.attack.kind, Is.EqualTo(PlayerAttackKind.Projectile));
            Assert.That(bow.attack.projectileSprite, Is.Not.Null);
            Assert.That(bow.attack.projectileSpeed * bow.attack.projectileLifetime, Is.InRange(7f, 10f));
            Assert.That(bow.attack.windup + bow.attack.activeTime + bow.attack.recovery, Is.EqualTo(0.5f).Within(0.001));
            var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(bow.heldSprite));
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(importer.textureShape, Is.EqualTo(TextureImporterShape.Texture2D));
            Assert.That(importer.alphaIsTransparency, Is.True);
            Assert.That(importer.mipmapEnabled, Is.False);
        }
    }
}
