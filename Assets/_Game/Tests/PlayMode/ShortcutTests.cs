using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Roguelite.Tests
{
    public class ShortcutTests
    {
        [UnityTest]
        public IEnumerator RareRewardGrantsTwoStacksAndConvertsCappedStacksOnlyOnce()
        {
            for (int capped = 0; capped < 2; capped++)
            {
                yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
                var dungeon = Object.FindFirstObjectByType<DungeonManager>();
                var config = Object.Instantiate(dungeon.shortcutRules); config.eliteRareChance = 1;
                dungeon.shortcutRules = config;
                while (dungeon.Run.RoomIndex != 7)
                {
                    foreach (var enemy in dungeon.Room.Enemies) enemy.Receive(10000, Vector2.zero);
                    yield return null;
                    if (dungeon.PendingRewards.Length > 0) dungeon.ChooseReward(0);
                    Assert.That(dungeon.Advance(true), Is.True); yield return null;
                }
                foreach (var enemy in dungeon.Room.Enemies) enemy.Receive(10000, Vector2.zero);
                yield return null;
                Assert.That(dungeon.PendingRewardStacks, Is.EqualTo(2)); Assert.That(dungeon.Advance(), Is.False);
                var item = dungeon.PendingRewards[0]; var inventory = dungeon.Player.GetComponent<RunInventory>();
                if (capped == 1) while (inventory.Count(item.id) < item.maximumStacks) inventory.Add(item);
                int before = inventory.Count(item.id), gold = dungeon.Run.Gold;
                Assert.That(dungeon.ChooseReward(0), Is.True);
                Assert.That(inventory.Count(item.id), Is.EqualTo(Mathf.Min(before + 2, item.maximumStacks)));
                int overflow = Mathf.Max(0, before + 2 - item.maximumStacks);
                Assert.That(dungeon.Run.Gold, Is.EqualTo(gold + overflow * dungeon.cappedItemGold));
                Assert.That(dungeon.ChooseReward(0), Is.False);
                Assert.That(dungeon.Run.Gold, Is.EqualTo(gold + overflow * dungeon.cappedItemGold));
                Assert.That(dungeon.Advance(), Is.True); Assert.That(dungeon.Run.RoomIndex, Is.EqualTo(6));
                Object.Destroy(config);
            }
        }
        [UnityTest]
        public IEnumerator ChoiceRoomHasTwoDistinctPhysicalExitsAndDangerLeadsToElite()
        {
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            while (dungeon.Run.RoomIndex != 3)
            {
                foreach (var enemy in dungeon.Room.Enemies) enemy.Receive(10000, Vector2.zero);
                yield return null;
                if (dungeon.PendingRewards.Length > 0) dungeon.ChooseReward(0);
                Assert.That(dungeon.Advance(), Is.True); yield return null;
            }
            var exits = dungeon.Room.GetComponentsInChildren<RoomExit>();
            Assert.That(exits.Length, Is.EqualTo(2));
            var danger = System.Array.Find(exits, e => e.name == "Dangerous exit");
            Assert.That(danger, Is.Not.Null);
            Assert.That(danger.GetComponent<BoxCollider2D>().size, Is.EqualTo(Vector2.one));
            Assert.That(danger.GetComponent<SpriteRenderer>().color, Is.Not.EqualTo(dungeon.Room.exit.GetComponent<SpriteRenderer>().color));
            dungeon.Player.GetComponent<PlayerMotor2D>().Teleport(danger.transform.position);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.That(dungeon.Run.RoomIndex, Is.EqualTo(7));
            Assert.That(dungeon.Advance(), Is.False);
        }
    }
}
