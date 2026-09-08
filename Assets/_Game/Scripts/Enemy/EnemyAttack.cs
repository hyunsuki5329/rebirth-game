using System;
using UnityEngine;
namespace Roguelite
{
    [DefaultExecutionOrder(-20)]
    public sealed class EnemyAttack : MonoBehaviour
    {
        public EnemyData data;
        private HealthBody owner;
        private float impactAt, activeUntil, readyAt;
        private bool pending;
        private GameObject preview;
        public Vector2 Direction { get; private set; }
        public bool IsBusy => Time.time < readyAt;
        public bool IsActive => !pending && Time.time < activeUntil;
        public event Action Fired;
        private void Awake() => owner = GetComponent<HealthBody>();
        public bool TryBegin(Vector2 facing)
        {
            if (!isActiveAndEnabled || owner.IsDead || IsBusy || facing.sqrMagnitude < 0.001f) return false;
            Direction = facing.normalized;
            impactAt = Time.time + data.attack.windup;
            activeUntil = impactAt + data.attack.activeTime;
            readyAt = activeUntil + data.attack.recovery;
            pending = true;
            preview = ShowVisual(new Color(1, 0.3f, 0.15f, 0.2f), data.attack.windup,
                data.attackKind == EnemyAttackKind.Charge ? data.chargeSpeed * data.attack.activeTime : data.attack.range,
                data.attackKind == EnemyAttackKind.Charge ? 18 : data.attack.arc);
            return true;
        }
        private void Update()
        {
            if (owner.IsDead) { Cancel(); return; }
            if (!pending || Time.time < impactAt) return;
            pending = false;
            if (data.attackKind == EnemyAttackKind.Projectile) FireProjectile(); else FireMelee();
            Fired?.Invoke();
        }
        private void FireMelee()
        {
            var go = new GameObject("Enemy attack", typeof(CircleCollider2D), typeof(Hitbox));
            go.layer = LayerMask.NameToLayer("Hitbox");
            go.transform.position = transform.position;
            if (data.attackKind == EnemyAttackKind.Charge) go.transform.SetParent(transform, true);
            go.GetComponent<CircleCollider2D>().isTrigger = true;
            go.GetComponent<CircleCollider2D>().radius = data.attack.range;
            go.GetComponent<Hitbox>().Initialize(owner, Direction, data.attack.range, data.attack.arc, data.attack.damage,
                data.attack.knockback, data.attack.activeTime, null);
            var activeVisual = ShowVisual(new Color(1, 0.4f, 0.2f, 0.45f), data.attack.activeTime, data.attack.range, data.attack.arc);
            if (data.attackKind == EnemyAttackKind.Charge) activeVisual.transform.SetParent(transform, true);
        }
        private void FireProjectile()
        {
            var go = new GameObject("Enemy projectile", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D), typeof(Projectile));
            go.layer = LayerMask.NameToLayer("Projectile");
            go.transform.position = transform.position + (Vector3)Direction * 0.6f;
            go.transform.localScale = new Vector3(0.25f, 0.25f, 1);
            go.GetComponent<SpriteRenderer>().sprite = GetComponent<SpriteRenderer>().sprite;
            go.GetComponent<SpriteRenderer>().color = new Color(1, 0.75f, 0.15f);
            go.GetComponent<CircleCollider2D>().isTrigger = true;
            go.GetComponent<CircleCollider2D>().radius = 0.5f;
            go.GetComponent<Projectile>().Initialize(owner, Direction, data.projectileSpeed, data.attack.damage, data.attack.knockback, data.projectileLifetime);
        }
        private GameObject ShowVisual(Color color, float duration, float range, float arc)
        {
            var go = new GameObject("Enemy telegraph", typeof(AttackVisual));
            go.transform.position = transform.position;
            go.GetComponent<AttackVisual>().Initialize(range, arc, Direction, color, duration);
            return go;
        }
        public void Cancel()
        {
            pending = false; readyAt = activeUntil = 0;
            if (preview != null) Destroy(preview);
        }
        private void OnDisable() => Cancel();
    }
}
