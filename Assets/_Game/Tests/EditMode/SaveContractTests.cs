using NUnit.Framework;
using Roguelite.Core;
namespace Roguelite.Tests
{
    public class SaveContractTests
    {
        [Test]
        public void ProfileIncludesSettingsAndSaveCodecExists()
        {
            Assert.That(typeof(ProfileData).GetField("settings"), Is.Not.Null, "Persistent settings are required by M11.");
            Assert.That(typeof(GameBootstrap).Assembly.GetType("Roguelite.SaveCodec"), Is.Not.Null);
        }
    }
}
