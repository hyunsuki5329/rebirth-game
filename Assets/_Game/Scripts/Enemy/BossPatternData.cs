using UnityEngine;
namespace Roguelite
{
    [CreateAssetMenu(menuName = "Roguelite/Boss Patterns")]
    public sealed class BossPatternData : ScriptableObject
    {
        public float initialRecovery = 0.8f, blastWarning = 0.8f, blastInterval = 0.3f;
        public float blastRadius = 1.3f, blastDamage = 16, blastLifetime = 0.2f, blastRecovery = 1.3f;
        public float blastSpacing = 2.5f;
    }
}
