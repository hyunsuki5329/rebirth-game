using System.Collections.Generic;
using Roguelite.Core;
using UnityEngine;
namespace Roguelite
{
    public sealed class RunInventory : MonoBehaviour
    {
        private readonly Dictionary<string, int> stacks = new Dictionary<string, int>();
        private readonly Dictionary<string, ItemData> items = new Dictionary<string, ItemData>();
        public int Count(string id) => stacks.TryGetValue(id, out int count) ? count : 0;
        public bool Add(ItemData item)
        {
            int count = Count(item.id);
            if (count >= item.maximumStacks) return false;
            stacks[item.id] = count + 1; items[item.id] = item; Apply(); return true;
        }
        private void Apply()
        {
            float damage = 1, movement = 1, speed = 1;
            foreach (var entry in items)
            {
                float bonus = WeaponStats.Modified(1, entry.Value.bonusPerStack, Count(entry.Key)) - 1;
                switch (entry.Value.stat)
                {
                    case ItemStat.Damage: damage += bonus; break;
                    case ItemStat.Movement: movement += bonus; break;
                    case ItemStat.AttackSpeed: speed += bonus; break;
                }
            }
            var combat = GetComponent<PlayerCombat>(); combat.DamageMultiplier = damage; combat.ItemSpeedMultiplier = speed;
            GetComponent<PlayerMovement>().BonusMultiplier = movement;
        }
    }
}
