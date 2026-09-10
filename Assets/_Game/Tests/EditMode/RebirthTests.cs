using NUnit.Framework;
using Roguelite.Core;
namespace Roguelite.Tests
{
    public class RebirthTests
    {
        [Test]
        public void EligibilityExactCostPointsAndPersistentUpgradesAreAtomic()
        {
            var rules = new RebirthSystem(100, new[] { 1f, 2f, 2.7f });
            var profile = new ProfileData { bankGold = 100 };
            Assert.That(rules.TryRebirth(profile), Is.False); Assert.That(profile.bankGold, Is.EqualTo(100));
            profile.totalClears = 1; profile.bankGold = 99;
            Assert.That(rules.TryRebirth(profile), Is.False); Assert.That(profile.rebirthCount, Is.Zero);
            profile.bankGold = 100; var unlocks = (string[])profile.unlockIds.Clone();
            Assert.That(rules.TryRebirth(profile), Is.True); Assert.That(profile.bankGold, Is.Zero);
            Assert.That(profile.rebirthCount, Is.EqualTo(1)); Assert.That(profile.rebirthPoints, Is.EqualTo(1));
            Assert.That(profile.permanentGoldMultiplier, Is.EqualTo(2)); Assert.That(profile.unlockIds, Is.EqualTo(unlocks));
            Assert.That(rules.TryRebirth(profile), Is.False);
            Assert.That(rules.TryBuy(profile, "max-health", 1), Is.True);
            Assert.That(rules.TryBuy(profile, "max-health", 1), Is.False); Assert.That(rules.TryBuy(profile, "combo-window", 1), Is.False);
            profile.bankGold = 200; Assert.That(rules.TryRebirth(profile), Is.True);
            Assert.That(profile.upgradeIds, Does.Contain("max-health")); Assert.That(rules.TryBuy(profile, "combo-window", 1), Is.True);
            Assert.That(profile.upgradeIds.Length, Is.EqualTo(2)); Assert.That(profile.rebirthPoints, Is.Zero);
        }
        [Test]
        public void PermanentGrowthAndThreatRewardAreIndependentlyMeasurable()
        {
            var rules = new RebirthSystem(100, new[] { 1f, 2f, 2.7f });
            Assert.That(RewardCalculator.Gold(100, rules.Multiplier(0)), Is.EqualTo(100));
            Assert.That(RewardCalculator.Gold(100, rules.Multiplier(1)), Is.EqualTo(200));
            Assert.That(System.Math.Ceiling(200d / 100), Is.GreaterThan(System.Math.Ceiling(200d / 200)));
            Assert.That(WorldThreatSystem.ProjectileCount(0), Is.EqualTo(1)); Assert.That(WorldThreatSystem.ProjectileCount(1), Is.EqualTo(3));
            Assert.That(WorldThreatSystem.ProjectileCount(9), Is.EqualTo(3)); Assert.That(WorldThreatSystem.Level(2), Is.EqualTo(2));
            Assert.That(RewardCalculator.Gold(100, WorldThreatSystem.GoldMultiplier(1, 1.2f)), Is.EqualTo(120));
            Assert.That(RewardCalculator.Gold(100, rules.Multiplier(1) * WorldThreatSystem.GoldMultiplier(1, 1.2f)), Is.EqualTo(240));
        }
    }
}
