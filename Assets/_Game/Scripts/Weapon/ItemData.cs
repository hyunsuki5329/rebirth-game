using UnityEngine;
namespace Roguelite
{
    public enum ItemStat { Damage, Movement, AttackSpeed }
    [CreateAssetMenu(menuName = "Roguelite/Run Item")]
    public sealed class ItemData : ScriptableObject
    {
        public string id;
        public ItemStat stat;
        public float bonusPerStack = 0.15f;
        public int maximumStacks = 3;
    }
}
