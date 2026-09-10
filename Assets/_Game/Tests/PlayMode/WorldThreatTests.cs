using System.Collections;
using System.Linq;
using NUnit.Framework;
using Roguelite.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Roguelite.Tests
{
    public class WorldThreatTests
    {
        [UnityTest]
        public IEnumerator SameSeedRangedEnemyChangesFromOneToThreeTelegraphedShots()
        {
            for (int threat = 0; threat <= 1; threat++)
            {
                yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
                var dungeon = Object.FindFirstObjectByType<DungeonManager>();
                dungeon.combatRooms = new[] { dungeon.combatRooms[0] }; dungeon.WorldThreat = threat; dungeon.BeginRun(123); yield return null;
                var ranged = dungeon.Room.Enemies.Single(e => e.data.attackKind == EnemyAttackKind.Projectile);
                foreach (var enemy in dungeon.Room.Enemies) enemy.GetComponent<EnemyAI>().enabled = false;
                var attack = ranged.GetComponent<EnemyAttack>(); float windup = attack.data.attack.windup;
                Assert.That(attack.TryBegin(Vector2.left), Is.True);
                yield return new WaitForSeconds(windup * 0.5f); Assert.That(Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None), Is.Empty);
                yield return new WaitForSeconds(windup * 0.5f + 0.08f);
                var shots = Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None);
                Assert.That(shots.Length, Is.EqualTo(threat == 0 ? 1 : 3));
                var angles = shots.Select(p => Vector2.SignedAngle(Vector2.left, p.GetComponent<Rigidbody2D>().linearVelocity)).OrderBy(a => a).ToArray();
                if (threat == 1) { Assert.That(angles[0], Is.EqualTo(-20).Within(0.1)); Assert.That(angles[1], Is.EqualTo(0).Within(0.1)); Assert.That(angles[2], Is.EqualTo(20).Within(0.1)); }
                Assert.That(ranged.Maximum, Is.EqualTo(ranged.data.health));
            }
        }
    }
}
