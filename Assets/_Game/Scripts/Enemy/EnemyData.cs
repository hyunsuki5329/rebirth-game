using UnityEngine;
namespace Roguelite
{
    public enum EnemyAttackKind { Melee, Projectile, Charge }
    [CreateAssetMenu(menuName = "Roguelite/Enemy")]
    public sealed class EnemyData : ScriptableObject
    {
        public Sprite projectileSprite;
        public string id;
        public float health = 30;
        public float moveSpeed = 2.5f;
        public float preferredDistance = 4;
        public float distanceTolerance = 0.5f;
        public float activationDistance = 7;
        public float chargeSpeed = 10;
        public float projectileSpeed = 5.5f;
        public float projectileLifetime = 3;
        public EnemyAttackKind attackKind;
        public AttackDefinition attack;
    }
}
