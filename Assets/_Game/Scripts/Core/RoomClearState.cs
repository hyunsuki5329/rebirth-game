using System;
using System.Collections.Generic;
namespace Roguelite.Core
{
    public sealed class RoomClearState
    {
        private readonly HashSet<int> remaining;
        public bool IsClear => remaining.Count == 0;
        public event Action Cleared;
        public RoomClearState(IEnumerable<int> enemies) { remaining = new HashSet<int>(enemies); }
        public bool Defeat(int enemy)
        {
            if (!remaining.Remove(enemy)) return false;
            if (IsClear) Cleared?.Invoke();
            return true;
        }
    }
}
