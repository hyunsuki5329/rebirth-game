using UnityEngine;
namespace Roguelite
{
    [RequireComponent(typeof(Rigidbody2D), typeof(HealthBody))]
    public sealed class KnockbackBody : MonoBehaviour
    {
        public float deceleration = 12;
        private Rigidbody2D body;
        private HealthBody health;
        private void Awake()
        {
            body = GetComponent<Rigidbody2D>(); health = GetComponent<HealthBody>();
            health.KnockbackRequested += Apply;
        }
        private void Apply(Vector2 impulse) => body.linearVelocity = impulse;
        private void FixedUpdate() => body.linearVelocity = Vector2.MoveTowards(body.linearVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        private void OnDestroy() { if (health != null) health.KnockbackRequested -= Apply; }
    }
}
