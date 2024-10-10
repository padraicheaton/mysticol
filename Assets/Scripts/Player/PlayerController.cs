using System.Collections;
using System.Collections.Generic;
using Heaton.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Heaton.Player
{

    // Reference: https://www.youtube.com/playlist?list=PLYvjPIZvaz-o-DIBhiHzSrrau9HKSmeEz
    public class PlayerController : MonoBehaviour, PlayerControls.ILocomotionActions
    {
        // Private Component References
        private CharacterController _characterController;
        private PlayerAnimation _playerAnimation;

        // Serialised Component References
        [Header("References")]
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private LayerMask groundLayers;

        // Input Containers
        private Vector2 _movementInput;
        private Vector2 _lookInput;
        private bool _sprintToggled;
        private bool _jumpPressed;

        // Private Variables
        private Vector2 _cameraRotation;
        private Vector2 _playerTargetRotation;
        private float _verticalVelocity;
        private float _antiBump; //Force to apply when grounded to stick player to the ground

        // Movement Settings
        [Header("Movement Settings")]
        [SerializeField] private float runAcceleration;
        [SerializeField] private float forwardSpeed;
        [SerializeField] private float sprintSpeed;
        [SerializeField] private float backwardSpeed;
        [SerializeField] private float drag;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float gravity;
        [SerializeField] private float jumpSpeed;

        // Camera Settings
        [Header("Camera Settings")]
        [SerializeField] private float lookSensH;
        [SerializeField] private float lookSensV;
        [SerializeField] private float lookLimitV;

        #region Setup
        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _playerAnimation = GetComponent<PlayerAnimation>();

            _antiBump = sprintSpeed;
        }

        private void OnEnable()
        {
            Provider.Locomotion.SetCallbacks(this);
        }

        private void OnDisable()
        {
            Provider.Locomotion.RemoveCallbacks(this);
        }
        #endregion

        #region Input Events

        public void OnMovement(InputAction.CallbackContext context)
        {
            _movementInput = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            _lookInput = context.ReadValue<Vector2>();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.performed)
                _sprintToggled = !_sprintToggled;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            _jumpPressed = true;
        }

        #endregion

        private void Update()
        {
            HandleDetermineState();

            Vector3 newVelocity = _characterController.velocity;

            HandleGroundedMovement(ref newVelocity);

            HandleVerticalMovement(ref newVelocity);

            HandleDrag(ref newVelocity);

            _characterController.Move(newVelocity * Time.deltaTime);

            _playerAnimation.UpdateAnimationState(_movementInput);
        }

        private void LateUpdate()
        {
            RotateModelToMovementDirection();

            _cameraRotation.x += lookSensH * _lookInput.x;
            _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookSensV * _lookInput.y, -lookLimitV, lookLimitV);

            _playerTargetRotation.x += transform.eulerAngles.x + lookSensH * _lookInput.x;

            cameraTarget.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);

            ResetOneFrameInputs();
        }

        private void HandleVerticalMovement(ref Vector3 newVelocity)
        {
            _verticalVelocity -= gravity * Time.deltaTime;

            if (PlayerState.InGroundedState && _verticalVelocity < 0)
                _verticalVelocity = -_antiBump;

            if (_jumpPressed && PlayerState.InGroundedState)
            {
                _verticalVelocity += Mathf.Sqrt(jumpSpeed * 3f * gravity) + _antiBump;
            }

            newVelocity.y += _verticalVelocity;
        }

        private void HandleGroundedMovement(ref Vector3 newVelocity)
        {
            Vector3 cameraForwardXZ = new Vector3(cameraTarget.transform.forward.x, 0f, cameraTarget.transform.forward.z).normalized;
            Vector3 cameraRightXZ = new Vector3(cameraTarget.transform.right.x, 0f, cameraTarget.transform.right.z).normalized;

            Vector3 movementDirection = cameraRightXZ * _movementInput.x + cameraForwardXZ * _movementInput.y;
            Vector3 movementDelta = movementDirection * runAcceleration * Time.deltaTime;

            float runSpeed = _sprintToggled ? sprintSpeed : forwardSpeed;

            float speed = _movementInput.y >= 0 ? runSpeed : backwardSpeed;

            newVelocity += movementDelta * speed;

            newVelocity = Vector3.ClampMagnitude(new Vector3(newVelocity.x, 0f, newVelocity.z), speed);
        }

        private void HandleDrag(ref Vector3 newVelocity)
        {
            if (newVelocity.magnitude > drag * Time.deltaTime)
            {
                Vector3 currentDrag = newVelocity.normalized * drag * Time.deltaTime;

                newVelocity -= currentDrag;
            }
        }

        private bool IsGrounded()
        {
            return PlayerState.InGroundedState ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();
        }

        // The distinction being made here is to handle slopes, so that the player only leaves the grounded movement states
        // by jumping or falling off of a ledge
        private bool IsGroundedWhileGrounded()
        {
            Vector3 sphereCastPosition = transform.position + Vector3.down * _characterController.radius / 2f;

            return Physics.CheckSphere(sphereCastPosition, _characterController.radius, groundLayers, QueryTriggerInteraction.Ignore);
        }

        private bool IsGroundedWhileAirborne()
        {
            return _characterController.isGrounded;
        }

        private void HandleDetermineState()
        {
            // Jumping if not grounded and moving upwards
            if (!IsGrounded() && _characterController.velocity.y > 0)
            {
                PlayerState.SetState(PlayerState.MovementState.Jumping);
                return;
            }

            // Falling if not grounded and moving downwards
            if (!IsGrounded() && _characterController.velocity.y < 0)
            {
                PlayerState.SetState(PlayerState.MovementState.Falling);
                return;
            }

            // Sprinting if forward movement input and sprint button pressed
            if (_movementInput.y >= 0 && _movementInput.magnitude > 0 && _sprintToggled)
            {
                PlayerState.SetState(PlayerState.MovementState.Sprinting);
                return;
            }

            // Running if movement input
            if (_movementInput.magnitude > 0)
            {
                PlayerState.SetState(PlayerState.MovementState.Running);

                // Reset sprinting state if begun running
                _sprintToggled = false;

                return;
            }

            // Idle after anything else
            PlayerState.SetState(PlayerState.MovementState.Idle);

            // If the player is idle, un-toggle the sprint
            _sprintToggled = false;
        }

        private void RotateModelToMovementDirection()
        {
            // Only do this if the player is moving
            if (_movementInput.magnitude > 0)
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, _playerTargetRotation.x, 0f), Time.deltaTime * rotationSpeed);
        }

        private void ResetOneFrameInputs()
        {
            // Reset this value here, so that jump is only ever true for the frame that it is pressed in
            _jumpPressed = false;
        }

        private void OnDrawGizmos()
        {
            if (_characterController)
            {
                Vector3 sphereCastPosition = transform.position + Vector3.down * _characterController.radius / 2f;
                Gizmos.DrawWireSphere(sphereCastPosition, _characterController.radius);
            }
        }
    }
}