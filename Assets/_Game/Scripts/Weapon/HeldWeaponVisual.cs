using UnityEngine;
namespace Roguelite
{
    public sealed class HeldWeaponVisual : MonoBehaviour
    {
        private WeaponController weapons;
        private PlayerAim aim;
        private SpriteRenderer visual;
        private void Awake()
        {
            weapons = GetComponent<WeaponController>(); aim = GetComponent<PlayerAim>();
            var go = new GameObject("Held weapon", typeof(SpriteRenderer)); go.transform.SetParent(transform, false);
            visual = go.GetComponent<SpriteRenderer>(); visual.sortingOrder = 7;
        }
        private void LateUpdate()
        {
            var data = weapons.Current;
            visual.enabled = data != null && data.heldSprite != null && !GetComponent<PlayerHealth>().IsDead;
            if (!visual.enabled) return;
            visual.sprite = data.heldSprite;
            var direction = aim.Direction;
            visual.transform.localPosition = direction * 0.55f;
            visual.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            float extent = data.sizeHeldByWidth ? visual.sprite.bounds.size.x : visual.sprite.bounds.size.y;
            visual.transform.localScale = Vector3.one * (data.heldHeight / extent);
            visual.color = data.canEvolve && weapons.Progress.Evolved ? new Color(0.55f, 1, 1) : Color.white;
        }
    }
}
