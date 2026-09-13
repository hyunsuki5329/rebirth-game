using System;
namespace Roguelite.Core
{
    [Serializable]
    public sealed class ProfileSettings
    {
        public float masterVolume = 1;
    }
    [Serializable]
    public sealed class ProfileData
    {
        public int bankGold;
        public int totalClears;
        public string lastSettledRunId = "";
        public int rebirthCount, rebirthPoints;
        public float permanentGoldMultiplier = 1;
        public string[] upgradeIds = Array.Empty<string>();
        public string[] unlockIds = { "dagger", "sword", "greatsword", "bow" };
        public ProfileSettings settings = new ProfileSettings();
    }
}
