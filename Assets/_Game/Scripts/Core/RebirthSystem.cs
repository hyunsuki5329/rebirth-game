using System;
namespace Roguelite.Core
{
    public sealed class RebirthSystem
    {
        private readonly int initialCost;
        private readonly float[] multipliers;
        public RebirthSystem(int initialCost, float[] multipliers)
        {
            if (initialCost <= 0 || multipliers == null || multipliers.Length < 2) throw new ArgumentException("Rebirth configuration is incomplete.");
            this.initialCost = initialCost; this.multipliers = (float[])multipliers.Clone();
        }
        public int Cost(ProfileData profile) => checked(initialCost * (profile.rebirthCount + 1));
        public float Multiplier(int count)
        {
            count = Math.Max(0, count);
            int last = multipliers.Length - 1;
            return count <= last ? multipliers[count] : multipliers[last] + (count - last) * (multipliers[last] - multipliers[last - 1]);
        }
        public bool CanRebirth(ProfileData profile) => profile.totalClears > 0 && profile.bankGold >= Cost(profile);
        public bool TryRebirth(ProfileData profile)
        {
            if (!CanRebirth(profile)) return false;
            int cost = Cost(profile), count = checked(profile.rebirthCount + 1), points = checked(profile.rebirthPoints + 1);
            profile.bankGold -= cost; profile.rebirthCount = count; profile.rebirthPoints = points;
            profile.permanentGoldMultiplier = Multiplier(count); return true;
        }
        public bool TryBuy(ProfileData profile, string id, int cost)
        {
            if (string.IsNullOrWhiteSpace(id) || cost <= 0 || profile.rebirthPoints < cost || Array.IndexOf(profile.upgradeIds, id) >= 0) return false;
            var upgrades = new string[profile.upgradeIds.Length + 1]; Array.Copy(profile.upgradeIds, upgrades, profile.upgradeIds.Length); upgrades[upgrades.Length - 1] = id;
            profile.rebirthPoints -= cost; profile.upgradeIds = upgrades; return true;
        }
    }
    public static class WorldThreatSystem
    {
        public static int Level(int count) => Math.Max(0, count);
        public static int ProjectileCount(int threat) => threat > 0 ? 3 : 1;
        public static float GoldMultiplier(int threat, float elevated) => threat > 0 ? elevated : 1;
    }
}
