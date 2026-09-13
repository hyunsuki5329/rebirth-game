using System;
using System.Collections.Generic;
using Roguelite.Core;
using UnityEngine;
namespace Roguelite
{
    public sealed class SaveFormatException : Exception
    {
        public SaveFormatException(string message) : base(message) { }
    }
    [Serializable]
    public sealed class SaveData
    {
        public int schemaVersion;
        public ProfileData profile;
    }
    public sealed class SaveCodec
    {
        public const int CurrentVersion = 1;
        private readonly HashSet<string> weapons, upgrades;
        public SaveCodec(IEnumerable<string> weaponIds, IEnumerable<string> upgradeIds)
        { weapons = new HashSet<string>(weaponIds); upgrades = new HashSet<string>(upgradeIds); }
        public string Encode(ProfileData profile)
        {
            Validate(profile);
            return JsonUtility.ToJson(new SaveData { schemaVersion = CurrentVersion, profile = profile }, true);
        }
        public ProfileData Decode(string json)
        {
            SaveData data;
            try { data = JsonUtility.FromJson<SaveData>(json); }
            catch (ArgumentException) { throw new SaveFormatException("Save JSON is damaged."); }
            if (data == null) throw new SaveFormatException("Save is empty.");
            // Add a real schema loader here only when a later schema actually exists.
            if (data.schemaVersion != CurrentVersion) throw new SaveFormatException("Unsupported save schema: " + data.schemaVersion);
            Validate(data.profile); return data.profile;
        }
        private void Validate(ProfileData profile)
        {
            if (profile == null || profile.settings == null || profile.lastSettledRunId == null ||
                profile.bankGold < 0 || profile.totalClears < 0 || profile.rebirthCount < 0 || profile.rebirthPoints < 0 ||
                profile.rebirthCount > 1000000 || !Finite(profile.permanentGoldMultiplier) || profile.permanentGoldMultiplier < 1 ||
                !Finite(profile.settings.masterVolume) || profile.settings.masterVolume < 0 || profile.settings.masterVolume > 1)
                throw new SaveFormatException("Save contains invalid profile values.");
            ValidateIds(profile.unlockIds, weapons); ValidateIds(profile.upgradeIds, upgrades);
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static void ValidateIds(string[] ids, HashSet<string> known)
        {
            if (ids == null) throw new SaveFormatException("Save IDs are missing.");
            var seen = new HashSet<string>();
            foreach (string id in ids)
                if (id == null || !known.Contains(id) || !seen.Add(id)) throw new SaveFormatException("Save contains an unknown or repeated ID.");
        }
    }
}
