using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Roguelite.Tests
{
    public class RouteComparisonTests
    {
        [UnityTest]
        public IEnumerator SameBowBuildRunsEachRouteThreeTimesAndRecordsRiskReward()
        {
            float oldScale = Time.timeScale;
            try
            {
                Time.timeScale = 3;
                for (int route = 0; route < 2; route++)
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
                    var dungeon = Object.FindFirstObjectByType<DungeonManager>();
                    // Identical pre-branch state; setup kills are excluded from route metrics.
                    while (dungeon.Run.RoomIndex != 3)
                    {
                        foreach (var enemy in dungeon.Room.Enemies) enemy.Receive(10000, Vector2.zero);
                        yield return null;
                        if (dungeon.PendingRewards.Length > 0) dungeon.ChooseReward(0);
                        dungeon.Advance(); yield return null;
                    }
                    dungeon.Player.GetComponent<WeaponController>().Select(3);
                    float start = Time.time, hp = dungeon.Player.Current; int gold = dungeon.Run.Gold, rooms = 0;
                    Assert.That(dungeon.Advance(route == 1), Is.True); yield return null;
                    while (dungeon.Run.RoomIndex != 6 && !dungeon.Player.IsDead && Time.time - start < 60)
                    {
                        if (dungeon.Room.IsClear)
                        {
                            yield return null;
                            if (dungeon.PendingRewards.Length > 0) { Assert.That(dungeon.ChooseReward(0), Is.True); Assert.That(dungeon.ChooseReward(0), Is.False); }
                            Assert.That(dungeon.Rewards.GrantRoom(dungeon.Run, dungeon.Run.RoomIndex, 999), Is.False);
                            Assert.That(dungeon.Advance(route != 1), Is.True); rooms++; yield return null;
                        }
                        else
                        {
                            var target = dungeon.Room.Enemies.Where(e => e != null && !e.IsDead)
                                .OrderBy(e => (e.transform.position - dungeon.Player.transform.position).sqrMagnitude).FirstOrDefault();
                            if (target != null) dungeon.Player.GetComponent<PlayerCombat>().RequestAttack(target.transform.position - dungeon.Player.transform.position);
                            yield return null;
                        }
                    }
                    TestContext.WriteLine($"ROUTE_METRIC route={(route == 1 ? "danger" : "safe")} attempt={attempt + 1} seconds={Time.time - start:F2} startHP={hp:F0} remainingHP={dungeon.Player.Current:F0} death={dungeon.Player.IsDead} gold={dungeon.Run.Gold - gold} rooms={rooms} bossEntrance={dungeon.Run.RoomIndex == 6}");
                    Assert.That(dungeon.Run.RoomIndex, Is.EqualTo(6)); Assert.That(rooms, Is.EqualTo(route == 1 ? 1 : 2));
                    Assert.That(dungeon.ChosenShortcut, Is.EqualTo(route == 1));
                }
            }
            finally { Time.timeScale = oldScale; }
        }
    }
}
