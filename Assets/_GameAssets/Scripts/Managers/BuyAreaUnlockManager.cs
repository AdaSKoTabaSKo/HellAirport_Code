using System;
using System.Collections.Generic;
using System.Linq;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.InteractionObjects;
using _GameAssets.Scripts.Planes;
using _GameAssets.Scripts.Player;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _GameAssets.Scripts.Managers
{
    public class BuyAreaUnlockManager : SingleMonoBehaviour<BuyAreaUnlockManager>
    {
        [SerializeField] private Transform cameraHandler;
        [SerializeField] private CinemachineVirtualCamera unlockCamera;
        [SerializeField] private List<AreaStep> areaSteps;
        [SerializeField] private Button showBuyAreasButton;
        
        private int _stepBoughtAreas;
        private int _stepAreasToBuy;
        
        private bool _showingOtherStuff;
        private bool _startedUsingCamera;
        private bool _showingAnythingWithCamera;

        private Area _cachedUpgradeArea;
        
        public int CurrentBuyAreaUnlockStep
        {
            get => GameManager.Save.LoadValue("CurrentBuyAreaUnlockStep", 0);
            set => GameManager.Save.SaveValueAndSync("CurrentBuyAreaUnlockStep", value);
        }

        public void DisableAll()
        {
            foreach (var area in areaSteps.SelectMany(t => t.buyAreasToUnlock))
            {
                if (area.areaType != Area.AreaType.BuyArea) continue;
                if (!area.buyArea.IsItemBought())
                {
                    area.buyArea.Initialize();
                }
                
                area.buyArea.gameObject.SetActive(false);
            }
        }

        public void EnableSystem(bool atStart)
        {
            SDKManager.MiniLevelStarted();
            ManageAreaStep(atStart).Forget();
            
            showBuyAreasButton.onClick.AddListener(ProxyForShowinBuyAreas);
            showBuyAreasButton.gameObject.SetActive(true);
        }

        private void ProxyForShowinBuyAreas() => ShowCurrentBuyAreasWithCamera().Forget();
        private async UniTask ShowCurrentBuyAreasWithCamera()
        {
            if (_startedUsingCamera) return;
            if (IsCameraInUse()) return;
            
            foreach (var area in areaSteps[CurrentBuyAreaUnlockStep].buyAreasToUnlock)
            {
                var itemPos = Vector3.zero;

                if (area.areaType == Area.AreaType.BuyArea)
                {
                    if (!area.buyArea.gameObject.activeInHierarchy) continue;
                    itemPos = area.buyArea.transform.position;
                }
                else
                {
                    if (!area.airplaneSet.UpgradeTriggerObject().activeInHierarchy) continue;
                    itemPos = area.airplaneSet.UpgradeTriggerObject().transform.position;
                }
                
                if (!area.showCamera) continue;
                if (!_startedUsingCamera)
                {
                    _startedUsingCamera = true;
                    cameraHandler.position = PlayerManager.instance.transform.position;
                    unlockCamera.m_Priority = 11;
                    PlayerManager.instance.FocusToCameraAndLockPlayer(unlockCamera);
                    await cameraHandler.DOMove(itemPos, 1f);
                    await UniTask.Delay(1500);
                    continue;
                }
                
                await cameraHandler.DOMove(itemPos, 1f);
                await UniTask.Delay(1000);
                
            }
            
            PlayerManager.instance.FocusToNormalPlayerCamera();
            _startedUsingCamera = false;
            unlockCamera.m_Priority = 0;
        }
        
        public async UniTask ManageAreaStep(bool atStart)
        {
            if(CurrentBuyAreaUnlockStep == 0) SDKManager.BuyAreasStepStarted(CurrentBuyAreaUnlockStep + 1);
            
            var levelManager = LevelManager.instance;
            
            foreach (var area in areaSteps[CurrentBuyAreaUnlockStep].buyAreasToUnlock)
            {
                var itemPos = Vector3.zero;
                
                if (area.countToUnlockNextStep) _stepAreasToBuy++;

                if (area.areaType == Area.AreaType.BuyArea)
                {
                    if(area.countToUnlockNextStep) area.buyArea.AddActionToAfterBuy(()=>RegisterBoughtArea().Forget());
                    area.buyArea.gameObject.SetActive(true);
                    area.buyArea.Initialize();
                    itemPos = area.buyArea.transform.position;
                    
                    if (!area.buyArea.IsItemBought())
                    {
                        if (area.addXp)
                        {
                            if (area.thisIsAirplane) area.buyArea.AddXpToAfterBuy(levelManager.AddXpForAirplaneBuy, levelManager.XpValueForAirplane());
                            else area.buyArea.AddXpToAfterBuy(levelManager.AddXpForKioskBuy, levelManager.XpValueForKiosk());
                        }
                    }

                    if (!area.buyArea.isActiveAndEnabled) continue;
                }
                else
                {
                    _cachedUpgradeArea = area;
                    if(_cachedUpgradeArea.airplaneSet.TryToShowUpgradeTrigger(_cachedUpgradeArea.IsUpgradeBought)) RegisterAirplaneUpgradeBought();
                    itemPos = area.airplaneSet.UpgradeTriggerObject().transform.position;
                }
                
                if(atStart) continue;
                
                if (!area.showCamera) continue;

                if (!_startedUsingCamera)
                {
                    _startedUsingCamera = true;
                    cameraHandler.position = PlayerManager.instance.transform.position;
                    unlockCamera.m_Priority = 11;
                    PlayerManager.instance.FocusToCameraAndLockPlayer(unlockCamera);
                    await cameraHandler.DOMove(itemPos, 1f);
                    await UniTask.Delay(2000);
                    continue;
                }
                
                await cameraHandler.DOMove(itemPos, 1f);
                await UniTask.Delay(2000);
            }
            
            _startedUsingCamera = false;
            unlockCamera.m_Priority = 0;
            PlayerManager.instance.FocusToNormalPlayerCamera();
        }

        public void RegisterAirplaneUpgradeBought()
        {
            _cachedUpgradeArea.IsUpgradeBought = true;
            RegisterBoughtArea().Forget();
        }

        private async UniTask RegisterBoughtArea()
        {
            await UniTask.DelayFrame(10);
            
            _stepBoughtAreas++;

            if (_stepBoughtAreas < _stepAreasToBuy) return;
            showBuyAreasButton.gameObject.SetActive(false);
            
            SDKManager.BuyAreasStepComplete(CurrentBuyAreaUnlockStep + 1);
            CurrentBuyAreaUnlockStep++;
            SDKManager.BuyAreasStepStarted(CurrentBuyAreaUnlockStep + 1);
            _stepBoughtAreas = 0;
            _stepAreasToBuy = 0;

            _showingAnythingWithCamera = true;
            while (LevelManager.instance.IsLevelUpCanvasEnabled()) await UniTask.Delay(500);
            while (_showingOtherStuff) await UniTask.Delay(500);
            
            await ManageAreaStep(false);
            _showingAnythingWithCamera = false;
            showBuyAreasButton.gameObject.SetActive(true);
        }

        public async UniTask ShowExpandBuyArea(BuyArea buyArea)
        {
            _showingOtherStuff = true;
            cameraHandler.position = PlayerManager.instance.transform.position;
            unlockCamera.m_Priority = 11;
            PlayerManager.instance.FocusToCameraAndLockPlayer(unlockCamera);
            await cameraHandler.DOMove(buyArea.transform.position, 1f);
            await UniTask.Delay(1000);
            unlockCamera.m_Priority = 0;
            PlayerManager.instance.FocusToNormalPlayerCamera();
            await UniTask.Delay(2000);
            _showingOtherStuff = false;
        }

        public bool IsCameraInUse() => _showingAnythingWithCamera;
    }
    
    [Serializable]
    internal class AreaStep
    {
        public List<Area> buyAreasToUnlock;
    }

    [Serializable]
    internal class Area
    {
        public AreaType areaType = AreaType.BuyArea;
        
        [ShowIf(nameof(areaType), AreaType.BuyArea)]
        public BuyArea buyArea;
        [ShowIf(nameof(areaType), AreaType.AirplaneUpgradeArea)]
        public Airplane airplaneSet;
        [ShowIf(nameof(areaType), AreaType.AirplaneUpgradeArea)]
        public string saveName;
        
        
        public bool showCamera = true;

        [ShowIf(nameof(areaType), AreaType.BuyArea)]
        public bool countToUnlockNextStep = true;
        
        [ShowIf(nameof(areaType), AreaType.BuyArea)]
        public bool addXp;
        [ShowIf(nameof(addXp))]
        public bool thisIsAirplane;
        
        public enum AreaType
        {
            BuyArea,
            AirplaneUpgradeArea,
        }
        
        public bool IsUpgradeBought
        {
            get => GameManager.Save.LoadValue($"IsUpgradeBought_{saveName}", false);
            set => GameManager.Save.SaveValueAndSync($"IsUpgradeBought_{saveName}", value);
        }
    }
}