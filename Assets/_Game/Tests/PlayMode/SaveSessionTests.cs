using System.Collections;
using System.IO;
using NUnit.Framework;
using Roguelite.Core;
using UnityEngine;
using UnityEngine.TestTools;
namespace Roguelite.Tests
{
    public class SaveSessionTests
    {
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var session in Object.FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None)) Object.Destroy(session.gameObject);
            yield return null; SaveSessionFixture.Cleanup();
        }
        private IEnumerator Ready(GameBootstrap s)
        {
            float until = Time.realtimeSinceStartup + 15;
            while (s.IsLoading && Time.realtimeSinceStartup < until) yield return null;
            Assert.That(s.IsLoading, Is.False); yield return null;
        }
        [UnityTest]
        public IEnumerator RecreatedSessionLoadsSettlementRebirthUpgradeAndSettingsWithoutResumingRun()
        {
            string directory = SaveSessionFixture.NewDirectory(); yield return SaveSessionFixture.Load(directory);
            var session = Object.FindFirstObjectByType<GameBootstrap>();
            Assert.That(File.Exists(session.SavePath), Is.True); Assert.That(session.SaveError, Is.Empty);
            session.OpenHub(); yield return Ready(session);
            session.Profile.bankGold = 300; session.Profile.totalClears = 1;
            Assert.That(session.Rebirth(), Is.True); Assert.That(session.BuyUpgrade(0), Is.True);
            Assert.That(session.SetMasterVolume(0.4f), Is.True); Assert.That(AudioListener.volume, Is.EqualTo(0.4f));
            session.StartRun(); yield return Ready(session);
            session.Dungeon.Run.Award("fixture-reward", 20);
            session.Dungeon.Player.Receive(10000, Vector2.zero); yield return null;
            Assert.That(session.Profile.bankGold, Is.EqualTo(210)); string id = session.Profile.lastSettledRunId;
            Object.Destroy(session.gameObject); yield return null;
            yield return SaveSessionFixture.Load(directory);
            session = Object.FindFirstObjectByType<GameBootstrap>();
            Assert.That(session.Flow.Phase, Is.EqualTo(RunFlowPhase.MainMenu)); Assert.That(session.Dungeon, Is.Null);
            Assert.That(session.Profile.bankGold, Is.EqualTo(210)); Assert.That(session.Profile.lastSettledRunId, Is.EqualTo(id));
            Assert.That(session.Profile.rebirthCount, Is.EqualTo(1)); Assert.That(session.Profile.permanentGoldMultiplier, Is.EqualTo(2));
            Assert.That(session.Profile.rebirthPoints, Is.Zero); Assert.That(session.Profile.upgradeIds, Does.Contain("max-health"));
            Assert.That(session.Profile.settings.masterVolume, Is.EqualTo(0.4f)); Assert.That(AudioListener.volume, Is.EqualTo(0.4f));
        }
        [UnityTest]
        public IEnumerator SaveFailureKeepsLiveProfileAndExplicitRetryWritesIt()
        {
            string root = SaveSessionFixture.NewDirectory(); Directory.CreateDirectory(root);
            string blocked = Path.Combine(root, "blocked"); File.WriteAllText(blocked, "obstruction");
            yield return SaveSessionFixture.Load(blocked);
            var session = Object.FindFirstObjectByType<GameBootstrap>(); var profile = session.Profile;
            Assert.That(session.SaveError, Is.Not.Empty);
            session.SetMasterVolume(0.3f); Assert.That(session.Profile, Is.SameAs(profile));
            File.Delete(blocked); Assert.That(session.RetrySave(), Is.True); Assert.That(session.SaveError, Is.Empty);
            Assert.That(File.Exists(session.SavePath), Is.True);
        }
    }
}
