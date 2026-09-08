using UnityEngine;
namespace Roguelite
{
    public sealed class CameraFollow : MonoBehaviour
    {
        public Transform target;
        private void LateUpdate()
        {
            if (target == null) return;
            var goal = new Vector3(target.position.x * 0.18f, target.position.y * 0.18f, -10);
            transform.position = Vector3.Lerp(transform.position, goal, 1 - Mathf.Exp(-10 * Time.deltaTime));
        }
    }
}
