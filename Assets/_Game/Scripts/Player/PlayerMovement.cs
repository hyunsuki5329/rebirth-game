using UnityEngine;
using Roguelite.Core;
namespace Roguelite
{
    public sealed class PlayerMovement : MonoBehaviour
    {
        public PlayerStats stats;
        private PlayerInputReader input;
        private PlayerSprint sprint;
        public float ActionMultiplier { get; set; } = 1;
        public float BonusMultiplier { get; set; } = 1;
        private void Awake() { input = GetComponent<PlayerInputReader>(); sprint = GetComponent<PlayerSprint>(); }
        public Vector2 Velocity
        {
            get
            {
                var v = MovementRules.Velocity(input.Move.x, input.Move.y, stats.moveSpeed * ActionMultiplier * BonusMultiplier,
                    sprint.Multiplier, sprint.IsSprinting);
                return new Vector2(v.X, v.Y);
            }
        }
    }
}
