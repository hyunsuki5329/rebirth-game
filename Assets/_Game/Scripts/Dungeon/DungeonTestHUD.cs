using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
namespace Roguelite
{
    public sealed class DungeonTestHUD : MonoBehaviour
    {
        private DungeonManager dungeon;
        private Text status;
        private void Start()
        {
            dungeon = GetComponent<DungeonManager>();
            var canvasObject = new GameObject("Dungeon test UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform);
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var events = new GameObject("UI events", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(transform);
            var module = events.GetComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions(); module.submit = null; module.cancel = null; module.move = null;
            status = MakeText(canvasObject.transform, new Vector2(15, -15), new Vector2(650, 90));
            var buttonObject = new GameObject("New run", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(canvasObject.transform, false);
            Place(buttonObject.GetComponent<RectTransform>(), new Vector2(15, -110), new Vector2(150, 35));
            var button = buttonObject.GetComponent<Button>();
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() => dungeon.BeginRun(dungeon.initialSeed));
            var label = MakeText(buttonObject.transform, Vector2.zero, new Vector2(150, 35));
            label.text = "New Run"; label.color = Color.black; label.alignment = TextAnchor.MiddleCenter;
        }
        private void Update()
        {
            if (status == null || dungeon.Run == null) return;
            status.text = $"M4 Dungeon test | Room {dungeon.Run.RoomIndex + 1} | Gold {dungeon.Run.Gold}\n" +
                (dungeon.Player.IsDead ? "Defeated. Click New Run." : dungeon.PathComplete ? "Boss entrance reached (combat follows in M8)." :
                dungeon.Room.IsClear ? "Room clear. Walk into the green exit on the right." : "Defeat all enemies to unlock the exit.");
        }
        private static Text MakeText(Transform parent, Vector2 position, Vector2 size)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            Place(go.GetComponent<RectTransform>(), position, size);
            var text = go.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18; text.color = Color.white; text.raycastTarget = false; return text;
        }
        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position; rect.sizeDelta = size;
        }
    }
}
