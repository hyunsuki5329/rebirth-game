using UnityEngine;
namespace Roguelite
{
    public abstract class EnemyAI : MonoBehaviour
    {
        public EnemyData data;
        protected EnemyMotor2D motor;
        protected EnemyAttack attack;
        protected EnemyHealth health;
        protected HealthBody target;
        protected virtual void Awake()
        {
            motor = GetComponent<EnemyMotor2D>(); attack = GetComponent<EnemyAttack>(); health = GetComponent<EnemyHealth>();
        }
        public void SetTarget(HealthBody value) => target = value;
        private void Update()
        {
            if (health.IsDead || target == null || target.IsDead) { motor.DesiredVelocity = Vector2.zero; attack.Cancel(); return; }
            Vector2 delta = target.transform.position - transform.position;
            Tick(delta);
        }
        protected abstract void Tick(Vector2 delta);
        protected virtual void OnDisable() { if (motor != null) motor.DesiredVelocity = Vector2.zero; if (attack != null) attack.Cancel(); }
    }
}
