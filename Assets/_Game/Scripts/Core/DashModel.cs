namespace Roguelite.Core
{
    public sealed class DashModel
    {
        private readonly float duration, cooldown;
        private double end = double.NegativeInfinity;
        public DashModel(float distance, float duration, float cooldown)
        {
            if (distance <= 0 || duration <= 0 || cooldown < 0 || float.IsNaN(distance) || float.IsNaN(duration))
                throw new System.ArgumentOutOfRangeException(nameof(duration));
            this.duration = duration;
            this.cooldown = cooldown;
            Speed = distance / duration;
        }
        public float Speed { get; }
        public bool TryStart(double now)
        {
            if (now < end + cooldown) return false;
            end = now + duration;
            return true;
        }
        public bool IsDashing(double now) => now < end;
        public double CooldownRemaining(double now) => System.Math.Max(0, end + cooldown - now);
    }
}
