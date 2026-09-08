using UnityEngine;
namespace Roguelite
{
    public sealed class AttackVisual : MonoBehaviour
    {
        private Mesh mesh;
        private Material material;
        private float end;
        public void Initialize(float radius, float arc, Vector2 direction, Color color, float duration)
        {
            const int segments = 24;
            var vertices = new Vector3[segments + 2];
            var triangles = new int[segments * 3];
            for (int i = 0; i <= segments; i++)
            {
                float angle = (-arc / 2 + arc * i / segments) * Mathf.Deg2Rad;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                if (i < segments) { triangles[i * 3] = 0; triangles[i * 3 + 1] = i + 1; triangles[i * 3 + 2] = i + 2; }
            }
            mesh = new Mesh { vertices = vertices, triangles = triangles };
            mesh.RecalculateBounds();
            gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            material = new Material(Shader.Find("Sprites/Default")) { color = color };
            var renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material; renderer.sortingOrder = 5;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            end = Time.time + duration;
        }
        private void Update() { if (Time.time >= end) Destroy(gameObject); }
        private void OnDestroy() { if (mesh != null) Destroy(mesh); if (material != null) Destroy(material); }
    }
}
