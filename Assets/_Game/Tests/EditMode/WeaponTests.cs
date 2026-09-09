using NUnit.Framework;
using Roguelite.Core;
namespace Roguelite.Tests
{
    public class WeaponTests
    {
        [Test]
        public void EvolutionStartsOnThirdQualifyingComboAndRemainsEvolved()
        {
            var state = new WeaponProgress(3);
            Assert.That(state.RecordCombo(false), Is.False);
            Assert.That(state.ComboCount, Is.Zero);
            Assert.That(state.RecordCombo(true), Is.False);
            Assert.That(state.RecordCombo(true), Is.False);
            Assert.That(state.Evolved, Is.False);
            Assert.That(state.RecordCombo(true), Is.True);
            Assert.That(state.Evolved, Is.True);
            Assert.That(state.RecordCombo(true), Is.True);
            Assert.That(state.ComboCount, Is.EqualTo(3));
            Assert.That(new WeaponProgress(3).Evolved, Is.False);
        }
        [Test]
        public void ItemStatsStackAdditivelyAndChoicesAreDistinctAndSeeded()
        {
            Assert.That(WeaponStats.Modified(10, 0.15f, 2), Is.EqualTo(13).Within(0.001));
            Assert.That(WeaponStats.Modified(10, 0.15f, 0), Is.EqualTo(10));
            var choices = WeaponStats.RewardChoices(123, 2, 2, 3);
            Assert.That(choices.Length, Is.EqualTo(2));
            Assert.That(choices[0], Is.Not.EqualTo(choices[1]));
            Assert.That(choices, Is.EqualTo(WeaponStats.RewardChoices(123, 2, 2, 3)));
        }
    }
}
