using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Roguelite.Tests
{
    public class PiercingWaveTests
    {
        [UnityTest]
        public IEnumerator WavePiercesTwoTargetsOnceAndReportsFinalKillReward()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            var player = dungeon.Player;
            for (int i = 0; i < 2; i++)
            {
                var enemy = dungeon.Room.Enemies[i]; enemy.GetComponent<EnemyAI>().enabled = false;
                enemy.transform.position = player.transform.position + Vector3.right * (i + 1) * 1.5f;
                enemy.Receive(enemy.Current - 10, Vector2.zero);
                var extra = new GameObject("Second hurtbox", typeof(BoxCollider2D), typeof(Hurtbox));
                extra.transform.SetParent(enemy.transform, false); extra.GetComponent<BoxCollider2D>().isTrigger = true;
                extra.GetComponent<Hurtbox>().health = enemy;
            }
            Physics2D.SyncTransforms();
            var combat = player.GetComponent<PlayerCombat>(); int hits = 0, kills = 0;
            combat.Hit += (_, amount) => hits++; combat.Killed += _ => kills++;
            var momentum = player.GetComponent<MomentumSystem>(); momentum.Model.Add(40, Time.timeAsDouble);
            var go = new GameObject("Test wave", typeof(CircleCollider2D), typeof(Rigidbody2D), typeof(PiercingWave));
            go.transform.position = player.transform.position; go.GetComponent<CircleCollider2D>().isTrigger = true;
            go.GetComponent<CircleCollider2D>().radius = 0.1f;
            go.GetComponent<PiercingWave>().Initialize(player, Vector2.right, 10, 18, 1, combat.ReportHit);
            player.GetComponent<CombatEffects>().Track(go);
            yield return new WaitForSeconds(0.45f);
            Assert.That(hits, Is.EqualTo(2)); Assert.That(kills, Is.EqualTo(2));
            Assert.That(momentum.Model.Value, Is.EqualTo(78).Within(0.01));
            Assert.That(dungeon.Run.Gold, Is.EqualTo(13));
            dungeon.BeginRun(123); Assert.That(go.activeSelf, Is.False);
            yield return null; Assert.That(go == null, Is.True);
        }
        [UnityTest]
        public IEnumerator WaveStopsAtWallAndExpiresWithoutTargets()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var player = Object.FindFirstObjectByType<DungeonManager>().Player;
            var go = new GameObject("Wall wave", typeof(CircleCollider2D), typeof(Rigidbody2D), typeof(PiercingWave));
            go.transform.position = player.transform.position; go.GetComponent<CircleCollider2D>().isTrigger = true;
            go.GetComponent<PiercingWave>().Initialize(player, Vector2.left, 10, 1, 2);
            yield return new WaitForSeconds(0.3f); Assert.That(go == null, Is.True);
            var timed = new GameObject("Timed wave", typeof(CircleCollider2D), typeof(Rigidbody2D), typeof(PiercingWave));
            timed.transform.position = player.transform.position; timed.GetComponent<CircleCollider2D>().isTrigger = true;
            timed.GetComponent<PiercingWave>().Initialize(player, Vector2.right, 1, 1, 0.05f);
            yield return new WaitForSeconds(0.1f); Assert.That(timed == null, Is.True);
        }
    }
}
