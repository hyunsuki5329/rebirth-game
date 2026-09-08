namespace Roguelite.Core
{
    public sealed class MomentumModel
    {
        public float Value { get; private set; }
        private readonly float maximum, grace, decay;
        private double lastTick, lastGain = double.NegativeInfinity;
        public MomentumModel(float maximum, float grace, float decay)
        {
            if (!(maximum > 0) || !(grace >= 0) || !(decay >= 0)) throw new System.ArgumentOutOfRangeException(nameof(maximum));
            this.maximum = maximum; this.grace = grace; this.decay = decay;
        }
        public void Add(float amount, double now)
        {
            Tick(now);
            if (!(amount > 0)) return;
            Value = System.Math.Min(maximum, Value + amount); lastGain = now;
        }
        public void Tick(double now)
        {
            if (now < lastTick || double.IsNaN(now) || double.IsInfinity(now)) throw new System.ArgumentOutOfRangeException(nameof(now));
            var elapsed = System.Math.Max(0, now - System.Math.Max(lastTick, lastGain + grace));
            Value = System.Math.Max(0, Value - (float)(elapsed * decay)); lastTick = now;
        }
        public float Multiplier(float maximumBonus) => 1 + Value / maximum * System.Math.Max(0, maximumBonus);
    }
}
