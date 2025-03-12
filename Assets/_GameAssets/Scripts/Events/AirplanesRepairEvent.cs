using System;
using System.Collections.Generic;
using System.Linq;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.Currency;
using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Other;
using _GameAssets.Scripts.Planes;
using _GameAssets.Scripts.Player;
using Cinemachine;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GameAssets.Scripts.Events
{
    public class AirplanesRepairEvent : SingleMonoBehaviour<AirplanesRepairEvent>
    {

        [SerializeField] private int onWhatLevelToEnableThisEvent;
        
        [SerializeField] private List<AirplaneRepairEventConfig> eventConfigList;
        [Space]
        [SerializeField] private Interactable mechanicSuitInteractable;
        [SerializeField] private TutorialArrow mechanicTutorialArrow;
        [SerializeField] private CinemachineVirtualCamera mechanicSuitCamera;
        [SerializeField] private GameObject fireManLockerObject;
        
        private List<Airplane> _allAirplanes;
        private AirplaneRepairEventConfig _currentConfig;

        private bool _isPlayerAMechanic;
        private bool _startTimerAddedToAction;
        public bool IsPlayerAMechanic() => _isPlayerAMechanic;
        
        private int CurrentRepairEventIndex
        {
            get => GameManager.Save.LoadValue("CurrentRepairEventIndex", 0);
            set => GameManager.Save.SaveValueAndSync("CurrentRepairEventIndex", value);
        }

        private bool IsTutorialDone
        {
            get => GameManager.Save.LoadValue("RepairEventTutorialIsDone", false);
            set => GameManager.Save.SaveValueAndSync("RepairEventTutorialIsDone", value);
        }

        private void Start()
        {
            _allAirplanes = AirplanesManager.instance.GiveListOfAllAirplanes;
            mechanicSuitInteractable.AddActionToInteract(MakePlayerAMechanic);
            mechanicSuitInteractable.gameObject.SetActive(false);
            mechanicTutorialArrow.gameObject.SetActive(false);
            
            if (LevelManager.instance.CurrentPlayerLevel < onWhatLevelToEnableThisEvent)
            {
                fireManLockerObject.SetActive(false);
                LevelManager.instance.OnLevelUp += StartEventOnCorrectLevel;
                _startTimerAddedToAction = true;
            }
            else
            {
                StartTimer().Forget();
            }
            
        }

        public async UniTask StartTimer()
        {
            if (_startTimerAddedToAction)
            {
                LevelManager.instance.OnLevelUp -= StartEventOnCorrectLevel;
                _startTimerAddedToAction = false;
            }
            
            _currentConfig = eventConfigList[CurrentRepairEventIndex];
            var currentTimer = !IsTutorialDone? 1 : _currentConfig.timerToCrashAnAirplane;
            

            while (currentTimer > 0)
            {
                await UniTask.Delay(1000);
                currentTimer -= 1;
            }

            var activeAirplanes = _allAirplanes.Where(a => a.gameObject.activeInHierarchy).ToList();

            if (activeAirplanes.Count > 0) 
            {
                var randomActiveAirplane = activeAirplanes[Random.Range(0, activeAirplanes.Count)];
                randomActiveAirplane.SetAirplaneAsBroken(_currentConfig.numberOfItemsToRepair);
            }
            else
            {
                Debug.LogError("There is no Airplanes to be broken. Event start failed!!!");
            }
            
        }

        private void StartEventOnCorrectLevel()
        {
            if (onWhatLevelToEnableThisEvent > LevelManager.instance.CurrentPlayerLevel) return;
            StartTimer().Forget();
        }

        public void FinishEvent()
        {
            SpawnMoneyAfterFinishingRepair().Forget();
            
            SDKManager.InGameEventCompleted("AirplaneOnFireEvent");
            
            PlayerSkinManager.instance.EnableClassicLook();
            _isPlayerAMechanic = false;
            CurrentRepairEventIndex++;

            if (CurrentRepairEventIndex > eventConfigList.Count - 1) CurrentRepairEventIndex = 0;

            if (!IsTutorialDone) IsTutorialDone = true;
            
            StartTimer().Forget();
        }

        private async UniTask SpawnMoneyAfterFinishingRepair()
        {
            var moneySystem = MoneyPickupSystem.instance;
            var moneyValue = _currentConfig.howMuchEachMoneyPickupGives;
            await UniTask.Delay(500);

            for (var i = 0; i < _currentConfig.howManyMoneyPickups; i++)
            {
                moneySystem.GiveAvailableMoneyPickup().SpawnMe(moneyValue, PlayerManager.instance.transform.position + Vector3.zero.With(y: 0.5f), true).Forget();
                await UniTask.Delay(10);
            }
        }
        
        public void EnableInteractionWithMechanicSuit()
        {
            mechanicSuitInteractable.gameObject.SetActive(true);
            mechanicTutorialArrow.gameObject.SetActive(true);
            mechanicTutorialArrow.ShowArrow();
        }
        
        private void MakePlayerAMechanic()
        {
            mechanicSuitInteractable.gameObject.SetActive(false);
            mechanicTutorialArrow.HideAndDisableArrow();
            PlayerSkinManager.instance.EnableMechanicSkin();
            _isPlayerAMechanic = true;
        }

        public void TryEnablingFireManLocker()
        {
            if(!IsTutorialDone) fireManLockerObject.SetActive(true);
        }

        public CinemachineVirtualCamera GetMechanicCamera() => mechanicSuitCamera;
    }

    [Serializable]
    internal class AirplaneRepairEventConfig
    {
        public int timerToCrashAnAirplane;
        public int numberOfItemsToRepair;
        public int howManyMoneyPickups;
        public int howMuchEachMoneyPickupGives;
    }
}