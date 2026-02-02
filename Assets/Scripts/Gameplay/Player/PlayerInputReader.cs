using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Player
{
    public class PlayerInputReader : MonoBehaviour
    {
        private InputActionMap actionMap;
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction dropThroughAction;

        public Vector2 Move { get; private set; }

        public event System.Action<float> OnJumpPressed;
        private void Awake()
        {
            actionMap = new InputActionMap("Player");

            moveAction = actionMap.AddAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");

            jumpAction = actionMap.AddAction("Jump", InputActionType.Button);
            jumpAction.AddBinding("<Keyboard>/upArrow");

            dropThroughAction = actionMap.AddAction("DropThrough", InputActionType.Button);
            dropThroughAction.AddBinding("<Keyboard>/downArrow");

            moveAction.performed += ctx => Move = ctx.ReadValue<Vector2>();
            moveAction.canceled += ctx => Move = Vector2.zero;
            jumpAction.started += ctx => OnJumpPressed?.Invoke(Time.time);
        }

        public void Enable() => actionMap.Enable();
        public void Disable() => actionMap.Disable();

        private void OnEnable() => Enable();
        private void OnDisable() => Disable();
    }
}