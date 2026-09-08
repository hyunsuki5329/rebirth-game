using System.Collections.Generic;
using NUnit.Framework;
using Roguelite.Core;
namespace Roguelite.Tests
{
    public class DungeonTests
    {
        [Test]
        public void SameSeedProducesSameRoomChoices()
        {
            var a = new RoomGraph(123); var b = new RoomGraph(123);
            Assert.That(a.Nodes.Length, Is.EqualTo(8));
            for (int i = 0; i < a.Nodes.Length; i++) Assert.That(a.Nodes[i].Variant, Is.EqualTo(b.Nodes[i].Variant));
        }
        [TestCase(false, 4)]
        [TestCase(true, 3)]
        public void BothRoutesReachBossWithoutCycles(bool shortcut, int combatRooms)
        {
            var graph = new RoomGraph(7); var visited = new HashSet<int>();
            int node = 0, fights = 0, last = -1;
            while (node >= 0)
            {
                Assert.That(visited.Add(node), Is.True, "Room route cannot loop.");
                Assert.That(node, Is.LessThan(graph.Nodes.Length));
                if (graph.Nodes[node].Kind == RoomKind.Combat || graph.Nodes[node].Kind == RoomKind.Elite) fights++;
                last = node; node = graph.Next(node, shortcut);
            }
            Assert.That(graph.Nodes[last].Kind, Is.EqualTo(RoomKind.Boss));
            Assert.That(fights, Is.EqualTo(combatRooms));
        }
        [Test]
        public void RoomRewardsAreOneTimeAndNewRunResetsGold()
        {
            var run = new RunState(1);
            Assert.That(run.Award("room0", RewardCalculator.Gold(10, 1.25f)), Is.True);
            Assert.That(run.Award("room0", 1000), Is.False);
            Assert.That(run.Gold, Is.EqualTo(12));
            Assert.That(new RunState(1).Gold, Is.Zero);
            Assert.That(RewardCalculator.Gold(-10, 2), Is.Zero);
        }
    }
}
