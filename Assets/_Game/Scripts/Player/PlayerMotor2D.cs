using UnityEngine;
namespace Roguelite
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMotor2D : MonoBehaviour
    {
        private Rigidbody2D body;
        private PlayerMovement movement;
        private PlayerDash dash;
        private Vector2 impulse;
        public bool CanMove { get; set; } = true;
        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            movement = GetComponent<PlayerMovement>();
            dash = GetComponent<PlayerDash>();
        }
        public void Knockback(Vector2 velocity) => impulse = velocity;
        public void Teleport(Vector2 position)
        {
            impulse = Vector2.zero;
            body.linearVelocity = Vector2.zero;
            body.position = position;
            transform.position = position;
        }
        private void FixedUpdate()
        {
            if (!CanMove) { body.linearVelocity = Vector2.zero; return; }
            body.linearVelocity = dash.IsActive ? dash.Velocity : movement.Velocity + impulse;
            impulse = Vector2.MoveTowards(impulse, Vector2.zero, 30 * Time.fixedDeltaTime);
        }
        private void OnDisable() { if (body != null) body.linearVelocity = Vector2.zero; }
    }
}
