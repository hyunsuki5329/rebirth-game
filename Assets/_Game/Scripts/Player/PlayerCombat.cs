using System;
using UnityEngine;
namespace Roguelite
{
    public sealed class PlayerCombat : MonoBehaviour
    {
        public AttackDefinition attack;
        private PlayerInputReader input;
        private PlayerAim aim;
        private PlayerMovement movement;
        private HealthBody health;
        private CombatEffects effects;
        private Vector2 direction;
        private float impactAt, readyAt, activeDuration;
        private bool pending;
        public float DamageMultiplier { get; set; } = 1;
        public float SpeedMultiplier { get; set; } = 1;
        public float ItemSpeedMultiplier { get; set; } = 1;
        public event Action AttackStarted;
        public event Action<HealthBody, float> Hit;
        public event Action<HealthBody> Killed;
        public bool IsAttacking => Time.time < readyAt;
        public Vector2 AttackDirection => direction;
        private void Awake()
        {
            input = GetComponent<PlayerInputReader>(); aim = GetComponent<PlayerAim>();
            movement = GetComponent<PlayerMovement>(); health = GetComponent<HealthBody>();
            effects = GetComponent<CombatEffects>() ?? gameObject.AddComponent<CombatEffects>();
        }
        private void Update()
        {
            if (health.IsDead) { pending = false; movement.ActionMultiplier = 1; return; }
            if (input.AttackPressed) RequestAttack(aim.Direction);
            if (pending && Time.time >= impactAt) { pending = false; Strike(); }
            movement.ActionMultiplier = IsAttacking ? attack.movementMultiplier : 1;
        }
        public bool RequestAttack(Vector2 facing)
        {
            if (!isActiveAndEnabled || health.IsDead || IsAttacking || facing.sqrMagnitude < 0.001f) return false;
            direction = facing.normalized;
            float speed = Mathf.Max(0.1f, SpeedMultiplier * ItemSpeedMultiplier);
            activeDuration = attack.activeTime / speed;
            impactAt = Time.time + attack.windup / speed;
            readyAt = Time.time + (attack.windup + attack.activeTime + attack.recovery) / speed;
            pending = true;
            ShowVisual(new Color(1, 0.8f, 0.3f, 0.12f), attack.windup / speed);
            AttackStarted?.Invoke();
            return true;
        }
        private void Strike()
        {
            if (attack.kind == PlayerAttackKind.Projectile) { FireArrow(); return; }
            var go = new GameObject("Player attack", typeof(CircleCollider2D), typeof(Hitbox));
            effects.Track(go);
            go.transform.position = transform.position;
            var collider = go.GetComponent<CircleCollider2D>();
            collider.radius = attack.range; collider.isTrigger = true;
            go.GetComponent<Hitbox>().Initialize(health, direction, attack.range, attack.arc,
                attack.damage * DamageMultiplier, attack.knockback, activeDuration, ReportHit);
            ShowVisual(new Color(0.4f, 1, 0.85f, 0.35f), activeDuration);
        }
        internal void ReportHit(HealthBody target, float amount)
        {
            Hit?.Invoke(target, amount); if (target.IsDead) Killed?.Invoke(target);
        }
        private void FireArrow()
        {
            var arrow = new GameObject("Player arrow", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(Projectile));
            effects.Track(arrow); arrow.layer = LayerMask.NameToLayer("Projectile");
            arrow.transform.position = transform.position;
            arrow.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            arrow.GetComponent<SpriteRenderer>().sprite = attack.projectileSprite;
            arrow.GetComponent<SpriteRenderer>().sortingOrder = 6;
            var collider = arrow.GetComponent<BoxCollider2D>(); collider.isTrigger = true; collider.size = new Vector2(0.55f, 0.12f);
            arrow.GetComponent<Projectile>().Initialize(health, direction, attack.projectileSpeed,
                attack.damage * DamageMultiplier, attack.knockback, attack.projectileLifetime, ReportHit);
        }
        private void ShowVisual(Color color, float duration)
        {
            var visual = new GameObject("Attack range", typeof(AttackVisual));
            effects.Track(visual);
            visual.transform.position = transform.position;
            visual.GetComponent<AttackVisual>().Initialize(attack.range, attack.arc, direction, color, duration);
        }
        private void OnDisable()
        {
            pending = false;
            readyAt = 0;
            if (movement != null) movement.ActionMultiplier = 1;
        }
    }
}
