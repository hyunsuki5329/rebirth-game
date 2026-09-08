using UnityEngine;
namespace Roguelite
{
    public sealed class ChaserAI : EnemyAI
    {
        protected override void Tick(Vector2 delta)
        {
            motor.DesiredVelocity = !attack.IsBusy && delta.magnitude > data.attack.range * 0.8f ? delta.normalized * data.moveSpeed : Vector2.zero;
            if (delta.magnitude <= data.attack.range) attack.TryBegin(delta.normalized);
        }
    }
}
