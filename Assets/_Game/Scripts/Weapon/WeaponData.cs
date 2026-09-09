using UnityEngine;
namespace Roguelite
{
    [CreateAssetMenu(menuName = "Roguelite/Weapon")]
    public sealed class WeaponData : ScriptableObject
    {
        public string id;
        public Sprite heldSprite;
        public float heldHeight = 1.25f;
        public bool sizeHeldByWidth;
        public AttackDefinition attack;
        public ComboDefinition[] combos;
        public float comboWindowBonus;
        public Color color = Color.cyan;
        public bool canEvolve;
        public string evolutionComboId = "dagger-chain";
        public int evolutionCombos = 3;
        public float waveDamage = 18, waveSpeed = 10, waveLifetime = 0.8f;
        public Sprite waveSprite;
    }
}
