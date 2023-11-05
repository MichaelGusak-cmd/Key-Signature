using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    /// <summary>
    /// Encapsulates the player's gameplay inputs into a few readable properties and events.
    /// </summary>
    public class PlayerGameplayInput : MonoBehaviour
    {
        /// <summary>
        /// Returns the move axis input from the player.
        /// </summary>
        public Vector2 MoveAxis { get; private set; }

        /// <summary>
        /// Returns true if the Jump button is being pressed, false if not.
        /// </summary>
        public bool JumpButton { get; private set; }

        /// <summary>
        /// Invoked when the jump button is pressed.
        /// </summary>
        public event System.EventHandler JumpButtonPressed;

        /// <summary>
        /// Invoked when the jump button is released.
        /// </summary>
        public event System.EventHandler JumpButtonReleased;

        private PlayerInputActions _playerInputActions;
        private InputAction _moveAction;
        private InputAction _jumpAction;

        private void Awake()
        {
            _playerInputActions = new PlayerInputActions();   
        }

        private void OnEnable()
        {
            _moveAction = _playerInputActions.Gameplay.Move;
            _jumpAction = _playerInputActions.Gameplay.Jump;

            _moveAction.Enable();
            _jumpAction.Enable();

            _jumpAction.performed += JumpAction_performed;
            _jumpAction.canceled += JumpAction_canceled;
        }

        private void OnDisable()
        {
            _moveAction.Disable();
            _jumpAction.Disable();
        }

        private void Update()
        {
            MoveAxis = _moveAction.ReadValue<Vector2>();
            JumpButton = _jumpAction.ReadValue<float>() > 0.5f;
        }

        private void JumpAction_performed(InputAction.CallbackContext obj)
        {
            OnJumpButtonPressed();
        }

        private void JumpAction_canceled(InputAction.CallbackContext obj)
        {
            OnJumpButtonReleased();
        }

        protected virtual void OnJumpButtonPressed() =>
            JumpButtonPressed?.Invoke(this, System.EventArgs.Empty);

        protected virtual void OnJumpButtonReleased() =>
            JumpButtonReleased?.Invoke(this, System.EventArgs.Empty);
    }
}