using UnityEngine;
namespace Roguelite
{
    public enum PermanentStat { MaximumHealth, ComboWindow }
    [CreateAssetMenu(menuName = "Roguelite/Rebirth Upgrade")]
    public sealed class RebirthUpgradeData : ScriptableObject
    {
        public string id;
        public PermanentStat stat;
        public int pointCost = 1;
        public float value;
    }
}
