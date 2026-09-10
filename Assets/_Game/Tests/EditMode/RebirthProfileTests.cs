using NUnit.Framework;
using Roguelite.Core;
namespace Roguelite.Tests
{
    public class RebirthProfileTests
    {
        [Test]
        public void ProfileOwnsPersistentRebirthAndUpgradeState()
        {
            foreach (string field in new[] { "rebirthCount", "rebirthPoints", "permanentGoldMultiplier", "upgradeIds", "unlockIds" })
                Assert.That(typeof(ProfileData).GetField(field), Is.Not.Null, field);
        }
    }
}
