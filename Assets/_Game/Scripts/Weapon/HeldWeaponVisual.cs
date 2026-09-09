using UnityEngine;
namespace Roguelite
{
    public sealed class HeldWeaponVisual : MonoBehaviour
    {
        public Sprite handSprite;
        public Vector2 handAnchor = new Vector2(0, -0.34f);
        public float reach = 0.43f;
        private WeaponController weapons;
        private PlayerAim aim;
        private PlayerCombat combat;
        private PlayerHealth health;
        private SpriteRenderer body, visual, hand, support;
        private void Awake()
        {
            weapons = GetComponent<WeaponController>(); aim = GetComponent<PlayerAim>();
            combat = GetComponent<PlayerCombat>(); health = GetComponent<PlayerHealth>(); body = GetComponent<SpriteRenderer>();
            visual = Create("Held weapon"); hand = Create("Weapon hand"); support = Create("Bow draw hand");
            hand.sprite = support.sprite = handSprite;
            hand.color = support.color = new Color(0.95f, 0.68f, 0.43f);
            hand.transform.localScale = Vector3.one * 0.15f;
            support.transform.localScale = Vector3.one * 0.13f;
        }
        private SpriteRenderer Create(string label)
        {
            var go = new GameObject(label, typeof(SpriteRenderer)); go.transform.SetParent(transform, false);
            return go.GetComponent<SpriteRenderer>();
        }
        private void LateUpdate()
        {
            var data = weapons.Current;
            bool visible = data != null && data.heldSprite != null && !health.IsDead;
            visual.enabled = visible; hand.enabled = visible && handSprite != null;
            support.enabled = visible && handSprite != null && data.attack.kind == PlayerAttackKind.Projectile;
            if (!visible) return;
            visual.sprite = data.heldSprite; visual.flipX = data.heldFlipX;
            Vector2 direction = combat.IsAttacking ? combat.AttackDirection : aim.Direction;
            bool bow = data.attack.kind == PlayerAttackKind.Projectile;
            float swing = 0, recoil = 0, draw = 0;
            if (combat.IsAttacking)
            {
                var attack = data.attack;
                float total = attack.windup + attack.activeTime + attack.recovery;
                float windup = attack.windup / total;
                float activeEnd = (attack.windup + attack.activeTime) / total;
                float t = combat.AttackNormalizedTime;
                if (bow)
                {
                    draw = t < windup ? Mathf.InverseLerp(0, windup, t) : 0;
                    recoil = t < windup ? 0 : 0.10f * (1 - Mathf.InverseLerp(windup, 1, t));
                }
                else
                {
                    float side = direction.x < 0 ? -1 : 1;
                    swing = side * (t < windup ? Mathf.Lerp(-25, -65, Mathf.InverseLerp(0, windup, t))
                        : t < activeEnd ? Mathf.Lerp(-65, 40, Mathf.InverseLerp(windup, activeEnd, t))
                        : Mathf.Lerp(40, 0, Mathf.InverseLerp(activeEnd, 1, t)));
                }
            }
            var rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + swing);
            Vector3 gripPosition = (Vector3)handAnchor + rotation * Vector3.right * (reach - recoil);
            float extent = data.sizeHeldByWidth ? visual.sprite.bounds.size.x : visual.sprite.bounds.size.y;
            float scale = data.heldHeight / extent;
            Vector2 gripOffset = (Vector2.Scale(data.heldGrip, visual.sprite.rect.size) - visual.sprite.pivot) / visual.sprite.pixelsPerUnit;
            if (data.heldFlipX) gripOffset.x = -gripOffset.x;
            visual.transform.localScale = Vector3.one * scale;
            visual.transform.localRotation = rotation;
            visual.transform.localPosition = gripPosition - rotation * (Vector3)(gripOffset * scale);
            hand.transform.localPosition = gripPosition; hand.transform.localRotation = rotation;
            support.transform.localPosition = gripPosition - (Vector3)direction * (0.25f + draw * 0.16f);
            support.transform.localRotation = rotation;
            // An upward-facing weapon passes behind the body, while hands remain above the grip.
            int order = body.sortingOrder + (direction.y > 0.45f ? -3 : 3);
            visual.sortingOrder = order; hand.sortingOrder = order + 1; support.sortingOrder = order + 1;
            visual.color = data.canEvolve && weapons.Progress.Evolved ? new Color(0.55f, 1, 1) : Color.white;
        }
        private void OnDisable()
        {
            if (visual != null) visual.enabled = false;
            if (hand != null) hand.enabled = false;
            if (support != null) support.enabled = false;
        }
    }
}
