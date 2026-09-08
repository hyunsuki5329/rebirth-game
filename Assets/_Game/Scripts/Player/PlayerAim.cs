using UnityEngine;
namespace Roguelite
{
    [DefaultExecutionOrder(-80)]
    public sealed class PlayerAim : MonoBehaviour
    {
        public Transform indicator;
        private PlayerInputReader input;
        public Vector2 Direction { get; private set; } = Vector2.right;
        private void Awake() => input = GetComponent<PlayerInputReader>();
        private void Update()
        {
            var camera = Camera.main;
            if (camera == null) return;
            var ray = camera.ScreenPointToRay(input.AimScreen);
            if (new Plane(Vector3.forward, Vector3.zero).Raycast(ray, out var distance))
            {
                Vector2 offset = ray.GetPoint(distance) - transform.position;
                if (offset.sqrMagnitude > 0.001f) Direction = offset.normalized;
            }
            if (indicator != null)
            {
                indicator.position = transform.position + (Vector3)(Direction * 0.7f);
                indicator.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg);
            }
        }
    }
}
