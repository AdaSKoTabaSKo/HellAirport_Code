using System;
using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Other;
using _GameAssets.Scripts.Player;
using Cinemachine;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GameAssets.Scripts.Shipment
{
    public class ShipmentTutorial : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera tutorialCamera;
        [SerializeField] private TutorialArrow arrow1;
        [SerializeField] private TutorialArrow arrow2;
        
        public bool TutorialShown
        {
            get => GameManager.Save.LoadValue($"ShipmentTutorialShown", false);
            private set => GameManager.Save.SaveValueAndSync($"ShipmentTutorialShown", value);
        }
        
        public bool TutorialDone
        {
            get => GameManager.Save.LoadValue($"ShipmentTutorialDone", false);
            private set => GameManager.Save.SaveValueAndSync($"ShipmentTutorialDone", value);
        }

        public void TryToShowTutorial()
        {
            if (!TutorialShown) ShowCamera().Forget();
            if (!TutorialDone) StartTutorial();
        }

        private async UniTask ShowCamera()
        {
            tutorialCamera.m_Priority = 11;
            tutorialCamera.gameObject.SetActive(true);
            PlayerManager.instance.FocusToCameraAndLockPlayer(tutorialCamera);
            await UniTask.Delay(2000);
            tutorialCamera.m_Priority = 0;
            PlayerManager.instance.FocusToNormalPlayerCamera();
            TutorialShown = true;
        }

        private void StartTutorial() => arrow1.gameObject.SetActive(true);

        public void MoveToNextTutorialStep()
        {
            arrow1.HideAndDisableArrow();
            arrow2.gameObject.SetActive(true);
        }

        public void FinishTutorial()
        {
            arrow2.HideAndDisableArrow();
            TutorialDone = true;
        }
    }
}