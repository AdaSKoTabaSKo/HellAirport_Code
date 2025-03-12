using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace _GameAssets.Scripts.VideoUtils
{
    public class MovieCameraSystem : MonoBehaviour
    {
        [System.Serializable]
        public struct CameraKey
        {
            public CinemachineVirtualCamera camera;
            public KeyCode key;
        }
        [SerializeField] private List<CameraKey> cameraKeys;

        void Update()
        {
            foreach (var cameraKey in cameraKeys)
            {
                if (Input.GetKeyDown(cameraKey.key))
                {
                    cameraKey.camera.MoveToTopOfPrioritySubqueue();
                }
            }
        }
    }
}
