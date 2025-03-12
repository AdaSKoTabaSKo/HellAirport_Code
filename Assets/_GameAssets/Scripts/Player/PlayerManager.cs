using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.Managers;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace _GameAssets.Scripts.Player
{
    public class PlayerManager : SingleMonoBehaviour<PlayerManager>
    {
        [Header("Components")]
        public Animator playerAnimator;
        [SerializeField] private PlayerMovementSystem movementSystem;
        [SerializeField] private PlayerRefreshObjectSystem itemSystem;
        
        [Header("Cameras")]
        [SerializeField] private CinemachineVirtualCamera playerCamera;
        [SerializeField] private CinemachineVirtualCamera playerCameraRight;
        [SerializeField] private CinemachineVirtualCamera playerCameraLeft;
        
        
        protected override void Awake()
        {
            base.Awake();
            playerCamera.MoveToTopOfPrioritySubqueue();
            GameManager.OnSceneFinishLoading += ()=>ResetPlayerPositionOnSceneLoading().Forget();
        }

        private async UniTask ResetPlayerPositionOnSceneLoading()
        {
            playerAnimator.SetTrigger("Reset");
            playerCamera.gameObject.SetActive(false);
            await UniTask.Yield();
            playerCamera.gameObject.SetActive(true);
            movementSystem.UnlockPlayerMovement();
        }

        public void FocusToNormalPlayerCamera()
        {
            movementSystem.UnlockPlayerMovement();
            playerCamera.MoveToTopOfPrioritySubqueue();
        }

        public void ChangeRightCameraPriority(int prioNumber) => playerCameraRight.m_Priority = prioNumber;
        public void ChangeLeftCameraPriority(int prioNumber) => playerCameraLeft.m_Priority = prioNumber;

        public void FocusToCameraAndLockPlayer(CinemachineVirtualCamera camToFocus)
        {
            movementSystem.LockPlayerMovement();
            camToFocus.MoveToTopOfPrioritySubqueue();
        }
        
        public bool playerMoving => movementSystem.joystick.Direction != Vector2.zero;

        public void UnlockPlayerMovement() => movementSystem.UnlockPlayerMovement();
        public void LockPlayerMovement() => movementSystem.LockPlayerMovement();

        public void ForcePlayerLookAtOnPosition(Vector3 pos) => movementSystem.EnableLookAtInteraction(pos);
        public void UnFocusPlayerLookAtOnPosition() => movementSystem.DisableLookAtInteraction();

        public void EnableAnimationBool(string boolName) => playerAnimator.SetBool(boolName, true);
        public void DisableAnimationBool(string boolName) => playerAnimator.SetBool(boolName, false);
        
        public PlayerRefreshObjectSystem GivePlayerRefreshSystem() => itemSystem;
    }
}