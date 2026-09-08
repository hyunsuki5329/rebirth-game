using UnityEngine;
namespace Roguelite
{
    public sealed class HitFeedback : MonoBehaviour
    {
        private SpriteRenderer sprite;
        private HealthBody health;
        private Color original;
        private float until;
        private void Awake()
        {
            sprite = GetComponent<SpriteRenderer>(); health = GetComponent<HealthBody>();
            original = sprite.color;
            health.Damaged += Flash;
            health.Died += MarkDead;
        }
        private void Flash(float amount) => until = Time.time + 0.12f;
        private void MarkDead() => original = new Color(0.15f, 0.15f, 0.18f);
        private void Update() => sprite.color = Time.time < until ? Color.white : original;
        private void OnDestroy() { if (health != null) { health.Damaged -= Flash; health.Died -= MarkDead; } }
    }
}
