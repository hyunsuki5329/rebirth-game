using UnityEngine;
namespace Roguelite
{
    public sealed class ChargerAI : EnemyAI
    {
        protected override void Tick(Vector2 delta)
        {
            if (attack.IsActive) motor.DesiredVelocity = attack.Direction * data.chargeSpeed;
            else if (attack.IsBusy) motor.DesiredVelocity = Vector2.zero;
            else
            {
                motor.DesiredVelocity = delta.normalized * data.moveSpeed;
                if (delta.magnitude <= data.activationDistance) { motor.DesiredVelocity = Vector2.zero; attack.TryBegin(delta.normalized); }
            }
        }
    }
}
