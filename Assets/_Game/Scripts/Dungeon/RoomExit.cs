using System;
using UnityEngine;
namespace Roguelite
{
    public sealed class RoomExit : MonoBehaviour
    {
        public bool IsOpen { get; private set; }
        public Color openColor = Color.green;
        public event Action Entered;
        public void SetOpen(bool value)
        {
            IsOpen = value;
            GetComponent<SpriteRenderer>().color = value ? openColor : Color.red;
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsOpen && other.GetComponent<PlayerHealth>() is PlayerHealth player && !player.IsDead) Entered?.Invoke();
        }
        private void OnTriggerStay2D(Collider2D other) => OnTriggerEnter2D(other);
    }
}
