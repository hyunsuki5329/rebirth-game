using UnityEngine;
namespace Roguelite
{
    [CreateAssetMenu(menuName = "Roguelite/Run Flow Config")]
    public sealed class RunFlowConfig : ScriptableObject
    {
        public WeaponData[] weapons;
        public float victoryRetention = 1, deathRetention = 0.5f;
        public int firstSeed = 123;
    }
}
