using System;
using System.Collections.Generic;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Player;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _GameAssets.Scripts.Planes
{
    public class AirplaneUpgradeUi : SingleMonoBehaviour<AirplaneUpgradeUi>
    {
        [Header("UWU")]
        [SerializeField] private CanvasGroup upgradeCanvas;
        [SerializeField] private GameObject upgradeButtonsParent;
        [SerializeField] private GameObject waitInfoParent;
        [SerializeField] private Button exitButton;
        
        [Header("Upgrade Buttons")]
        [SerializeField] private TextMeshProUGUI airplaneLevelTmp;
        [SerializeField] private TextMeshProUGUI ticketPriceValueTmp;
        [SerializeField] private TextMeshProUGUI seatsValueTmp;
        
        [Header("Upgrade Buttons")]
        [SerializeField] private AirplaneUpgradeUiButton upgradeButton;
        
        private Airplane _currentPlane;
        private UpgradeType _currentUpgradeType;

        private int _priceForUpgrade;

        private bool _maxLevel;
        private bool _uiIsOpen;

        private AirplanesManager _airplaneManager;
        
        private void Start()
        {
            upgradeCanvas.interactable = false;
            upgradeCanvas.alpha = 0;
            _airplaneManager = AirplanesManager.instance;
            upgradeButton.AssignUpgradeFunction(MakeUpgrade);
            exitButton.onClick.AddListener(()=>HideUpgradeUi().Forget());
        }

        public void ShowUpgradeUi(Airplane p)
        {
            LevelManager.instance.RegisterOtherUiOpen();
            
            _uiIsOpen = true;
            upgradeCanvas.gameObject.SetActive(true);
            _currentPlane = p;
            
            if (_currentPlane.IsPlaneDocked())
            {
                RefreshUiValue();
                upgradeButtonsParent.SetActive(true);
            }
            else
            {
                waitInfoParent.SetActive(true);
            }
            
            upgradeCanvas.DOFade(1, 0.2f).OnComplete(() => upgradeCanvas.interactable = true);
        }

        public async UniTask HideUpgradeUi()
        {
            _currentPlane.DisableUpgradeCamera();
            PlayerManager.instance.FocusToNormalPlayerCamera();
            _uiIsOpen = false;
            _currentPlane = null;
            upgradeCanvas.interactable = false;
            await upgradeCanvas.DOFade(0, 0.2f).OnComplete(() => upgradeCanvas.gameObject.SetActive(false));
            waitInfoParent.SetActive(false);
            upgradeButtonsParent.SetActive(false);
            
            LevelManager.instance.RegisterOtherUiClosed();
        }

        private void RefreshUiValue()
        {
            if(!_currentPlane) Debug.LogError("There is no airplane while stepping into upgrade trigger!?");
            if (!AirplanesManager.instance.IsAirplaneUpgradedToMax(_currentPlane.AirplaneLevel)) 
                _currentUpgradeType = _airplaneManager.GiveCurrentUpgradeType(_currentPlane.AirplaneLevel);
            
            airplaneLevelTmp.text = $"Airplane Level: {_currentPlane.AirplaneLevel + 1}";
            ticketPriceValueTmp.text = $"${_airplaneManager.TicketUpgrades[_currentPlane.TicketPriceUpgradeLevel].upgradeValue}";
            seatsValueTmp.text = $"{_airplaneManager.SeatsUpgrades[_currentPlane.SeatsUpgradeLevel].upgradeValue}";
            
            RefreshUpgradeButton();
        }


        private void RefreshUpgradeButton()
        {
            List<AirplanesManager.UpgradeSetting> list = null;
            var level = 0;
            
            switch (_currentUpgradeType)
            {
                case UpgradeType.TicketPrice:
                    list = _airplaneManager.TicketUpgrades;
                    level = _currentPlane.TicketPriceUpgradeLevel;
                    break;
                case UpgradeType.Seats:
                    list = _airplaneManager.SeatsUpgrades;
                    level = _currentPlane.SeatsUpgradeLevel;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            upgradeButton.RefreshButton(_currentPlane, level, list,_currentUpgradeType);
            if (level + 1 >= list.Count) return;
            _priceForUpgrade = (int)(list[level + 1].upgradePrice * AirplanesManager.instance.GivePriceMultiplier(_currentPlane));
        }

        public int GivePriceForUpgrade() => _priceForUpgrade;

        private void MakeUpgrade()
        {
            AudioManager.instance.PlaySound(SoundType.Upgrade);
            LevelManager.instance.AddXpForAirplaneUpgradeBuy();
            CurrencyManager.instance.SpendCoins(_priceForUpgrade);
            _currentPlane.MakeUpgrade(_currentUpgradeType);
            //RefreshUiValue();
        }

        public void TryToRefreshUpgradeWindow()
        {
            if (!_uiIsOpen) return;

            if (!_currentPlane.IsPlaneDocked())
            {
                waitInfoParent.SetActive(true);
                upgradeButtonsParent.SetActive(false);
            }
            else
            {
                waitInfoParent.SetActive(false);
                upgradeButtonsParent.SetActive(true);
            }

            RefreshUiValue();
        }
    }
}