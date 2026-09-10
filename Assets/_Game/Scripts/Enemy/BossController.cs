using System;
using UnityEngine;
namespace Roguelite
{
    public enum BossPhase { Recovery, ChargeWarning, Charging, BlastWarning, Blasting }
    public sealed class BossController : EnemyAI
    {
        public BossPatternData patterns;
        public BossPhase Phase { get; private set; }
        public event Action<BossPhase> PhaseChanged;
        private float until;
        private bool blastNext;
        private Vector2 chargeDirection;
        protected override void Awake()
        {
            base.Awake(); Phase = BossPhase.Recovery; until = Time.time + patterns.initialRecovery;
        }
        protected override void Tick(Vector2 delta)
        {
            motor.DesiredVelocity = Phase == BossPhase.Charging ? chargeDirection * data.chargeSpeed : Vector2.zero;
            if (Time.time < until) return;
            switch (Phase)
            {
                case BossPhase.Recovery:
                    if (blastNext) BeginBlasts(delta);
                    else
                    {
                        chargeDirection = delta.sqrMagnitude > 0.001f ? delta.normalized : Vector2.left;
                        if (attack.TryBegin(chargeDirection)) Change(BossPhase.ChargeWarning, data.attack.windup);
                        else return;
                    }
                    blastNext = !blastNext; break;
                case BossPhase.ChargeWarning: Change(BossPhase.Charging, data.attack.activeTime); break;
                case BossPhase.Charging: Change(BossPhase.Recovery, data.attack.recovery); break;
                case BossPhase.BlastWarning: Change(BossPhase.Blasting, patterns.blastInterval * 2 + patterns.blastLifetime); break;
                case BossPhase.Blasting: Change(BossPhase.Recovery, patterns.blastRecovery); break;
            }
            motor.DesiredVelocity = Phase == BossPhase.Charging ? chargeDirection * data.chargeSpeed : Vector2.zero;
        }
        private void BeginBlasts(Vector2 delta)
        {
            var direction = delta.sqrMagnitude > 0.001f ? delta.normalized : Vector2.left;
            var across = new Vector2(-direction.y, direction.x);
            for (int i = 0; i < 3; i++)
            {
                var go = new GameObject("Boss sequential blast", typeof(TelegraphedHazard));
                go.transform.position = target.transform.position + (Vector3)across * ((i - 1) * patterns.blastSpacing);
                go.GetComponent<TelegraphedHazard>().Initialize(health, patterns.blastRadius,
                    patterns.blastWarning + i * patterns.blastInterval, patterns.blastDamage, patterns.blastLifetime);
                GetComponent<CombatEffects>().Track(go);
            }
            Change(BossPhase.BlastWarning, patterns.blastWarning);
        }
        private void Change(BossPhase phase, float duration)
        {
            Phase = phase; until = Time.time + duration; PhaseChanged?.Invoke(phase);
        }
        protected override void OnDisable()
        {
            base.OnDisable(); if (TryGetComponent<CombatEffects>(out var effects)) effects.Clear();
        }
    }
}
