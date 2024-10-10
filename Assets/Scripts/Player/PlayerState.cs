using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Heaton.Player
{
    public static class PlayerState
    {
        public enum MovementState
        {
            Idle,
            Running,
            Sprinting,
            Jumping,
            Falling,
        }

        public static UnityAction<MovementState> OnMovementStateChanged;

        public static MovementState Current { get; private set; } = MovementState.Idle;

        public static void SetState(MovementState state)
        {
            if (Current == state)
                return;

            Current = state;

            OnMovementStateChanged?.Invoke(Current);

            Debug.Log($"New Player State {state}");
        }

        public static bool InGroundedState => Current == MovementState.Idle || Current == MovementState.Running || Current == MovementState.Sprinting;
    }
}
