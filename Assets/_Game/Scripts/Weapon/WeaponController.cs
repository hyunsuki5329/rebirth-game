using System;
using System.Collections.Generic;
using Roguelite.Core;
using UnityEngine;
namespace Roguelite
{
    public sealed class WeaponController : MonoBehaviour
    {
        public WeaponData[] weapons;
        public bool tintBodyWithWeapon = true;
        public WeaponData Current { get; private set; }
        public WeaponProgress Progress => progress[Current.id];
        public event Action Evolved;
        private readonly Dictionary<string, WeaponProgress> progress = new Dictionary<string, WeaponProgress>();
        private PlayerCombat combat;
        private ComboSystem combo;
        private bool hitSinceCombo;
        private void Start() { if (Current == null) Select(0); }
        private void Awake() { combat = GetComponent<PlayerCombat>(); combo = GetComponent<ComboSystem>(); }
        private void OnEnable() { combat.Hit += OnHit; combo.Completed += OnCombo; }
        private void OnDisable() { combat.Hit -= OnHit; combo.Completed -= OnCombo; hitSinceCombo = false; }
        public bool Select(int index)
        {
            if (index < 0 || index >= weapons.Length || combat.IsAttacking || GetComponent<PlayerHealth>().IsDead) return false;
            GetComponent<CombatEffects>().Clear();
            Current = weapons[index];
            if (!progress.ContainsKey(Current.id)) progress.Add(Current.id, new WeaponProgress(Current.evolutionCombos));
            combat.attack = Current.attack; combo.SetDefinitions(Current.combos, Current.comboWindowBonus);
            hitSinceCombo = false;
            var bodyColor = tintBodyWithWeapon ? Current.color : Color.white;
            GetComponent<SpriteRenderer>().color = bodyColor;
            GetComponent<HitFeedback>().SetBaseColor(bodyColor);
            return true;
        }
        private void OnHit(HealthBody target, float amount) { if (amount > 0) hitSinceCombo = true; }
        private void OnCombo(string id)
        {
            bool qualifies = Current != null && Current.canEvolve && id == Current.evolutionComboId && hitSinceCombo;
            hitSinceCombo = false;
            if (!qualifies) return;
            bool previous = Progress.Evolved;
            if (!Progress.RecordCombo(true)) return;
            if (!previous) Evolved?.Invoke();
            var go = new GameObject("Evolved piercing wave", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D), typeof(PiercingWave));
            go.layer = LayerMask.NameToLayer("Projectile"); go.transform.position = transform.position;
            go.transform.localScale = new Vector3(0.4f, 0.4f, 1);
            go.GetComponent<SpriteRenderer>().sprite = Current.waveSprite;
            go.GetComponent<SpriteRenderer>().color = Color.cyan;
            go.GetComponent<CircleCollider2D>().isTrigger = true;
            go.GetComponent<PiercingWave>().Initialize(GetComponent<PlayerHealth>(), combat.AttackDirection,
                Current.waveSpeed, Current.waveDamage * combat.DamageMultiplier, Current.waveLifetime, combat.ReportHit);
            GetComponent<CombatEffects>().Track(go);
        }
    }
}
