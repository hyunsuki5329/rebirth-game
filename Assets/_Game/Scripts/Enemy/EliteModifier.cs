using UnityEngine;
namespace Roguelite
{
    public sealed class EliteModifier : MonoBehaviour
    {
        private PlayerHealth target;
        private EnemyHealth health;
        private ShortcutRules rules;
        private float nextHazard;
        public void Initialize(PlayerHealth player, ShortcutRules config)
        {
            target = player; rules = config; health = GetComponent<EnemyHealth>();
            nextHazard = Time.time + rules.firstHazardDelay;
        }
        private void Update()
        {
            if (rules == null || target == null || target.IsDead || health.IsDead || Time.time < nextHazard) return;
            nextHazard = Time.time + rules.hazardInterval;
            var go = new GameObject("Elite ground warning", typeof(TelegraphedHazard));
            go.transform.position = target.transform.position;
            go.GetComponent<TelegraphedHazard>().Initialize(health, rules.hazardRadius, rules.warningTime, rules.hazardDamage, rules.hazardLifetime);
            GetComponent<CombatEffects>().Track(go);
        }
    }
}
