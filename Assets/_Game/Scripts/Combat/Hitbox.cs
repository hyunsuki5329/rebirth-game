using System;
using System.Collections.Generic;
using UnityEngine;
namespace Roguelite
{
    public sealed class Hitbox : MonoBehaviour
    {
        private readonly HashSet<HealthBody> hit = new HashSet<HealthBody>();
        public Guid AttackId { get; private set; }
        private int ownerTeam;
        private float damage, knockback, arc, radius, expires;
        private Vector2 direction;
        private HealthBody owner;
        private Action<HealthBody, float> onHit;
        public void Initialize(HealthBody source, Vector2 facing, float range, float angle, float power, float force, float lifetime, Action<HealthBody, float> callback)
        {
            AttackId = Guid.NewGuid(); owner = source; ownerTeam = source.team;
            direction = facing.normalized; radius = range; arc = angle; damage = power; knockback = force;
            expires = Time.time + lifetime; onHit = callback;
        }
        private void Update() { if (Time.time >= expires || owner == null || owner.IsDead) Destroy(gameObject); }
        private void OnTriggerEnter2D(Collider2D other) => TryHit(other.GetComponent<Hurtbox>());
        private void OnTriggerStay2D(Collider2D other) => TryHit(other.GetComponent<Hurtbox>());
        public bool TryHit(Hurtbox hurtbox)
        {
            if (hurtbox == null || owner == null || owner.IsDead || Time.time >= expires) return false;
            var target = hurtbox.health;
            if (target == null || target.IsDead || target.team == ownerTeam || hit.Contains(target)) return false;
            Vector2 offset = target.transform.position - transform.position;
            if (offset.magnitude > radius + 0.35f || (offset.sqrMagnitude > 0.001f && Vector2.Angle(direction, offset) > arc * 0.5f)) return false;
            hit.Add(target);
            float actual = target.Receive(damage, offset.normalized * knockback);
            if (actual > 0) onHit?.Invoke(target, actual);
            return actual > 0;
        }
    }
}
