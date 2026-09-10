using UnityEngine;
namespace Roguelite
{
    [CreateAssetMenu(menuName = "Roguelite/Shortcut Rules")]
    public sealed class ShortcutRules : ScriptableObject
    {
        public float eliteGoldMultiplier = 2.5f;
        [Range(0, 1)] public float safeRareChance = 0.2f, eliteRareChance = 0.7f;
        public int rareStacks = 2;
        public float firstHazardDelay = 0.6f, hazardInterval = 3, warningTime = 0.7f;
        public float hazardRadius = 1.3f, hazardDamage = 14, hazardLifetime = 0.15f;
        public bool IsRare(int seed, int room, bool elite)
        {
            var random = new System.Random(unchecked(seed * 397 ^ room * 7919));
            return random.NextDouble() < (elite ? eliteRareChance : safeRareChance);
        }
    }
}
