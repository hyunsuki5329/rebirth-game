using UnityEngine;
namespace Roguelite
{
    [CreateAssetMenu(menuName = "Roguelite/Attack")]
    public class AttackDefinition : ScriptableObject
    {
        public float damage = 12;
        public float range = 1.8f;
        public float arc = 100;
        public float windup = 0.06f;
        public float activeTime = 0.12f;
        public float recovery = 0.25f;
        public float knockback = 4;
        public float movementMultiplier = 0.7f;
    }
}
