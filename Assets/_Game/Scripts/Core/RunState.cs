using System;
namespace Roguelite.Core
{
    public sealed class RunState
    {
        public string Id { get; } = Guid.NewGuid().ToString("N");
        public int Seed { get; }
        public int RoomIndex { get; set; }
        public int Gold { get; private set; }
        private readonly System.Collections.Generic.HashSet<string> rewardedRooms = new System.Collections.Generic.HashSet<string>();
        public RunState(int seed) { Seed = seed; }
        public bool Award(string roomId, int gold)
        {
            if (string.IsNullOrEmpty(roomId)) throw new ArgumentException("Room ID is required.", nameof(roomId));
            if (!rewardedRooms.Add(roomId)) return false;
            Gold = checked(Gold + Math.Max(0, gold));
            return true;
        }
    }
    public static class RewardCalculator
    {
        public static int Gold(int baseGold, float multiplier)
        {
            if (float.IsNaN(multiplier) || float.IsInfinity(multiplier)) throw new ArgumentOutOfRangeException(nameof(multiplier));
            return checked((int)Math.Floor(Math.Max(0, baseGold) * (double)Math.Max(0, multiplier)));
        }
    }
}
