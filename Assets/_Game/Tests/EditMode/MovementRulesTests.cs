using System;
using NUnit.Framework;
using Roguelite.Core;

namespace Roguelite.Tests
{
    public class MovementRulesTests
    {
        [TestCase(1, 0, false, 5)]
        [TestCase(1, 1, false, 5)]
        [TestCase(1, 1, true, 7.5f)]
        [TestCase(0, 0, true, 0)]
        [TestCase(0.5f, 0, false, 2.5f)]
        public void MovementClampsDiagonalButPreservesAnalogMagnitude(float x, float y, bool sprint, float expected)
        {
            var v = MovementRules.Velocity(x, y, 5, 1.5f, sprint);
            Assert.That(Math.Sqrt(v.X * v.X + v.Y * v.Y), Is.EqualTo(expected).Within(0.0001));
        }

        [Test]
        public void SprintReleaseImmediatelyRestoresWalkingSpeed()
        {
            Assert.That(MovementRules.Velocity(1, 0, 4, 1.5f, true).X, Is.EqualTo(6));
            Assert.That(MovementRules.Velocity(1, 0, 4, 1.5f, false).X, Is.EqualTo(4));
        }

        [Test]
        public void DashCooldownStartsAtEndAndRejectsRepeatedStarts()
        {
            var dash = new DashModel(3, 0.25f, 1);
            Assert.That(dash.TryStart(10), Is.True);
            Assert.That(dash.Speed, Is.EqualTo(12));
            Assert.That(dash.IsDashing(10.24), Is.True);
            Assert.That(dash.IsDashing(10.25), Is.False);
            Assert.That(dash.TryStart(10.1), Is.False);
            Assert.That(dash.CooldownRemaining(10.25), Is.EqualTo(1));
            Assert.That(dash.TryStart(11.24), Is.False);
            Assert.That(dash.TryStart(11.25), Is.True);
        }

        [Test]
        public void InvalidDashDurationCannotCreateInfiniteSpeed()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DashModel(3, 0, 1));
        }
    }
}
