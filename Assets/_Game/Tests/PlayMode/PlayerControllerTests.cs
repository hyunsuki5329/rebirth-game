using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Roguelite.Tests
{
    public class PlayerControllerTests
    {
        private Keyboard keyboard;
        private Mouse mouse;
        private InputSettings.BackgroundBehavior oldBackground;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode oldEditorBehavior;
#endif
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            oldBackground = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            oldEditorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            keyboard = InputSystem.AddDevice<Keyboard>();
            mouse = InputSystem.AddDevice<Mouse>();
            yield return SceneManager.LoadSceneAsync("PlayerArena");
            // The batch Editor is unfocused. Model a focused game window for virtual-device tests.
            GameObject.Find("Player").SendMessage("OnApplicationFocus", true);
            InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(Screen.width, Screen.height / 2f) });
            yield return null;
        }
        [TearDown]
        public void TearDown()
        {
            InputSystem.RemoveDevice(keyboard);
            InputSystem.RemoveDevice(mouse);
            InputSystem.settings.backgroundBehavior = oldBackground;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = oldEditorBehavior;
#endif
        }
        private void Keys(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));

        [UnityTest]
        public IEnumerator EqualTimeAxisAndDiagonalTravelHaveEqualDistance()
        {
            var body = GameObject.Find("Player").GetComponent<Rigidbody2D>();
            Keys(Key.D);
            yield return null;
            yield return new WaitForFixedUpdate();
            var start = body.position;
            for (int i = 0; i < 10; i++) yield return new WaitForFixedUpdate();
            var axis = Vector2.Distance(start, body.position);
            body.position = Vector2.zero;
            Keys(Key.D, Key.W);
            yield return null;
            yield return new WaitForFixedUpdate();
            start = body.position;
            for (int i = 0; i < 10; i++) yield return new WaitForFixedUpdate();
            var diagonal = Vector2.Distance(start, body.position);
            Assert.That(axis, Is.EqualTo(1).Within(0.11f));
            Assert.That(diagonal, Is.EqualTo(axis).Within(0.11f));
        }

        private float measuredDash;
        private IEnumerator MeasureDash(bool sprint)
        {
            var body = GameObject.Find("Player").GetComponent<Rigidbody2D>();
            body.position = Vector2.zero;
            if (sprint) Keys(Key.D, Key.LeftShift, Key.Space); else Keys(Key.D, Key.Space);
            yield return new WaitForSeconds(0.04f);
            Keys();
            yield return new WaitForSeconds(0.25f);
            measuredDash = body.position.x;
        }

        [UnityTest]
        public IEnumerator WalkingAndSprintingDashCoverTheSameDistance()
        {
            yield return MeasureDash(false);
            var walkingDash = measuredDash;
            yield return new WaitForSeconds(1);
            yield return MeasureDash(true);
            // One fixed step at dash speed is 0.375 units; allow one boundary step.
            Assert.That(walkingDash, Is.EqualTo(3).Within(0.4f));
            Assert.That(measuredDash, Is.EqualTo(walkingDash).Within(0.4f));
        }

        [UnityTest]
        public IEnumerator ActualWasdAndShiftHoldReachTheSingleMotor()
        {
            var player = GameObject.Find("Player");
            var body = player.GetComponent<Rigidbody2D>();
            Keys(Key.D, Key.W);
            yield return new WaitForSeconds(0.1f);
            Assert.That(body.linearVelocity.magnitude, Is.EqualTo(5).Within(0.05f));
            Keys(Key.D, Key.LeftShift);
            yield return new WaitForSeconds(0.1f);
            Assert.That(body.linearVelocity.x, Is.EqualTo(7.5f).Within(0.05f));
            Keys(Key.D);
            yield return new WaitForSeconds(0.1f);
            Assert.That(body.linearVelocity.x, Is.EqualTo(5).Within(0.05f));
            player.GetComponent<PlayerInputReader>().SetBlocked(true);
            yield return new WaitForSeconds(0.06f);
            Assert.That(body.linearVelocity.magnitude, Is.LessThan(0.01f));
            Assert.That(player.GetComponent<PlayerInputReader>().SprintHeld, Is.False);
            player.GetComponent<PlayerInputReader>().SetBlocked(false);
            Keys(Key.D, Key.LeftShift);
            yield return new WaitForSeconds(0.06f);
            player.SendMessage("OnApplicationFocus", false);
            yield return new WaitForSeconds(0.06f);
            Assert.That(body.linearVelocity.magnitude, Is.LessThan(0.01f), "Focus loss must release movement.");
        }

        [UnityTest]
        public IEnumerator SprintDoesNotMultiplyDashAndWallStopsIt()
        {
            var player = GameObject.Find("Player");
            var body = player.GetComponent<Rigidbody2D>();
            int starts = 0;
            player.GetComponent<PlayerDash>().Started += () => starts++;
            Keys(Key.D, Key.LeftShift, Key.Space);
            yield return new WaitForSeconds(0.06f);
            Assert.That(body.linearVelocity.x, Is.EqualTo(18.75f).Within(0.1f));
            Keys();
            yield return new WaitForSeconds(0.2f);
            Assert.That(body.position.x, Is.InRange(2.4f, 3.4f));
            Keys(Key.Space);
            yield return new WaitForSeconds(0.04f);
            Assert.That(starts, Is.EqualTo(1));
            Keys();
            yield return new WaitForSeconds(1);
            body.position = new Vector2(6, 0);
            Keys(Key.D, Key.Space);
            yield return new WaitForSeconds(0.3f);
            Assert.That(starts, Is.EqualTo(2));
            Assert.That(body.position.x, Is.LessThan(7));
        }

        [UnityTest]
        public IEnumerator IdleDashUsesMousePositionAimAndAttackIsLeftClick()
        {
            var player = GameObject.Find("Player");
            InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(Screen.width / 2f, Screen.height) });
            yield return null;
            Keys(Key.Space);
            yield return new WaitForSeconds(0.06f);
            Assert.That(player.GetComponent<Rigidbody2D>().linearVelocity.y, Is.GreaterThan(15));
            InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(Screen.width / 2f, Screen.height), buttons = 1 });
            yield return null;
            Assert.That(player.GetComponent<PlayerInputReader>().AttackPressed, Is.True);
        }

        [UnityTest]
        public IEnumerator LeftMouseClickReachesCombatAndDamagesTarget()
        {
            yield return SceneManager.LoadSceneAsync("CombatArena");
            GameObject.Find("Player").SendMessage("OnApplicationFocus", true);
            InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(Screen.width, Screen.height / 2f), buttons = 1 });
            yield return new WaitForSeconds(0.16f);
            Assert.That(GameObject.Find("TrainingDummy").GetComponent<HealthBody>().Current, Is.EqualTo(28));
        }
    }
}
