using UnityEngine;
namespace Roguelite
{
    public sealed class HitFeedback : MonoBehaviour
    {
        private SpriteRenderer sprite;
        private HealthBody health;
        private Color original;
        private float until;
        private MaterialPropertyBlock properties;
        private void Awake()
        {
            sprite = GetComponent<SpriteRenderer>(); health = GetComponent<HealthBody>();
            original = sprite.color;
            properties = new MaterialPropertyBlock();
            health.Damaged += Flash;
            health.Died += MarkDead;
        }
        private void Flash(float amount) => until = Time.time + 0.12f;
        public void SetBaseColor(Color color) => original = color;
        private void MarkDead() => original = new Color(0.15f, 0.15f, 0.18f);
        private void Update()
        {
            bool flashing = Time.time < until;
            sprite.color = flashing ? Color.white : original;
            sprite.GetPropertyBlock(properties); properties.SetFloat("_HitFlash", flashing ? 1 : 0); sprite.SetPropertyBlock(properties);
        }
        private void OnDestroy() { if (health != null) { health.Damaged -= Flash; health.Died -= MarkDead; } }
    }
}
