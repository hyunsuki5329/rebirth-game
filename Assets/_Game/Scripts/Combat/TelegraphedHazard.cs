using UnityEngine;
namespace Roguelite
{
    public sealed class TelegraphedHazard : MonoBehaviour
    {
        private HealthBody owner;
        private float impactAt, radius, damage, lifetime;
        private bool fired;
        public bool HasFired => fired;
        public void Initialize(HealthBody source, float range, float warning, float power, float activeTime)
        {
            owner = source; radius = range; damage = power; lifetime = activeTime; impactAt = Time.time + warning;
            Show(new Color(1, 0.45f, 0.05f, 0.32f), warning);
        }
        private void Update()
        {
            if (owner == null || owner.IsDead) { gameObject.SetActive(false); Destroy(gameObject); return; }
            if (fired || Time.time < impactAt) return;
            fired = true;
            var body = gameObject.AddComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic; body.gravityScale = 0;
            gameObject.layer = LayerMask.NameToLayer("Hitbox");
            var collider = gameObject.AddComponent<CircleCollider2D>(); collider.isTrigger = true; collider.radius = radius;
            gameObject.AddComponent<Hitbox>().Initialize(owner, Vector2.right, radius, 360, damage, 0, lifetime, null);
            Show(new Color(1, 0.2f, 0.05f, 0.65f), lifetime);
            Destroy(gameObject, lifetime);
        }
        private void Show(Color color, float duration)
        {
            var go = new GameObject("Ground hazard visual", typeof(AttackVisual)); go.transform.SetParent(transform, false);
            go.GetComponent<AttackVisual>().Initialize(radius, 360, Vector2.right, color, duration);
        }
    }
}
