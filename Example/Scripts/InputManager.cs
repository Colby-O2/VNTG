using UnityEngine;
using UnityEngine.InputSystem;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    InputManager.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.Example
{
    [RequireComponent(typeof(PlayerInput))]
    internal class InputManager : MonoBehaviour
    {
        [SerializeField] private PlayerInput _input;

        private InputAction _moveAction;
        private InputAction _lookAction;

        public Vector2 RawMovement { get; private set; }
        public Vector2 RawLook { get; private set; }

        private void Awake()
        {
            if (!_input) _input = GetComponent<PlayerInput>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _moveAction = _input.actions["Move"];
            _lookAction = _input.actions["Look"];

            _moveAction.performed += HandleMoveAction;
            _lookAction.performed += HandleLookAction;
        }

        private void OnDestroy()
        {
            _moveAction.performed -= HandleMoveAction;
            _lookAction.performed -= HandleLookAction;
        }

        private void HandleMoveAction(InputAction.CallbackContext e)
        {
            RawMovement = e.ReadValue<Vector2>();
        }

        private void HandleLookAction(InputAction.CallbackContext e)
        {
            RawLook = e.ReadValue<Vector2>();
        }

        public void EnableMovement()
        {
            _moveAction?.Enable();
        }

        public void DisableMovement()
        {
            RawMovement = Vector2.zero;
            _moveAction.Disable();
        }
    }
}