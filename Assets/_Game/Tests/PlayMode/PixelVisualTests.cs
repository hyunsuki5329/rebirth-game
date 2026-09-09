using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Roguelite.Tests
{
    public class PixelVisualTests
    {
        [UnityTest]
        public IEnumerator EveryWeaponRendersAndColoredCharacterStillFlashesOnHit()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var player = Object.FindFirstObjectByType<DungeonManager>().Player;
            var weapons = player.GetComponent<WeaponController>();
            var held = player.transform.Find("Held weapon").GetComponent<SpriteRenderer>();
            for (int i = 0; i < weapons.weapons.Length; i++)
            {
                Assert.That(weapons.Select(i), Is.True); yield return null;
                Assert.That(held.enabled, Is.True); Assert.That(held.sprite, Is.EqualTo(weapons.Current.heldSprite));
                Assert.That(Mathf.Max(held.bounds.size.x, held.bounds.size.y), Is.InRange(0.2f, 3f));
            }
            player.Receive(1, Vector2.zero); yield return null;
            var block = new MaterialPropertyBlock(); var sprite = player.GetComponent<SpriteRenderer>();
            sprite.GetPropertyBlock(block); Assert.That(block.GetFloat("_HitFlash"), Is.EqualTo(1));
            yield return new WaitForSeconds(0.15f);
            sprite.GetPropertyBlock(block); Assert.That(block.GetFloat("_HitFlash"), Is.Zero);
            Assert.That(sprite.color, Is.EqualTo(Color.white));
        }
    }
}
