using System;
using System.IO;
using NUnit.Framework;
using Roguelite.Core;
namespace Roguelite.Tests
{
    public class SaveSerializationTests
    {
        private string directory;
        private SaveCodec codec;
        [SetUp] public void Setup()
        {
            directory = Path.Combine(Path.GetTempPath(), "RogueliteSaveTests", Guid.NewGuid().ToString("N"));
            codec = new SaveCodec(new[] { "dagger", "sword", "greatsword", "bow" }, new[] { "max-health", "combo-window" });
        }
        [TearDown] public void Cleanup() { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        private ProfileData Profile() => new ProfileData
        {
            bankGold = 123, totalClears = 4, lastSettledRunId = "completed-run",
            rebirthCount = 2, rebirthPoints = 1, permanentGoldMultiplier = 2.7f,
            upgradeIds = new[] { "max-health" }, unlockIds = new[] { "bow", "dagger" },
            settings = new ProfileSettings { masterVolume = 0.35f }
        };
        [Test]
        public void AllRequiredFieldsRoundTripAndMissingFileCreatesDefault()
        {
            var store = new ProfileFileStore(directory, codec);
            var initial = store.Load(); Assert.That(initial.Profile.bankGold, Is.Zero);
            Assert.That(initial.Profile.settings.masterVolume, Is.EqualTo(1));
            var original = Profile(); string json = codec.Encode(original);
            Assert.That(json, Does.Contain("\"schemaVersion\": 1"));
            Assert.That(store.TrySave(original, out var error), Is.True, error);
            var loaded = new ProfileFileStore(directory, codec).Load().Profile;
            Assert.That(codec.Encode(loaded), Is.EqualTo(json));
            Assert.That(File.Exists(store.FilePath + ".tmp"), Is.False);
        }
        [TestCase("{}")]
        [TestCase("not json")]
        [TestCase("{\"schemaVersion\":99,\"profile\":{}}")]
        public void InvalidOrUnsupportedFileIsPreservedWithSafeDefault(string contents)
        {
            Directory.CreateDirectory(directory); var store = new ProfileFileStore(directory, codec);
            File.WriteAllText(store.FilePath, contents);
            var result = store.Load(); Assert.That(result.Profile.bankGold, Is.Zero); Assert.That(result.Notice, Is.Not.Empty);
            var preserved = Directory.GetFiles(directory, "profile.json.corrupt-*");
            Assert.That(preserved.Length, Is.EqualTo(1)); Assert.That(File.ReadAllText(preserved[0]), Is.EqualTo(contents));
            Assert.That(store.TrySave(result.Profile, out var error), Is.True, error);
        }
        [Test]
        public void ValidBackupRecoversAfterCorruptionAndSurvivesNextWrite()
        {
            var store = new ProfileFileStore(directory, codec); var profile = Profile();
            Assert.That(store.TrySave(profile, out _), Is.True);
            profile.bankGold = 999; Assert.That(store.TrySave(profile, out _), Is.True);
            File.WriteAllText(store.FilePath, "broken");
            var recovered = store.Load(); Assert.That(recovered.Profile.bankGold, Is.EqualTo(123));
            Assert.That(recovered.Notice, Does.Contain("backup"));
            Assert.That(store.TrySave(recovered.Profile, out _), Is.True);
            Assert.That(codec.Decode(File.ReadAllText(store.FilePath + ".bak")).bankGold, Is.EqualTo(123));
        }
        [Test]
        public void InvalidPrimaryAndBackupBothSurviveFallbackAndLaterSaves()
        {
            Directory.CreateDirectory(directory); var store = new ProfileFileStore(directory, codec);
            File.WriteAllText(store.FilePath, "invalid main"); File.WriteAllText(store.FilePath + ".bak", "invalid backup");
            var loaded = store.Load();
            Assert.That(store.TrySave(loaded.Profile, out _), Is.True);
            loaded.Profile.bankGold = 10; Assert.That(store.TrySave(loaded.Profile, out _), Is.True);
            var originals = Directory.GetFiles(directory, "*.corrupt-*");
            Assert.That(originals.Length, Is.EqualTo(2));
            Assert.That(System.Array.ConvertAll(originals, File.ReadAllText), Is.EquivalentTo(new[] { "invalid main", "invalid backup" }));
        }
        [Test]
        public void FailedReplacementKeepsPreviousFileAndAllowsRetryWithoutChangingProfile()
        {
            var store = new ProfileFileStore(directory, codec); var profile = Profile();
            Assert.That(store.TrySave(profile, out _), Is.True); string original = File.ReadAllText(store.FilePath);
            profile.bankGold = 456;
            var failing = new ProfileFileStore(directory, codec, (source, target, backup) => { throw new IOException("simulated replacement failure"); });
            Assert.That(failing.TrySave(profile, out var error), Is.False); Assert.That(error, Does.Contain("simulated"));
            Assert.That(File.ReadAllText(store.FilePath), Is.EqualTo(original)); Assert.That(profile.bankGold, Is.EqualTo(456));
            Assert.That(store.TrySave(profile, out _), Is.True); Assert.That(store.Load().Profile.bankGold, Is.EqualTo(456));
        }
        [Test]
        public void UnwritableLocationReportsFailureWithoutMutatingProfile()
        {
            Directory.CreateDirectory(directory); string path = Path.Combine(directory, "file-as-directory"); File.WriteAllText(path, "keep");
            var profile = Profile(); var store = new ProfileFileStore(path, codec);
            Assert.That(store.TrySave(profile, out var error), Is.False); Assert.That(error, Is.Not.Empty);
            Assert.That(File.ReadAllText(path), Is.EqualTo("keep")); Assert.That(profile.bankGold, Is.EqualTo(123));
        }
        [Test]
        public void UnknownIdsAndInvalidNumbersAreRejectedRatherThanSilentlyApplied()
        {
            string valid = codec.Encode(Profile());
            Assert.Throws<SaveFormatException>(() => codec.Decode(valid.Replace("max-health", "unknown-upgrade")));
            Assert.Throws<SaveFormatException>(() => codec.Decode(valid.Replace("\"bow\"", "\"unknown-weapon\"")));
            Assert.Throws<SaveFormatException>(() => codec.Decode(valid.Replace("\"bankGold\": 123", "\"bankGold\": -1")));
            var invalidVolume = UnityEngine.JsonUtility.FromJson<SaveData>(valid);
            invalidVolume.profile.settings.masterVolume = 2;
            Assert.Throws<SaveFormatException>(() => codec.Decode(UnityEngine.JsonUtility.ToJson(invalidVolume)));
        }
    }
}
