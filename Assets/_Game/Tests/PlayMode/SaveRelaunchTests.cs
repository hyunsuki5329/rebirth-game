using System.Collections;
using System.IO;
using NUnit.Framework;
using Roguelite.Core;
using UnityEngine;
using UnityEngine.TestTools;
namespace Roguelite.Tests
{
    public class SaveRelaunchTests
    {
        // Explicit two-process checks: run Write, let Unity exit, then run Read.
        // Kept out of the ordinary suite to avoid ordering assumptions.
        private string DirectoryPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../TestResults/RelaunchProfile"));
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var session in Object.FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None)) Object.Destroy(session.gameObject);
            yield return null; AudioListener.volume = 1;
        }
        [UnityTest, Explicit("First half of a two-process save check")]
        public IEnumerator WriteProfileForNextUnityProcess()
        {
            if (Directory.Exists(DirectoryPath)) Directory.Delete(DirectoryPath, true);
            yield return SaveSessionFixture.Load(DirectoryPath);
            var session = Object.FindFirstObjectByType<GameBootstrap>();
            session.OpenHub(); while (session.IsLoading) yield return null;
            session.Profile.bankGold = 500; session.Profile.totalClears = 2;
            session.Profile.lastSettledRunId = "relaunch-settlement";
            Assert.That(session.Rebirth(), Is.True); Assert.That(session.BuyUpgrade(0), Is.True);
            Assert.That(session.Rebirth(), Is.True); Assert.That(session.SetMasterVolume(0.3f), Is.True);
            Assert.That(session.SaveError, Is.Empty);
            Assert.That(session.Profile.bankGold, Is.EqualTo(200));
            TestContext.WriteLine("RELAUNCH_WRITE count=2 points=1 bank=200 multiplier=2.7 upgrade=max-health volume=0.3");
        }
        [UnityTest, Explicit("Second half, in a new Unity process")]
        public IEnumerator ReadProfileWrittenByPreviousUnityProcess()
        {
            Assert.That(File.Exists(Path.Combine(DirectoryPath, "profile.json")), Is.True, "Run Write in a separate Unity process first.");
            yield return SaveSessionFixture.Load(DirectoryPath);
            var session = Object.FindFirstObjectByType<GameBootstrap>(); var profile = session.Profile;
            Assert.That(session.Flow.Phase, Is.EqualTo(RunFlowPhase.MainMenu)); Assert.That(session.Dungeon, Is.Null);
            Assert.That(profile.rebirthCount, Is.EqualTo(2)); Assert.That(profile.rebirthPoints, Is.EqualTo(1));
            Assert.That(profile.bankGold, Is.EqualTo(200)); Assert.That(profile.permanentGoldMultiplier, Is.EqualTo(2.7f));
            Assert.That(profile.upgradeIds, Is.EqualTo(new[] { "max-health" }));
            Assert.That(profile.unlockIds, Is.EqualTo(new[] { "dagger", "sword", "greatsword", "bow" }));
            Assert.That(profile.totalClears, Is.EqualTo(2)); Assert.That(profile.lastSettledRunId, Is.EqualTo("relaunch-settlement"));
            Assert.That(profile.settings.masterVolume, Is.EqualTo(0.3f)); Assert.That(session.SaveError, Is.Empty);
            TestContext.WriteLine("RELAUNCH_READ all persisted fields matched; phase=MainMenu, no active run");
        }
    }
}
