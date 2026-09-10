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
        private readonly Button[] rewardButtons = new Button[2];
        private void Start()
        {
            dungeon = GetComponent<DungeonManager>();
            var canvasObject = new GameObject("Dungeon test UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform);
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280, 720);
            var events = new GameObject("UI events", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(transform);
            var module = events.GetComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions(); module.submit = null; module.cancel = null; module.move = null;
            status = MakeText(canvasObject.transform, new Vector2(15, -15), new Vector2(1200, 90));
            var controls = MakeText(canvasObject.transform, new Vector2(15, -205), new Vector2(1100, 75));
            controls.text = "WASD Move | Left Shift (hold) Sprint | Space Dash | Mouse Aim | Left Click Attack\n" +
                "Dagger combos: Attack > Attack > Dash > Attack / Dash > Attack > Attack\n" +
                "Land hits during dagger-chain 3 times to evolve. Choose a weapon above; New Run restarts.";
            controls.fontSize = 15;
            var buttonObject = new GameObject("New run", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(canvasObject.transform, false);
            Place(buttonObject.GetComponent<RectTransform>(), new Vector2(15, -110), new Vector2(150, 35));
            var button = buttonObject.GetComponent<Button>();
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() => dungeon.BeginRun(dungeon.initialSeed));
            var label = MakeText(buttonObject.transform, Vector2.zero, new Vector2(150, 35));
            label.text = "New Run"; label.color = Color.black; label.alignment = TextAnchor.MiddleCenter;
            var weaponList = dungeon.playerPrefab.GetComponent<WeaponController>().weapons;
            for (int i = 0; i < weaponList.Length; i++)
            {
                int index = i;
                MakeButton(canvasObject.transform, new Vector2(180 + i * 155, -110), weaponList[i].name,
                    () => dungeon.Player.GetComponent<WeaponController>().Select(index));
            }
            for (int i = 0; i < 2; i++)
            {
                int index = i;
                rewardButtons[i] = MakeButton(canvasObject.transform, new Vector2(15 + i * 210, -155), "Reward", () => dungeon.ChooseReward(index));
                rewardButtons[i].GetComponent<RectTransform>().sizeDelta = new Vector2(200, 35);
                rewardButtons[i].GetComponentInChildren<Text>().rectTransform.sizeDelta = new Vector2(200, 35);
            }
        }
        private void Update()
        {
            if (status == null || dungeon.Run == null) return;
            status.text = $"Dungeon test | HP {dungeon.Player.Current:0}/{dungeon.Player.Maximum:0} | Room {dungeon.Run.RoomIndex + 1} | Gold {dungeon.Run.Gold}\n" +
                (dungeon.Player.IsDead ? "Defeated. Click New Run." : dungeon.Outcome == Roguelite.Core.RunOutcome.Victory ? "Victory! Boss defeated. Click New Run." :
                dungeon.Room.IsClear ? "Room clear. Walk into the green exit on the right." : "Defeat all enemies to unlock the exit.");
            if (dungeon.Player.TryGetComponent<MomentumSystem>(out var momentum))
                status.text += $"\nMomentum {momentum.Model.Value:0} | Gold x{momentum.GoldMultiplier:0.00} | Combo {dungeon.Player.GetComponent<ComboSystem>().LastCompleted ?? "-"}";
            if (dungeon.Player.TryGetComponent<WeaponController>(out var weapon) && weapon.Current != null)
                status.text += $" | {weapon.Current.id}" + (weapon.Current.canEvolve ? $" {(weapon.Progress.Evolved ? "EVOLVED" : weapon.Progress.ComboCount + "/" + weapon.Current.evolutionCombos)}" : "");
            if (dungeon.Run.RoomIndex == 3)
                status.text += "\nChoose: green rectangle = Safe (2 fights) / orange diamond = Dangerous (1 elite + ground blasts).";
            if (dungeon.Boss != null)
            {
                var health = dungeon.Boss.GetComponent<EnemyHealth>();
                status.text += $"\nRIFT WARDEN | {health.Current:0}/{health.Maximum:0} HP | {dungeon.Boss.Phase}";
            }
            for (int i = 0; i < rewardButtons.Length; i++)
            {
                rewardButtons[i].gameObject.SetActive(i < dungeon.PendingRewards.Length);
                if (i < dungeon.PendingRewards.Length) rewardButtons[i].GetComponentInChildren<Text>().text =
                    (dungeon.PendingRewardStacks > 1 ? "Rare " : "Choose ") + dungeon.PendingRewards[i].id + " x" + dungeon.PendingRewardStacks;
            }
        }
        private static Text MakeText(Transform parent, Vector2 position, Vector2 size)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            Place(go.GetComponent<RectTransform>(), position, size);
            var text = go.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18; text.color = Color.white; text.raycastTarget = false; return text;
        }
        private static Button MakeButton(Transform parent, Vector2 position, string title, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(title, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false);
            Place(go.GetComponent<RectTransform>(), position, new Vector2(150, 35));
            var button = go.GetComponent<Button>(); button.navigation = new Navigation { mode = Navigation.Mode.None }; button.onClick.AddListener(action);
            var label = MakeText(go.transform, Vector2.zero, new Vector2(150, 35)); label.text = title; label.color = Color.black; label.alignment = TextAnchor.MiddleCenter;
            return button;
        }
        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position; rect.sizeDelta = size;
        }
    }
}
