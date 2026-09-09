using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Roguelite.Tests
{
    public class WeaponGripTests
    {
        [UnityTest]
        public IEnumerator BowFacesForwardAndGripFollowsHand()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var player = Object.FindFirstObjectByType<DungeonManager>().Player;
            var weapons = player.GetComponent<WeaponController>();
            Assert.That(weapons.Select(3), Is.True); yield return null;
            var held = player.transform.Find("Held weapon").GetComponent<SpriteRenderer>();
            Assert.That(held.flipX, Is.True, "Bow artwork faces left and must be mirrored to aim forward.");
            var hand = player.transform.Find("Weapon hand"); Assert.That(hand, Is.Not.Null);
            Vector2 grip = (new Vector2(0.435f, 0.5f) * held.sprite.rect.size - held.sprite.pivot) / held.sprite.pixelsPerUnit;
            grip.x = -grip.x;
            Assert.That(Vector3.Distance(held.transform.TransformPoint(grip), hand.position), Is.LessThan(0.001f));
            Assert.That(hand.GetComponent<SpriteRenderer>().sortingOrder, Is.GreaterThan(held.sortingOrder));
        }
        [UnityTest]
        public IEnumerator MeleePoseUsesAcceptedAttackDirectionThenReturnsToIdle()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var player = Object.FindFirstObjectByType<DungeonManager>().Player;
            var held = player.transform.Find("Held weapon"); var idle = held.rotation;
            var combat = player.GetComponent<PlayerCombat>();
            Assert.That(combat.RequestAttack(Vector2.left), Is.True); yield return null;
            Assert.That(Vector2.Dot(held.right, Vector2.left), Is.GreaterThan(0.2f));
            yield return new WaitForSeconds(0.6f);
            var aim = player.GetComponent<PlayerAim>().Direction;
            Assert.That(Vector2.Dot(held.right, aim), Is.GreaterThan(0.999f));
            var hand = player.transform.Find("Weapon hand"); Assert.That(hand, Is.Not.Null);
            player.Receive(10000, Vector2.zero); yield return null;
            Assert.That(hand.GetComponent<SpriteRenderer>().enabled, Is.False);
            Assert.That(held.GetComponent<SpriteRenderer>().enabled, Is.False);
        }
    }
}
