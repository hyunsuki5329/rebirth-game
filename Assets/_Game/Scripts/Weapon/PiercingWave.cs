using System.Collections.Generic;
using UnityEngine;
namespace Roguelite
{
    public sealed class PiercingWave : MonoBehaviour
    {
        private HealthBody owner;
        private float damage, expires;
        private readonly HashSet<HealthBody> hit = new HashSet<HealthBody>();
        private System.Action<HealthBody, float> onHit;
        public void Initialize(HealthBody source, Vector2 direction, float speed, float power, float lifetime,
            System.Action<HealthBody, float> callback = null)
        {
            onHit = callback;
            owner = source; damage = power; expires = Time.time + lifetime;
            var body = GetComponent<Rigidbody2D>(); body.gravityScale = 0; body.linearVelocity = direction.normalized * speed;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        private void Update() { if (Time.time >= expires || owner == null || owner.IsDead) Destroy(gameObject); }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (owner == null || owner.IsDead || Time.time >= expires) return;
            if (other.TryGetComponent<Hurtbox>(out var hurt) && hurt.health != null)
            {
                if (hurt.health.team != owner.team && hit.Add(hurt.health))
                {
                    float amount = hurt.health.Receive(damage, Vector2.zero);
                    if (amount > 0) onHit?.Invoke(hurt.health, amount);
                }
            }
            else if (!other.isTrigger) { gameObject.SetActive(false); Destroy(gameObject); }
        }
    }
}
