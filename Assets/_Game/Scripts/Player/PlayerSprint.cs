using UnityEngine;
namespace Roguelite
{
    public sealed class PlayerSprint : MonoBehaviour
    {
        public PlayerStats stats;
        private PlayerInputReader input;
        private void Awake() => input = GetComponent<PlayerInputReader>();
        public bool IsSprinting => input.SprintHeld;
        public float Multiplier => IsSprinting ? stats.sprintMultiplier : 1;
    }
}
