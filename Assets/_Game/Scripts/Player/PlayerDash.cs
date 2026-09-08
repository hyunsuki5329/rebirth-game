using System;
using Roguelite.Core;
using UnityEngine;
namespace Roguelite
{
    [DefaultExecutionOrder(-60)]
    public sealed class PlayerDash : MonoBehaviour
    {
        public PlayerStats stats;
        private PlayerInputReader input;
        private PlayerAim aim;
        private DashModel model;
        private Vector2 direction;
        public event Action Started;
        public event Action Ended;
        private bool wasActive;
        public bool IsActive => model != null && model.IsDashing(Time.timeAsDouble);
        public float CooldownRemaining => model == null ? 0 : (float)model.CooldownRemaining(Time.timeAsDouble);
        public Vector2 Velocity => direction * model.Speed;
        private void Awake()
        {
            input = GetComponent<PlayerInputReader>();
            aim = GetComponent<PlayerAim>();
            model = new DashModel(stats.dashDistance, stats.dashDuration, stats.dashCooldown);
        }
        private void Update()
        {
            if (wasActive && !IsActive) { wasActive = false; Ended?.Invoke(); }
            if (input.DashPressed && model.TryStart(Time.timeAsDouble))
            {
                direction = input.Move.sqrMagnitude > 0.001f ? input.Move.normalized : aim.Direction;
                wasActive = true;
                Started?.Invoke();
            }
        }
    }
}
