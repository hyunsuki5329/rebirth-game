using Roguelite.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
namespace Roguelite
{
    public sealed class GameFlowView : MonoBehaviour
    {
        private GameBootstrap session;
        private GameObject menu, hub, result, hud;
        private Text hubText, resultText, hudText;
        private Button start, continueButton, menuButton;
        private Button[] weapons;
        private readonly Button[] rewards = new Button[2];
        private void Start()
        {
            session = GetComponent<GameBootstrap>();
            var canvas = new GameObject("Game UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.transform.SetParent(transform, false); canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280, 720);
            var events = new GameObject("Game UI events", typeof(EventSystem), typeof(InputSystemUIInputModule)); events.transform.SetParent(transform, false);
            var module = events.GetComponent<InputSystemUIInputModule>(); module.AssignDefaultActions(); module.submit = null; module.move = null; module.cancel = null;
            menu = Panel(canvas.transform, "Main Menu");
            TextAt(menu.transform, "2D ACTION ROGUELITE", 40, 45, 40);
            TextAt(menu.transform, "Vertical Slice", 40, 105, 24);
            menuButton = ButtonAt(menu.transform, "Enter Hub", 40, 170, () => session.OpenHub());
            hub = Panel(canvas.transform, "Hub"); TextAt(hub.transform, "HUB", 40, 35, 36);
            hubText = TextAt(hub.transform, "", 40, 90, 23);
            weapons = new Button[session.config.weapons.Length];
            for (int i = 0; i < weapons.Length; i++)
            {
                int index = i;
                weapons[i] = ButtonAt(hub.transform, session.config.weapons[i].name, 40 + i * 230, 190, () => session.SelectWeapon(index));
            }
            start = ButtonAt(hub.transform, "Enter Dungeon", 40, 275, () => session.StartRun());
            TextAt(hub.transform, "Choose a weapon. Each run starts with fresh HP, items, Momentum and evolution.\nWASD move | Left Shift hold sprint | Space dash | Mouse aim | Left click attack", 40, 350, 20);
            result = Panel(canvas.transform, "Run Result"); TextAt(result.transform, "RUN RESULT", 40, 40, 36);
            resultText = TextAt(result.transform, "", 40, 110, 25);
            continueButton = ButtonAt(result.transform, "Return to Hub", 40, 315, () => session.OpenHub());
            hud = new GameObject("Dungeon HUD", typeof(RectTransform)); hud.transform.SetParent(canvas.transform, false); Stretch(hud.GetComponent<RectTransform>());
            hudText = TextAt(hud.transform, "", 18, 16, 20);
            for (int i = 0; i < rewards.Length; i++)
            {
                int choice = i;
                rewards[i] = ButtonAt(hud.transform, "Reward", 18 + i * 225, 155, () => session.Dungeon.ChooseReward(choice));
            }
        }
        private void Update()
        {
            if (session == null || menu == null) return;
            var phase = session.Flow.Phase;
            menu.SetActive(phase == RunFlowPhase.MainMenu); hub.SetActive(phase == RunFlowPhase.Hub);
            result.SetActive(phase == RunFlowPhase.Result); hud.SetActive(phase == RunFlowPhase.Dungeon);
            menuButton.interactable = start.interactable = continueButton.interactable = !session.IsLoading;
            hubText.text = $"Bank Gold: {session.Profile.bankGold} | Victories: {session.Profile.totalClears}\nSelected: {session.config.weapons[session.SelectedWeapon].name}";
            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].interactable = !session.IsLoading;
                weapons[i].GetComponent<Image>().color = i == session.SelectedWeapon ? new Color(0.4f, 0.9f, 0.8f) : Color.white;
            }
            var summary = session.Flow.Result;
            if (summary != null) resultText.text = $"{summary.Outcome}\nEarned Gold: {summary.EarnedGold}\nRetained: {summary.Retention:P0} = {summary.RetainedGold}\nBank Gold: {session.Profile.bankGold}";
            if (session.Dungeon != null && session.Dungeon.Player != null)
            {
                var dungeon = session.Dungeon; var player = dungeon.Player;
                hudText.text = $"HP {player.Current:0}/{player.Maximum:0} | Gold {dungeon.Run.Gold} | Room {dungeon.Run.RoomIndex + 1}\n" +
                    $"{player.GetComponent<WeaponController>().Current.name} | Momentum {player.GetComponent<MomentumSystem>().Model.Value:0}\n" +
                    (dungeon.Room.IsClear ? "Room clear: use an exit." : "Defeat enemies to unlock the exit.");
                var weapon = player.GetComponent<WeaponController>();
                hudText.text += $"\nCombo: {player.GetComponent<ComboSystem>().LastCompleted ?? "-"}" +
                    (weapon.Current.canEvolve ? (weapon.Progress.Evolved ? " | EVOLVED" : $" | Evolution {weapon.Progress.ComboCount}/{weapon.Current.evolutionCombos}") : "");
                if (dungeon.Boss != null) hudText.text += $"\nRift Warden: {dungeon.Boss.GetComponent<EnemyHealth>().Current:0} HP | {dungeon.Boss.Phase}";
                for (int i = 0; i < rewards.Length; i++)
                {
                    rewards[i].gameObject.SetActive(i < dungeon.PendingRewards.Length);
                    if (i < dungeon.PendingRewards.Length) rewards[i].GetComponentInChildren<Text>().text =
                        dungeon.PendingRewards[i].id + " x" + dungeon.PendingRewardStacks;
                }
            }
        }
        private static GameObject Panel(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false); Stretch(go.GetComponent<RectTransform>());
            go.GetComponent<Image>().color = new Color(0.045f, 0.065f, 0.09f, 0.98f); return go;
        }
        private static void Stretch(RectTransform rect)
        { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        private static Text TextAt(Transform parent, string text, float x, float y, int size)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            Place(go.GetComponent<RectTransform>(), x, y, 1150, 180);
            var label = go.GetComponent<Text>(); label.text = text; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = size; label.color = Color.white; label.raycastTarget = false; return label;
        }
        private static Button ButtonAt(Transform parent, string text, float x, float y, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false);
            Place(go.GetComponent<RectTransform>(), x, y, 210, 48);
            var button = go.GetComponent<Button>(); button.navigation = new Navigation { mode = Navigation.Mode.None }; button.onClick.AddListener(action);
            var label = TextAt(go.transform, text, 0, 0, 22); label.rectTransform.sizeDelta = new Vector2(210, 48); label.color = Color.black; label.alignment = TextAnchor.MiddleCenter;
            return button;
        }
        private static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height);
        }
    }
}
