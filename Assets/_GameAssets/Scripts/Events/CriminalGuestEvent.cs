using System;
using System.Collections.Generic;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.Currency;
using _GameAssets.Scripts.Guest;
using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Player;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _GameAssets.Scripts.Events
{
    public class CriminalGuestEvent : SingleMonoBehaviour<CriminalGuestEvent>
    {
        [SerializeField] private List<EventConfig> eventConfigList;
        [SerializeField] private EventConfig tutorialConfig;
        
        
        [Header("Event Points")] 
        [SerializeField] private Transform criminalStartPoint;
        [SerializeField] private Transform criminalFinishPoint;
        [SerializeField] private Transform policeManStartPoint;
        [SerializeField] private Transform policeManCriminalPoint;
        [SerializeField] private Transform policemanEndPoint;
        [SerializeField] private Transform criminalKillPoint;
        [Space] 
        [SerializeField] private Transform itemPlaceOnDesk;
        
        [Header("animations objects")] 
        [SerializeField] private Animator scannerAnimator;
        [SerializeField] private GameObject firstTvScreen;
        [SerializeField] private GameObject secondTvScreen;
        
        [Header("Other Settings")] 
        [SerializeField] private CinemachineVirtualCamera tutorialCamera;
        [SerializeField] private CinemachineVirtualCamera criminalCamera;
        [SerializeField] private Interactable interactablePoint;
        [SerializeField] private GuestSystem policeMan;
        [SerializeField] private Button showCriminalBoyButton;
        
        private GuestSystem _myCurrentCriminal;
        
        private bool _canSpawnCriminal;
        private bool _criminalScanned;
        private bool _timerStarted;
        private bool _criminalBoyCameraIsOn;
        
        private int _currentTimer;
        
        private EventConfig _currentConfig;
        

        private int CurrentTimerSecurityOfficeIndex
        {
            get => GameManager.Save.LoadValue("CurrentTimerSecurityOfficeIndex", 0);
            set => GameManager.Save.SaveValueAndSync("CurrentTimerSecurityOfficeIndex", value);
        }

        private bool IsTutorialDone
        {
            get => GameManager.Save.LoadValue("CriminalEventTutorialIsDone", false);
            set => GameManager.Save.SaveValueAndSync("CriminalEventTutorialIsDone", value);
        }

        private void Start()
        {
            interactablePoint.AddActionToInteract(RegisterCriminalScan);
            interactablePoint.gameObject.SetActive(false);
            showCriminalBoyButton.onClick.AddListener(()=>ShowCriminalBoy().Forget());
        }

        public void ProxyForStartTimer() => StartTimer().Forget();
        
        public async UniTask StartTimer()
        {
            if (_timerStarted) return;
            _timerStarted = true;
            
            _currentConfig = !IsTutorialDone ? tutorialConfig : eventConfigList[CurrentTimerSecurityOfficeIndex];
            _currentTimer = _currentConfig.timerToSpawnCriminal;

            while (_currentTimer > 0)
            {
                await UniTask.Delay(1000);
                _currentTimer -= 1;
            }

            _canSpawnCriminal = true;
            _timerStarted = false;
        }

        public void TryRegisterAsCriminal(GuestSystem guest)
        {
            if (!_canSpawnCriminal) return;
            _canSpawnCriminal = false;
            Debug.LogError("Spawnionko kriminalisty");
            guest.SetAsCriminal();
            _myCurrentCriminal = guest;
        }

        public async UniTask StartShowingCriminalWithCamera()
        {
            if (!IsTutorialDone)
            {
                while (BuyAreaUnlockManager.instance.IsCameraInUse()) await UniTask.Delay(500);
                tutorialCamera.m_Priority = 11;
                tutorialCamera.gameObject.SetActive(true);
                PlayerManager.instance.FocusToCameraAndLockPlayer(tutorialCamera);
                await UniTask.Delay(1000);
            }
            else
            {
                showCriminalBoyButton.gameObject.SetActive(true);
            }
        }
        
        public async UniTask StartCriminalEvent()
        {
            SDKManager.InGameEventStarted("CriminalEvent");

            await _myCurrentCriminal
                .SetNewDestinationWithWaitingAndAction(criminalStartPoint.position,
                    _myCurrentCriminal.EnableEscortTrigger);
            
            if (!IsTutorialDone)
            {
                await UniTask.Delay(1500);
                tutorialCamera.m_Priority = 0;
                PlayerManager.instance.FocusToNormalPlayerCamera();
            }
            
            ContinueCriminalEvent().Forget();

            criminalCamera.m_Follow = _myCurrentCriminal.transform;
            criminalCamera.m_LookAt = _myCurrentCriminal.transform;
            
            //showCriminalBoyButton.gameObject.SetActive(true);
        }

        private async UniTask ContinueCriminalEvent()
        {
            await _myCurrentCriminal.SetNewDestinationWithWaiting(criminalFinishPoint.position, Vector3.left);
            
            _myCurrentCriminal.DisableEscortTrigger();

            interactablePoint.gameObject.SetActive(true);
            
            while (!_criminalScanned)
            {
                await UniTask.Delay(500);
            }

            firstTvScreen.SetActive(true);
            
            scannerAnimator.SetTrigger("Scan");
            await UniTask.Delay(1750);
            
            firstTvScreen.SetActive(false);
            secondTvScreen.SetActive(true);
            //tutaj animacja skanera i zapalanie ekranu
            
            if (!_currentConfig._spawnedItemPrefab)
            {
                var spawnedItem = _currentConfig._spawnedItemPrefab = Instantiate(_currentConfig.itemPrefab, transform);
                spawnedItem.transform.localScale = Vector3.zero;
                spawnedItem.SetActive(false);
            }

            var item = _currentConfig._spawnedItemPrefab;
            
            item.transform.position = _myCurrentCriminal.transform.position;
            item.SetActive(true);
            item.transform.DOScale(1, 0.1f);
            await item.transform.DOJump(itemPlaceOnDesk.position, 1,1,0.4f);

            await UniTask.Delay(1000);
            
            showCriminalBoyButton.gameObject.SetActive(false);
            
            await policeMan.SetNewDestinationWithWaiting(policeManCriminalPoint.position, Vector3.forward);
            await UniTask.Delay(1000);
            _myCurrentCriminal.SetNewDestinationWithWaitingAndKillAtTheEndLol(criminalKillPoint.position).Forget();
            await UniTask.Delay(500);
            
            if(!IsTutorialDone) IsTutorialDone = true;
            if (IsTutorialDone) CurrentTimerSecurityOfficeIndex++;
            if (CurrentTimerSecurityOfficeIndex + 1 > eventConfigList.Count) CurrentTimerSecurityOfficeIndex = 0;
            
            StartTimer().Forget();
            SpawnMoneyFromPoliceman().Forget();
            SDKManager.InGameEventCompleted("CriminalEvent");
            
            await policeMan.SetNewDestinationWithWaiting(policemanEndPoint.position, Vector3.left);
            policeMan.SetNewDestination(policeManStartPoint.position);
            
            
            
            _criminalScanned = false;
            _myCurrentCriminal = null;

            await item.transform.DOScale(0, 0.3f);
            item.SetActive(false);
            secondTvScreen.SetActive(false);
        }

        private async UniTask SpawnMoneyFromPoliceman()
        {
            var moneySystem = MoneyPickupSystem.instance;
            var moneyValue = _currentConfig.howMuchEachMoneyPickupGives;
            await UniTask.Delay(500);

            for (var i = 0; i < _currentConfig.howManyMoneyPickups; i++)
            {
                moneySystem.GiveAvailableMoneyPickup().SpawnMe(moneyValue, policeMan.transform.position + Vector3.zero.With(y: 0.5f)).Forget();
                await UniTask.Delay(10);
            }
        }
        
        private void RegisterCriminalScan()
        {
            _criminalScanned = true;
            interactablePoint.gameObject.SetActive(false);
        }

        public async UniTask ShowCriminalBoy()
        {
            if (_criminalBoyCameraIsOn) return;
            _criminalBoyCameraIsOn = true;
            
            
            while (BuyAreaUnlockManager.instance.IsCameraInUse()) await UniTask.Delay(500);
            
            criminalCamera.m_Priority = 11;
            criminalCamera.gameObject.SetActive(true);
            PlayerManager.instance.FocusToCameraAndLockPlayer(criminalCamera);
            await UniTask.Delay(2000);
            criminalCamera.m_Priority = 0;
            PlayerManager.instance.FocusToNormalPlayerCamera();

            _criminalBoyCameraIsOn = false;
        }
    }

    [Serializable]
    internal class EventConfig
    {
        public int timerToSpawnCriminal;
        public GameObject itemPrefab;
        public int howManyMoneyPickups;
        public int howMuchEachMoneyPickupGives;
        [HideInInspector] public GameObject _spawnedItemPrefab;
    }
}