using System;
namespace Roguelite.Core
{
    [Serializable]
    public sealed class ProfileData
    {
        public int bankGold;
        public int totalClears;
        public string lastSettledRunId = "";
    }
}
