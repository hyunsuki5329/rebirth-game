using System;
using Roguelite.Core;
using UnityEngine;
namespace Roguelite
{
    public sealed class MomentumSystem : MonoBehaviour
    {
        public MomentumConfig config;
        public Func<double> Clock { get; set; } = () => Time.timeAsDouble;
        public MomentumModel Model { get; private set; }
        public float GoldMultiplier => Model.Multiplier(config.maximumGoldBonus);
        private PlayerCombat combat;
        private PlayerDash dash;
        private ComboSystem combo;
        private PlayerHealth health;
        private double dashWindowEnds = double.NegativeInfinity, roomStarted;
        private bool dashBonusAvailable, hitSinceCombo;
        private void Awake()
        {
            Model = new MomentumModel(config.maximum, config.grace, config.decay);
            combat = GetComponent<PlayerCombat>(); dash = GetComponent<PlayerDash>();
            combo = GetComponent<ComboSystem>(); health = GetComponent<PlayerHealth>();
        }
        private void OnEnable()
        {
            combat.Hit += OnHit; combat.Killed += OnKill; dash.Ended += OnDashEnded; combo.Completed += OnCombo;
        }
        private void OnDisable()
        {
            combat.Hit -= OnHit; combat.Killed -= OnKill; dash.Ended -= OnDashEnded; combo.Completed -= OnCombo;
            dashBonusAvailable = hitSinceCombo = false;
        }
        private void Update() { Model.Tick(Clock()); ApplySpeed(); }
        private void ApplySpeed() => combat.SpeedMultiplier = Model.Multiplier(config.maximumAttackSpeedBonus);
        private void Gain(float amount) { if (!health.IsDead) Model.Add(amount, Clock()); ApplySpeed(); }
        private void OnHit(HealthBody target, float amount)
        {
            if (amount <= 0) return;
            Gain(config.hitGain); hitSinceCombo = true;
            if (dashBonusAvailable && Clock() <= dashWindowEnds) { dashBonusAvailable = false; Gain(config.dashHitGain); }
        }
        private void OnKill(HealthBody target) => Gain(config.killGain);
        private void OnDashEnded() { dashBonusAvailable = true; dashWindowEnds = Clock() + config.dashHitWindow; }
        private void OnCombo(string id)
        {
            if (hitSinceCombo) Gain(config.comboGain);
            hitSinceCombo = false;
        }
        public void BeginRoom() { roomStarted = Clock(); dashBonusAvailable = false; }
        public void ClearRoom(bool hadEnemies)
        {
            if (hadEnemies && Clock() - roomStarted <= config.fastClearWindow) Gain(config.fastClearGain);
        }
    }
}
