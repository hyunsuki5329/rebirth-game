using UnityEngine;
namespace Roguelite
{
    public sealed class PlayerHealth : HealthBody
    {
        public PlayerStats stats;
        protected override void Awake()
        {
            team = 0;
            maximumHealth = stats.maximumHealth;
            base.Awake();
            KnockbackRequested += impulse => GetComponent<PlayerMotor2D>().Knockback(impulse);
            Died += () =>
            {
                GetComponent<PlayerMotor2D>().CanMove = false;
                GetComponent<PlayerInputReader>().SetBlocked(true);
            };
        }
    }
}
