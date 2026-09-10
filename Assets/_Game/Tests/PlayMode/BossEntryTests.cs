using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Roguelite.Tests
{
    public class BossEntryTests
    {
        [UnityTest]
        public IEnumerator BossEntranceContainsLockedLivingBoss()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            while (dungeon.Run.RoomIndex != 6)
            {
                foreach (var enemy in dungeon.Room.Enemies) enemy.Receive(10000, Vector2.zero);
                yield return null;
                if (dungeon.PendingRewards.Length > 0) dungeon.ChooseReward(0);
                Assert.That(dungeon.Advance(true), Is.True); yield return null;
            }
            Assert.That(dungeon.Room.Enemies.Count, Is.EqualTo(1));
            Assert.That(dungeon.Room.exit.IsOpen, Is.False);
            Assert.That(dungeon.Room.Enemies[0].Maximum, Is.GreaterThan(100));
        }
    }
}
