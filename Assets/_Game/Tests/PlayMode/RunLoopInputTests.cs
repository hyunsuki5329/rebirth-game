using System.Collections;
using System.Linq;
using NUnit.Framework;
using Roguelite.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Roguelite.Tests
{
    public class RunLoopInputTests
    {
        private Mouse mouse;
        private InputSettings.BackgroundBehavior background;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode editorBehavior;
#endif
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            background = InputSystem.settings.backgroundBehavior; InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            editorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            mouse = InputSystem.AddDevice<Mouse>();
            yield return SaveSessionFixture.Load();
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            InputSystem.RemoveDevice(mouse); InputSystem.settings.backgroundBehavior = background;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = editorBehavior;
#endif
            foreach (var session in Object.FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None)) Object.Destroy(session.gameObject);
            yield return null; SaveSessionFixture.Cleanup();
        }
        private IEnumerator Click(string name)
        {
            Assert.That(Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Length, Is.EqualTo(1), "UI clicks require one active EventSystem.");
            Canvas.ForceUpdateCanvases();
            var button = Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b => b.name == name);
            var rect = button.GetComponent<RectTransform>(); var point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
            var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = point }, hits);
            TestContext.WriteLine($"UI_CLICK {name} point={point} screen={Screen.width}x{Screen.height} active={button.isActiveAndEnabled} interactable={button.interactable} hits={string.Join(",", hits.Select(h => h.gameObject.name))}");
            InputSystem.QueueStateEvent(mouse, new MouseState { position = point }); yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = point, buttons = 1 }); yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = point }); yield return null; yield return null;
        }
        private IEnumerator Loaded(GameBootstrap session)
        {
            float deadline = Time.realtimeSinceStartup + 10;
            while (session.IsLoading && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(session.IsLoading, Is.False); yield return null;
        }
        [UnityTest]
        public IEnumerator MouseChangesVolumeAndPersistsSetting()
        {
            var session = Object.FindFirstObjectByType<GameBootstrap>();
            yield return Click("Vol -");
            Assert.That(session.Profile.settings.masterVolume, Is.EqualTo(0.9f).Within(0.001f));
            Assert.That(AudioListener.volume, Is.EqualTo(0.9f).Within(0.001f));
            var data = JsonUtility.FromJson<SaveData>(System.IO.File.ReadAllText(session.SavePath));
            Assert.That(data.profile.settings.masterVolume, Is.EqualTo(0.9f).Within(0.001f));
            Assert.That(Object.FindObjectsByType<Text>(FindObjectsSortMode.None).Any(t => t.text.Contains("Runs do not resume")), Is.True);
        }
        [UnityTest]
        public IEnumerator MouseNavigatesMenuWeaponRewardAndResultButtons()
        {
            var session = Object.FindFirstObjectByType<GameBootstrap>();
            yield return Click("Enter Hub"); yield return Loaded(session); Assert.That(session.Flow.Phase, Is.EqualTo(RunFlowPhase.Hub));
            yield return Click("Bow"); Assert.That(session.SelectedWeapon, Is.EqualTo(3));
            yield return Click("Enter Dungeon"); yield return Loaded(session); Assert.That(session.Flow.Phase, Is.EqualTo(RunFlowPhase.Dungeon));
            var dungeon = session.Dungeon;
            Assert.That(dungeon.Player.GetComponent<WeaponController>().Current.id, Is.EqualTo("bow"));
            Assert.That(Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None).Any(p => p.name == "Player arrow"), Is.False);
            while (dungeon.Run.RoomIndex != 2)
            {
                foreach (var enemy in dungeon.Room.Enemies) enemy.Receive(10000, Vector2.zero);
                yield return null; dungeon.Advance(); yield return null;
            }
            Assert.That(dungeon.PendingRewards.Length, Is.GreaterThan(0));
            yield return Click("Reward"); Assert.That(dungeon.PendingRewards, Is.Empty);
            int gold = dungeon.Run.Gold; dungeon.Player.Receive(10000, Vector2.zero); yield return null;
            Assert.That(session.Flow.Phase, Is.EqualTo(RunFlowPhase.Result));
            yield return Click("Return to Hub"); yield return Loaded(session);
            Assert.That(session.Flow.Phase, Is.EqualTo(RunFlowPhase.Hub)); Assert.That(session.Profile.bankGold, Is.EqualTo(gold / 2));
        }
    }
}
