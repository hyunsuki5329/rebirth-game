using UnityEngine;
namespace Roguelite
{
    public sealed class Hurtbox : MonoBehaviour
    {
        public HealthBody health;
        private void Awake() { if (health == null) health = GetComponentInParent<HealthBody>(); }
    }
}
