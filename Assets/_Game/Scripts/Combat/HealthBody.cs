using System;
using Roguelite.Core;
using UnityEngine;
namespace Roguelite
{
    public class HealthBody : MonoBehaviour
    {
        public int team = 1;
        public float maximumHealth = 40;
        public float armor;
        protected HealthState state;
        public float Current => state.Current;
        public float Maximum => state.Maximum;
        public bool IsDead => state.IsDead;
        public event Action<float> Damaged;
        public event Action<Vector2> KnockbackRequested;
        public event Action Died;
        protected virtual void Awake()
        {
            state = new HealthState(maximumHealth);
            state.Died += () => Died?.Invoke();
        }
        public float Receive(float damage, Vector2 impulse)
        {
            var amount = state.Receive(DamageCalculator.Resolve(damage, 1, armor));
            if (amount > 0) { Damaged?.Invoke(amount); KnockbackRequested?.Invoke(impulse); }
            return amount;
        }
    }
}
