using NUnit.Framework;
using Roguelite.Core;
namespace Roguelite.Tests
{
    public class RoomClearTests
    {
        [Test]
        public void DoorWaitsForEveryRegisteredEnemyAndClearFiresOnce()
        {
            var room = new RoomClearState(new[] { 10, 20 });
            int clears = 0;
            room.Cleared += () => clears++;
            Assert.That(room.IsClear, Is.False);
            Assert.That(room.Defeat(99), Is.False);
            Assert.That(room.Defeat(10), Is.True);
            Assert.That(room.IsClear, Is.False);
            Assert.That(room.Defeat(10), Is.False);
            Assert.That(room.Defeat(20), Is.True);
            Assert.That(room.IsClear, Is.True);
            Assert.That(room.Defeat(20), Is.False);
            Assert.That(clears, Is.EqualTo(1));
        }
        [Test]
        public void EmptyRewardRoomIsAlreadyClear()
        {
            Assert.That(new RoomClearState(new int[0]).IsClear, Is.True);
        }
    }
}
