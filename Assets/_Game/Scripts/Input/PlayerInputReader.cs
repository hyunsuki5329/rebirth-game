using UnityEngine;
using UnityEngine.InputSystem;

namespace Roguelite
{
    [DefaultExecutionOrder(-100)]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        public InputActionAsset actions;
        private InputActionAsset instance;
        private InputActionMap map;
        private InputAction move, aim, sprint, dash, attack;
        private bool blocked;
        public Vector2 Move { get; private set; }
        public Vector2 AimScreen { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool DashPressed { get; private set; }
        public bool AttackPressed { get; private set; }

        private void Awake()
        {
            if (actions == null) { Debug.LogError("Player input requires an InputActionAsset.", this); enabled = false; return; }
            instance = Instantiate(actions);
            map = instance.FindActionMap("Player", true);
            move = map.FindAction("Move", true);
            aim = map.FindAction("Aim", true);
            sprint = map.FindAction("Sprint", true);
            dash = map.FindAction("Dash", true);
            attack = map.FindAction("Attack", true);
        }
        private void OnEnable() { if (!blocked) map?.Enable(); }
        private void OnDisable() { map?.Disable(); Clear(); }
        private void OnDestroy() { if (instance != null) Destroy(instance); }
        private void OnApplicationFocus(bool focus) { if (!focus) { map?.Disable(); Clear(); } else if (!blocked && enabled) map?.Enable(); }
        public void SetBlocked(bool value)
        {
            blocked = value;
            if (value) { map?.Disable(); Clear(); }
            else if (isActiveAndEnabled) map?.Enable();
        }
        private void Clear() { Move = Vector2.zero; SprintHeld = DashPressed = AttackPressed = false; }
        private void Update()
        {
            if (blocked || map == null || !map.enabled) { Clear(); return; }
            Move = Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1);
            AimScreen = aim.ReadValue<Vector2>();
            SprintHeld = sprint.IsPressed();
            DashPressed = dash.WasPressedThisFrame();
            AttackPressed = attack.WasPressedThisFrame();
        }
    }
}
