using UnityEngine;
namespace Roguelite
{
    public sealed class EnemyHealth : HealthBody
    {
        public EnemyData data;
        protected override void Awake()
        {
            team = 1; maximumHealth = data.health;
            base.Awake();
            KnockbackRequested += impulse => GetComponent<EnemyMotor2D>().Knockback(impulse);
            Died += () => Destroy(gameObject, 0.15f);
        }
    }
}
