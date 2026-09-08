using UnityEngine;
namespace Roguelite
{
    public sealed class Projectile : MonoBehaviour
    {
        private HealthBody owner;
        private float damage, force, expires;
        private Vector2 direction;
        private bool spent;
        public void Initialize(HealthBody source, Vector2 facing, float speed, float power, float knockback, float lifetime)
        {
            owner = source; direction = facing.normalized; damage = power; force = knockback; expires = Time.time + lifetime;
            var body = GetComponent<Rigidbody2D>(); body.gravityScale = 0; body.linearVelocity = direction * speed;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        private void Update() { if (Time.time >= expires || owner == null || owner.IsDead) Destroy(gameObject); }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (spent || owner == null || owner.IsDead) return;
            var hurt = other.GetComponent<Hurtbox>();
            if (hurt != null && hurt.health != null)
            {
                if (hurt.health.team == owner.team) return;
                spent = true;
                hurt.health.Receive(damage, direction * force);
                Destroy(gameObject);
            }
            else if (!other.isTrigger) { spent = true; Destroy(gameObject); }
        }
    }
}
