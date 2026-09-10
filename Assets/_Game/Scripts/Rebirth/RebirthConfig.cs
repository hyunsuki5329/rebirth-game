using Roguelite.Core;
using UnityEngine;
namespace Roguelite
{
    [CreateAssetMenu(menuName = "Roguelite/Rebirth Config")]
    public sealed class RebirthConfig : ScriptableObject
    {
        public int initialCost = 100;
        public float[] goldMultipliers = { 1, 2, 2.7f, 3.4f };
        public float threatGoldMultiplier = 1.2f, threatProjectileSpread = 20;
        public RebirthUpgradeData[] upgrades;
        public RebirthSystem CreateRules() => new RebirthSystem(initialCost, goldMultipliers);
    }
}
