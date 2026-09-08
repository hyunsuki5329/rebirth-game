using System;
using System.Collections.Generic;
namespace Roguelite.Core
{
    public enum CombatAction { Attack, Dash }
    public sealed class ComboRule
    {
        public readonly string Id;
        public readonly CombatAction[] Steps;
        public readonly double Window;
        public readonly int Priority;
        public ComboRule(string id, CombatAction[] steps, double window, int priority = 0)
        {
            if (string.IsNullOrEmpty(id) || steps == null || steps.Length == 0 || !(window > 0)) throw new ArgumentException("Invalid combo rule.");
            Id = id; Steps = (CombatAction[])steps.Clone(); Window = window; Priority = priority;
        }
    }
    public sealed class ComboRecognizer
    {
        private readonly ComboRule[] rules;
        private readonly List<(CombatAction action, double time)> history = new List<(CombatAction, double)>();
        private readonly int maximumLength;
        public ComboRecognizer(params ComboRule[] rules)
        {
            this.rules = (ComboRule[])rules.Clone();
            foreach (var rule in rules) maximumLength = Math.Max(maximumLength, rule.Steps.Length);
        }
        public string Push(CombatAction action, double now)
        {
            if (history.Count > 0 && now < history[history.Count - 1].time) throw new ArgumentOutOfRangeException(nameof(now));
            history.Add((action, now));
            if (history.Count > maximumLength) history.RemoveAt(0);
            ComboRule winner = null;
            foreach (var rule in rules)
            {
                int start = history.Count - rule.Steps.Length;
                if (start < 0) continue;
                bool matches = true;
                for (int i = 0; i < rule.Steps.Length; i++)
                    if (history[start + i].action != rule.Steps[i] ||
                        i > 0 && history[start + i].time - history[start + i - 1].time > rule.Window) { matches = false; break; }
                if (matches && (winner == null || rule.Steps.Length > winner.Steps.Length ||
                    rule.Steps.Length == winner.Steps.Length && (rule.Priority > winner.Priority ||
                    rule.Priority == winner.Priority && string.CompareOrdinal(rule.Id, winner.Id) < 0))) winner = rule;
            }
            if (winner == null) return null;
            Reset(); return winner.Id;
        }
        public void Reset() => history.Clear();
    }
}
