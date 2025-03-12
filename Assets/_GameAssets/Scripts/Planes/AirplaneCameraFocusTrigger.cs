using System;
using _GameAssets.Scripts.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace _GameAssets.Scripts.Planes
{
    public class AirplaneCameraFocusTrigger : MonoBehaviour
    {
        [SerializeField] private CameraDirection cameraDirection;

        private PlayerManager _player;
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerManager player)) return;
            if (!_player) _player = player;
            
            MoveToCamera();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out PlayerManager player)) return;
            ResetCamera();
        }

        private void ResetCamera()
        {
            if(cameraDirection == CameraDirection.Right) _player.ChangeRightCameraPriority(0);
            else if (cameraDirection == CameraDirection.Left) _player.ChangeLeftCameraPriority(0);
        }

        private void MoveToCamera()
        {
            if(cameraDirection == CameraDirection.Right) _player.ChangeRightCameraPriority(7);
            else if (cameraDirection == CameraDirection.Left) _player.ChangeLeftCameraPriority(7);
        }
    }

    enum CameraDirection
    {
        Right,
        Left
    }
}