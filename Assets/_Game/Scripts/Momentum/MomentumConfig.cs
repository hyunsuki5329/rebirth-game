using UnityEngine;
namespace Roguelite
{
    [CreateAssetMenu(menuName = "Roguelite/Momentum")]
    public sealed class MomentumConfig : ScriptableObject
    {
        public float maximum = 100, grace = 3, decay = 8;
        public float hitGain = 4, killGain = 10, comboGain = 12, fastClearGain = 10;
        public float dashHitGain = 8, dashHitWindow = 0.7f, fastClearWindow = 20;
        public float maximumAttackSpeedBonus = 0.3f, maximumGoldBonus = 0.5f;
    }
}
