using UnityEngine;
namespace Roguelite
{
    [CreateAssetMenu(menuName = "Roguelite/Player Stats")]
    public sealed class PlayerStats : ScriptableObject
    {
        [Min(0.1f)] public float moveSpeed = 5;
        [Min(1)] public float sprintMultiplier = 1.5f;
        [Min(0.1f)] public float dashDistance = 3;
        [Min(0.02f)] public float dashDuration = 0.16f;
        [Min(0.1f)] public float dashCooldown = 0.8f;
    }
}
