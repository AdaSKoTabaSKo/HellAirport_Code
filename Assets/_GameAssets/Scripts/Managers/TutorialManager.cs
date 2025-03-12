using System;
using System.Collections.Generic;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.InteractionObjects;
using _GameAssets.Scripts.Other;
using _GameAssets.Scripts.Player;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace _GameAssets.Scripts.Managers
{
    public class TutorialManager : SingleMonoBehaviour<TutorialManager>
    {
        [SerializeField] private CinemachineVirtualCamera tutorialCamera;
        [SerializeField] private BuyAreaUnlockManager buyAreaUnlockManager;
        [SerializeField] private BuyArea firstPerfumesBuyArea;
        [SerializeField] private BuyArea firstWaitingBuyArea;
        [SerializeField] private GameObject infiniteFingerTutorial;
        
        [Space] [Header("Before Refuelling")]
        [SerializeField] private Transform securityGateCameraPoint;
        
        [Space] [Header("Airplane Fly")]
        [SerializeField] private CinemachineVirtualCamera airplaneFlyCamera;
        
        [Space] [Header("PerfumePickup")]
        [SerializeField] private Transform perfumeStandPoint;
        [SerializeField] private TutorialArrow perfumePickupTutorialArrow;
        [SerializeField] private TutorialArrow perfumeStandPointTutorialArrow;
        
        [Space]
        [SerializeField] private List<TutorialStep> tutorialSteps;

        private bool _tutorialGuestsSpawned;
        private bool _loadingFinished;
        
        public bool AllTutorialDone
        {
            get => GameManager.Save.LoadValue("AllTutorialDone", false);
            set => GameManager.Save.SaveValueAndSync("AllTutorialDone", value);
        }

        private void Start()
        {
            GameManager.OnSceneFinishLoading += () => _loadingFinished = true;
            
            buyAreaUnlockManager.DisableAll();

            if (!AllTutorialDone)
            {
                if(!CheckIfTutorialIsDone(TutorialType.PerfumesKiosk)) firstPerfumesBuyArea.AddXpToAfterBuy(LevelManager.instance.AddXpForKioskBuy, LevelManager.instance.XpValueForKiosk());
                //if(!CheckIfTutorialIsDone(TutorialType.BuyWaitingArea)) firstWaitingBuyArea.AddActionToAfterBuy(LevelManager.instance.AddXpForWaitingAreaBuy);
                ManageTutorial().Forget();
            }
            else buyAreaUnlockManager.EnableSystem(true);
        }

        private async UniTask ManageTutorial()
        {
            
            
            await UniTask.DelayFrame(5);
            
            foreach (var step in tutorialSteps)
            {
                if (step.IsStepDone)
                {
                    step.thingsToDoAtStepFinish?.Invoke();
                    continue;
                }

                SDKManager.TutorialStepStarted(tutorialSteps.IndexOf(step) + 1);
                
                if (step == tutorialSteps[0] || step == tutorialSteps[1] || step == tutorialSteps[2])
                {
                    if (!_tutorialGuestsSpawned)
                    {
                        AirportManager.instance.SpawnDepartureGuestsForTutorial();
                        _tutorialGuestsSpawned = true;
                    }
                }
                
                step.tutorialTextBanner.transform.localScale = Vector3.zero;
                step.thingsToDoAtStepStart?.Invoke();
                //step.tutorialTextBanner.transform.DOScale(Vector3.one * 0.23f, 0.3f).SetEase(Ease.OutBack);

                while (!_loadingFinished)
                {
                    await UniTask.Delay(100);
                }
                
                if (step.tutorialType == TutorialType.BuyWaitingArea)
                {
                    tutorialCamera.m_Follow = securityGateCameraPoint;
                    tutorialCamera.m_Priority = 11;
                    tutorialCamera.gameObject.SetActive(true);
                    PlayerManager.instance.FocusToCameraAndLockPlayer(tutorialCamera);
                    await UniTask.Delay(5000);
                    await securityGateCameraPoint.DOMove(step.cameraPoint.position, 1f);
                }
                
                if (step.cameraPoint)
                {
                    await UniTask.Delay((int)(step.delayBeforeCameraMove * 1000));
                    tutorialCamera.m_Follow = step.cameraPoint;
                    tutorialCamera.m_Priority = 11;
                    tutorialCamera.gameObject.SetActive(true);
                    PlayerManager.instance.FocusToCameraAndLockPlayer(tutorialCamera);
                    await UniTask.Delay(2000);
                    tutorialCamera.m_Priority = 0;
                    PlayerManager.instance.FocusToNormalPlayerCamera();
                }

                if (step == tutorialSteps[0])
                {
                    infiniteFingerTutorial.SetActive(true);
                }

                while (!step.IsStepDone)
                {
                    //step is set as done in places like ticketDesk etc.
                    await UniTask.Delay(500);
                }

                if (step.tutorialType == TutorialType.AirplaneBoarding)
                {
                    airplaneFlyCamera.m_Priority = 11;
                    airplaneFlyCamera.gameObject.SetActive(true);
                    PlayerManager.instance.FocusToCameraAndLockPlayer(airplaneFlyCamera);
                    await UniTask.Delay(8000);
                    airplaneFlyCamera.m_Priority = 0;
                    PlayerManager.instance.FocusToNormalPlayerCamera();
                    airplaneFlyCamera.gameObject.SetActive(false);
                }

                if (step.tutorialType == TutorialType.PerfumesStacking)
                {
                    perfumeStandPointTutorialArrow.HideAndDisableArrow();
                    await UniTask.Delay(500);
                }
                
                //await step.tutorialTextBanner.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
                step.thingsToDoAtStepFinish?.Invoke();
                
                SDKManager.TutorialStepCompleted(tutorialSteps.IndexOf(step) + 1);
            }

            AllTutorialDone = true;
            buyAreaUnlockManager.EnableSystem(false);
            tutorialCamera.gameObject.SetActive(false);
        }
        
        public bool CheckIfTutorialIsDone(TutorialType type)
        {
            foreach (var t in tutorialSteps) if (t.tutorialType == type) return t.IsStepDone;

            Debug.LogError($"There is no tutorial with type: {type}");
            return false;
        }

        public void SetStepAsDone(TutorialType type)
        {
            //Debug.Log($"Tutorial: {type} is done");
            
            foreach (var t in tutorialSteps) if (t.tutorialType == type) t.SetStepAsDone();
        }

        public async UniTask StartMidTutorialAfterPickingUpPerfumes()
        {
            perfumePickupTutorialArrow.HideAndDisableArrow();
            perfumeStandPointTutorialArrow.gameObject.SetActive(true);
            perfumeStandPointTutorialArrow.ShowArrow();

            await UniTask.Delay(500);
            
            tutorialCamera.m_Follow = perfumeStandPoint;
            tutorialCamera.m_Priority = 11;
            tutorialCamera.gameObject.SetActive(true);
            PlayerManager.instance.FocusToCameraAndLockPlayer(tutorialCamera);
            await UniTask.Delay(2000);
            tutorialCamera.m_Priority = 0;
            PlayerManager.instance.FocusToNormalPlayerCamera();
        }
        
        public void SetPerfumeKioskStepAsDone() => SetStepAsDone(TutorialType.PerfumesKiosk);
        public void SetWaitingAreaStepAsDone() => SetStepAsDone(TutorialType.BuyWaitingArea);
    }

    public enum TutorialType
    {
        TicketBuy,
        RefuellingAirplane,
        Baggages,
        AirplaneBoarding,
        PerfumesKiosk,
        PerfumesStacking,
        BuyWaitingArea
    }

    [Serializable]
    internal class TutorialStep
    {
        [Header("Save Name")]
        [SerializeField] private string saveName;
        
        [Header("Mission Type")]
        public TutorialType tutorialType;
        
        [Header("Step Settings")]
        public Transform cameraPoint;
        [ShowIf(nameof(cameraPoint))] public float delayBeforeCameraMove;
        public GameObject tutorialTextBanner;
        [Space] 
        public UnityEvent thingsToDoAtStepStart;
        public UnityEvent thingsToDoAtStepFinish;

        public void SetStepAsDone() => IsStepDone = true;
        
        public bool IsStepDone
        {
            get => GameManager.Save.LoadValue($"IsStepDone_{saveName}", false);
            set => GameManager.Save.SaveValueAndSync($"IsStepDone_{saveName}", value);
        }
    }
}