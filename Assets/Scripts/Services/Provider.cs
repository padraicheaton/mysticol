using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Heaton.Input
{
    public static class Provider
    {
        private static PlayerControls _PlayerActions;
        private static PlayerControls PlayerActions => _PlayerActions != null ? _PlayerActions : SetupPlayerControls();

        public static PlayerControls.LocomotionActions Locomotion => PlayerActions.Locomotion;

        private static PlayerControls SetupPlayerControls()
        {
            _PlayerActions = new PlayerControls();
            _PlayerActions.Enable();

            // By default, the player is in locomotion mode
            _PlayerActions.Locomotion.Enable();
            MouseHandler.Lock();

            return _PlayerActions;
        }
    }
}
