using System.Collections.Generic;
using UnityEngine;
namespace Roguelite
{
    public sealed class CombatEffects : MonoBehaviour
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        public void Track(GameObject effect)
        {
            objects.RemoveAll(item => item == null);
            objects.Add(effect);
        }
        public void Clear()
        {
            foreach (var effect in objects)
                if (effect != null) { effect.SetActive(false); Destroy(effect); }
            objects.Clear();
        }
        private void OnDisable() => Clear();
    }
}
