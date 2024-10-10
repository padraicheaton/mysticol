using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace Heaton.Player
{
    public class CameraController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private CameraState[] cameraStates;

        private void OnEnable()
        {
            PlayerState.OnMovementStateChanged += UpdateCameraState;
        }

        private void OnDisable()
        {
            PlayerState.OnMovementStateChanged -= UpdateCameraState;
        }

        private void UpdateCameraState(PlayerState.MovementState newState)
        {
            foreach (CameraState cameraState in cameraStates)
            {
                cameraState.camera.Priority = cameraState.states.Contains(newState) ? 1 : 0;
            }
        }

        [System.Serializable]
        public struct CameraState
        {
            public List<PlayerState.MovementState> states;
            public CinemachineVirtualCamera camera;
        }
    }
}