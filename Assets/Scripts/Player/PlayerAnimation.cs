using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Heaton.Player
{
    public class PlayerAnimation : MonoBehaviour
    {
        // Serialised References
        [Header("References")]
        [SerializeField] private Animator animator;

        // Animator Parameter References
        private int inputXHash = Animator.StringToHash("InputX");
        private int inputYHash = Animator.StringToHash("InputY");
        private int isSprintingHash = Animator.StringToHash("isSprinting?");
        private int isGroundedHash = Animator.StringToHash("isGrounded?");
        private int isJumpingHash = Animator.StringToHash("isJumping?");
        private int isFallingHash = Animator.StringToHash("isFalling?");

        // Private Variables
        private Vector2 animationInput;

        // Settings
        [Header("Settings")]
        [SerializeField] private float blendSpeed;

        private void OnEnable()
        {
            PlayerState.OnMovementStateChanged += HandleMovementStateChange;
        }

        private void OnDisable()
        {
            PlayerState.OnMovementStateChanged -= HandleMovementStateChange;
        }

        public void UpdateAnimationState(Vector2 playerInput)
        {
            animationInput = Vector2.Lerp(animationInput, playerInput, Time.deltaTime * blendSpeed);

            animator.SetFloat(inputXHash, animationInput.x);
            animator.SetFloat(inputYHash, animationInput.y);
        }

        private void HandleMovementStateChange(PlayerState.MovementState newState)
        {
            animator.SetBool(isSprintingHash, newState == PlayerState.MovementState.Sprinting);
            animator.SetBool(isGroundedHash, PlayerState.InGroundedState);
            animator.SetBool(isJumpingHash, newState == PlayerState.MovementState.Jumping);
            animator.SetBool(isFallingHash, newState == PlayerState.MovementState.Falling);
        }
    }
}