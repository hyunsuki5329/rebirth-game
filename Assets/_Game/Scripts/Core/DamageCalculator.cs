namespace Roguelite.Core
{
    public static class DamageCalculator
    {
        public static float Resolve(float damage, float multiplier, float armor)
            => System.Math.Max(0, System.Math.Max(0, damage) * System.Math.Max(0, multiplier) - System.Math.Max(0, armor));
    }
}
