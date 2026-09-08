using UnityEngine;
using UnityEngine.InputSystem;
namespace GetThisRock
{
    [DefaultExecutionOrder(-100)]
    public sealed class PlayerInput : MonoBehaviour
    {
        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool InteractPressed { get; private set; }
        public bool DropPressed { get; private set; }
        public bool ThrowPressed { get; private set; }
        public bool PushHeld { get; private set; }
        public bool PushPressed { get; private set; }
        public bool ConfirmPressed { get; private set; }
        public bool TabletPressed { get; private set; }
        public bool InventoryPressed { get; private set; }
        public int ToolSlot { get; private set; }
        public bool UIBlocked { get; private set; }
        bool captureRequested;
        public bool Captured => !UIBlocked && captureRequested;
        void Update()
        {
            var k = Keyboard.current; var m = Mouse.current;
            Move = Look = Vector2.zero;
            JumpPressed = InteractPressed = DropPressed = ThrowPressed = PushHeld = PushPressed = ConfirmPressed = TabletPressed = InventoryPressed = false;
            ToolSlot = -2;
            if (k == null) return;
            TabletPressed = k.tabKey.wasPressedThisFrame || (UIBlocked && k.escapeKey.wasPressedThisFrame);
            InventoryPressed = k.iKey.wasPressedThisFrame;
            // Enter also works with the tablet open; the contract validates the physical delivery.
            ConfirmPressed = k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame;
            if (UIBlocked || TabletPressed || InventoryPressed) return;
            if (k.escapeKey.wasPressedThisFrame) { Capture(false); return; }
            if (!Captured && m != null && m.leftButton.wasPressedThisFrame) { Capture(true); return; }
            if (!Captured) return;
            Move = Vector2.ClampMagnitude(new Vector2((k.dKey.isPressed ? 1 : 0) - (k.aKey.isPressed ? 1 : 0), (k.wKey.isPressed ? 1 : 0) - (k.sKey.isPressed ? 1 : 0)), 1);
            if (m != null) { Look = m.delta.ReadValue(); PushHeld = m.leftButton.isPressed; PushPressed = m.leftButton.wasPressedThisFrame; }
            JumpPressed = k.spaceKey.wasPressedThisFrame;
            InteractPressed = k.eKey.wasPressedThisFrame;
            DropPressed = k.gKey.wasPressedThisFrame;
            ThrowPressed = k.qKey.wasPressedThisFrame;
            if (k.digit0Key.wasPressedThisFrame || k.fKey.wasPressedThisFrame) ToolSlot = -1;
            if (k.digit1Key.wasPressedThisFrame) ToolSlot = 0;
            if (k.digit2Key.wasPressedThisFrame) ToolSlot = 1;
            if (k.digit3Key.wasPressedThisFrame) ToolSlot = 2;
            if (k.digit4Key.wasPressedThisFrame) ToolSlot = 3;
            if (k.digit5Key.wasPressedThisFrame) ToolSlot = 4;
            if (k.digit6Key.wasPressedThisFrame) ToolSlot = 5;
            if (k.digit7Key.wasPressedThisFrame) ToolSlot = 6;
            if (k.digit8Key.wasPressedThisFrame) ToolSlot = 7;
        }
        public void SetUIBlocked(bool blocked)
        {
            UIBlocked = blocked; Move = Look = Vector2.zero;
            JumpPressed = InteractPressed = DropPressed = ThrowPressed = PushHeld = PushPressed = false;
            Capture(!blocked);
        }
        void Capture(bool value) { captureRequested = value; Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None; Cursor.visible = !value; }
        void OnApplicationFocus(bool focus) { if (!focus) Capture(false); }
        void OnDisable() => Capture(false);
    }
}
