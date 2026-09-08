using Roguelite.Core;
using UnityEngine;
namespace Roguelite
{
    public sealed class RangedAI : EnemyAI
    {
        protected override void Tick(Vector2 delta)
        {
            int steering = EnemySteering.RangedAxis(delta.magnitude, data.preferredDistance, data.distanceTolerance);
            motor.DesiredVelocity = attack.IsBusy ? Vector2.zero : delta.normalized * data.moveSpeed * steering;
            if (steering == 0 && delta.magnitude <= data.activationDistance && !Physics2D.Raycast(transform.position, delta.normalized, delta.magnitude, LayerMask.GetMask("World")))
                attack.TryBegin(delta.normalized);
        }
    }
}
