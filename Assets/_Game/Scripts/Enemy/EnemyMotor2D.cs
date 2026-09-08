using UnityEngine;
namespace Roguelite
{
    public sealed class EnemyMotor2D : MonoBehaviour
    {
        private Rigidbody2D body;
        private HealthBody health;
        private Vector2 impulse;
        public Vector2 DesiredVelocity { get; set; }
        private void Awake() { body = GetComponent<Rigidbody2D>(); health = GetComponent<HealthBody>(); }
        public void Knockback(Vector2 value) => impulse = value;
        private void FixedUpdate()
        {
            body.linearVelocity = health.IsDead ? Vector2.zero : DesiredVelocity + impulse;
            impulse = Vector2.MoveTowards(impulse, Vector2.zero, 20 * Time.fixedDeltaTime);
        }
        private void OnDisable() { if (body != null) body.linearVelocity = Vector2.zero; }
    }
}
