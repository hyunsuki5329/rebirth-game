using UnityEngine;
namespace Roguelite
{
    public sealed class ShortcutController : MonoBehaviour
    {
        public RoomExit dangerousExit;
        private DungeonManager dungeon;
        public void Bind(DungeonManager owner)
        {
            dungeon = owner; dangerousExit.SetOpen(true); dangerousExit.Entered += ChooseDanger;
        }
        private void ChooseDanger()
        {
            if (dungeon != null && dungeon.Room == GetComponent<RoomController>()) dungeon.Advance(true);
        }
        private void OnDisable() { if (dangerousExit != null) dangerousExit.Entered -= ChooseDanger; }
    }
}
