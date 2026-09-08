using Roguelite.Core;
using UnityEngine;
namespace Roguelite
{
    [CreateAssetMenu(menuName = "Roguelite/Combo")]
    public sealed class ComboDefinition : ScriptableObject
    {
        public string id;
        public CombatAction[] steps;
        public float stepWindow = 1;
        public int priority;
        public ComboRule CreateRule(float extraWindow = 0) => new ComboRule(id, steps, stepWindow + extraWindow, priority);
    }
}
