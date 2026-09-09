using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Roguelite.Tests
{
    public class BowInputTests
    {
        private Mouse mouse;
        private InputSettings.BackgroundBehavior background;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode editorBehavior;
#endif
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            background = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            editorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            mouse = InputSystem.AddDevice<Mouse>();
            yield return SceneManager.LoadSceneAsync("DungeonArena"); yield return null;
            Object.FindFirstObjectByType<DungeonManager>().Player.SendMessage("OnApplicationFocus", true);
        }
        [TearDown]
        public void TearDown()
        {
            InputSystem.RemoveDevice(mouse); InputSystem.settings.backgroundBehavior = background;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = editorBehavior;
#endif
        }
        [UnityTest]
        public IEnumerator MouseSelectsBowWithoutAttackingThenAimsAndFires()
        {
            var dungeon = Object.FindFirstObjectByType<DungeonManager>();
            foreach (var enemy in dungeon.Room.Enemies) enemy.GetComponent<EnemyAI>().enabled = false;
            var combat = dungeon.Player.GetComponent<PlayerCombat>(); int attacks = 0; combat.AttackStarted += () => attacks++;
            var button = Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b => b.GetComponentInChildren<Text>().text == "Bow");
            Canvas.ForceUpdateCanvases();
            Vector2 buttonPoint = RectTransformUtility.WorldToScreenPoint(null, button.GetComponent<RectTransform>().TransformPoint(button.GetComponent<RectTransform>().rect.center));
            InputSystem.QueueStateEvent(mouse, new MouseState { position = buttonPoint }); yield return null; yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = buttonPoint, buttons = 1 }); yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = buttonPoint }); yield return null; yield return null;
            Assert.That(attacks, Is.Zero, "Selecting a weapon must not also issue an attack.");
            Assert.That(dungeon.Player.GetComponent<WeaponController>().Current.id, Is.EqualTo("bow"));
            var target = dungeon.Room.Enemies[0]; target.transform.position = dungeon.Player.transform.position + Vector3.right * 3;
            Physics2D.SyncTransforms();
            Vector2 aim = Camera.main.WorldToScreenPoint(target.transform.position);
            InputSystem.QueueStateEvent(mouse, new MouseState { position = aim }); yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = aim, buttons = 1 }); yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = aim });
            yield return new WaitForSeconds(0.5f);
            Assert.That(attacks, Is.EqualTo(1));
            Assert.That(target.Current, Is.EqualTo(target.Maximum - 16).Within(0.01));
        }
    }
}
