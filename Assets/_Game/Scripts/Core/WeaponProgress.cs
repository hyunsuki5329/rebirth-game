namespace Roguelite.Core
{
    public sealed class WeaponProgress
    {
        public int ComboCount { get; private set; }
        public bool Evolved { get; private set; }
        private readonly int required;
        public WeaponProgress(int requiredCombos)
        {
            if (requiredCombos < 1) throw new System.ArgumentOutOfRangeException(nameof(requiredCombos));
            required = requiredCombos;
        }
        public bool RecordCombo(bool qualifying)
        {
            if (!qualifying) return false;
            ComboCount = System.Math.Min(required, ComboCount + 1); Evolved = ComboCount >= required;
            return Evolved;
        }
    }
    public static class WeaponStats
    {
        public static float Modified(float baseline, float bonusPerStack, int stacks) =>
            System.Math.Max(0, baseline) * (1 + System.Math.Max(0, bonusPerStack) * System.Math.Max(0, stacks));
        public static int[] RewardChoices(int seed, int room, int count, int available)
        {
            if (available < 0 || count < 0) throw new System.ArgumentOutOfRangeException(nameof(count));
            var pool = new int[available]; for (int i = 0; i < available; i++) pool[i] = i;
            var random = new System.Random(unchecked(seed * 397 ^ room));
            for (int i = available - 1; i > 0; i--) { int j = random.Next(i + 1); (pool[i], pool[j]) = (pool[j], pool[i]); }
            var result = new int[System.Math.Min(count, available)]; System.Array.Copy(pool, result, result.Length); return result;
        }
    }
}
