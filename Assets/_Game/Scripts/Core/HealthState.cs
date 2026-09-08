using System;
namespace Roguelite.Core
{
    public sealed class HealthState
    {
        public HealthState(float maximum)
        {
            if (maximum <= 0 || float.IsNaN(maximum) || float.IsInfinity(maximum)) throw new ArgumentOutOfRangeException(nameof(maximum));
            Maximum = maximum; Current = maximum;
        }
        public float Maximum { get; }
        public float Current { get; private set; }
        public bool IsDead => Current <= 0;
        public event Action Died;
        public float Receive(float damage)
        {
            if (IsDead || damage <= 0 || float.IsNaN(damage)) return 0;
            var applied = Math.Min(Current, damage);
            Current -= applied;
            if (IsDead) Died?.Invoke();
            return applied;
        }
    }
}
