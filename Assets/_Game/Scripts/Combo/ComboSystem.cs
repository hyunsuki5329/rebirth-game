using System;
using Roguelite.Core;
using UnityEngine;
namespace Roguelite
{
    public sealed class ComboSystem : MonoBehaviour
    {
        public ComboDefinition[] definitions;
        public float PermanentWindowBonus { get; set; }
        public Func<double> Clock { get; set; } = () => Time.timeAsDouble;
        public string LastCompleted { get; private set; }
        public int CompletedCount { get; private set; }
        public event Action<string> Completed;
        private ComboRecognizer recognizer;
        private PlayerCombat combat;
        private PlayerDash dash;
        private PlayerHealth health;
        private void Awake()
        {
            combat = GetComponent<PlayerCombat>(); dash = GetComponent<PlayerDash>(); health = GetComponent<PlayerHealth>();
            SetDefinitions(definitions);
        }
        public void SetDefinitions(ComboDefinition[] values, float extraWindow = 0)
        {
            var rules = new ComboRule[values.Length];
            for (int i = 0; i < values.Length; i++) rules[i] = values[i].CreateRule(extraWindow + PermanentWindowBonus);
            recognizer = new ComboRecognizer(rules); LastCompleted = null;
        }
        private void OnEnable() { combat.AttackStarted += OnAttack; dash.Started += OnDash; health.Died += ResetProgress; }
        private void OnDisable() { combat.AttackStarted -= OnAttack; dash.Started -= OnDash; health.Died -= ResetProgress; ResetProgress(); }
        private void OnAttack() => Push(CombatAction.Attack);
        private void OnDash() => Push(CombatAction.Dash);
        private void Push(CombatAction action)
        {
            if (health.IsDead) return;
            string result = recognizer.Push(action, Clock());
            if (result == null) return;
            LastCompleted = result; CompletedCount++; Completed?.Invoke(result);
        }
        public void ResetProgress() => recognizer?.Reset();
    }
}
