using UnityEngine;
namespace Roguelite
{
    public sealed class GameSceneEntry : MonoBehaviour
    {
        public GameBootstrap bootstrapPrefab;
        private void Start()
        {
            if (FindFirstObjectByType<GameBootstrap>() == null) Instantiate(bootstrapPrefab);
        }
    }
}
