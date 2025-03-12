using System;
using System.Collections.Generic;
using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Player;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GameAssets.Scripts.Planes
{
    public class AirplaneUpgradeTrigger : MonoBehaviour
    {
        [SerializeField] private Airplane myAirplane;
        [SerializeField] private Animator uiAnimator;
        
        private bool _IsPlayerStanding => !PlayerManager.instance.playerMoving;
        private bool _isPlayerInside;
        private bool _playerPickedUp;
        private bool _playerWasOnTriggerOnce;

        private List<int> upgradePrices = new List<int>();

        private AirplanesManager _airplaneManager;

        public bool IsPlayerInsideUpgradeTrigger() => _isPlayerInside;
        
        private void Start()
        {
            _airplaneManager = AirplanesManager.instance;
            CurrencyManager.instance.OnCoinUpdate += CheckIfCanBuyUpgrade;
            
            upgradePrices.Add(int.MaxValue);
            SetUpgradePrices();
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerManager player)) return;
            _isPlayerInside = true;
            _playerWasOnTriggerOnce = true;
            TryPickupAsync().Forget();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out PlayerManager player)) return;
            _isPlayerInside = false;
        }

        private async UniTask TryPickupAsync()
        {
            while (_isPlayerInside)
            {
                if(_IsPlayerStanding)
                {
                    AirplaneUpgradeUi.instance.ShowUpgradeUi(myAirplane);
                    upgradePrices[0] = AirplaneUpgradeUi.instance.GivePriceForUpgrade();
                    myAirplane.ShowUpgradeCamera();
                    return;
                }

                await UniTask.Yield();
            }
        }
        
        public void SetUpgradePrices()
        {
            if (AirplanesManager.instance.IsAirplaneUpgradedToMax(myAirplane.AirplaneLevel))
            {
                CurrencyManager.instance.OnCoinUpdate -= CheckIfCanBuyUpgrade;
                upgradePrices[0] = int.MaxValue;
                CheckIfCanBuyUpgrade();
                gameObject.SetActive(false);
                return;
            }
            
            List<AirplanesManager.UpgradeSetting> list = null;
            var level = 0;
            
            switch (_airplaneManager.GiveCurrentUpgradeType(myAirplane.AirplaneLevel))
            {
                case UpgradeType.TicketPrice:
                    list = _airplaneManager.TicketUpgrades;
                    level = myAirplane.TicketPriceUpgradeLevel;
                    break;
                case UpgradeType.Seats:
                    list = _airplaneManager.SeatsUpgrades;
                    level = myAirplane.SeatsUpgradeLevel;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (level + 1 >= list.Count) return;
            upgradePrices[0] = (int)(list[level + 1].upgradePrice * AirplanesManager.instance.GivePriceMultiplier(myAirplane));
            CheckIfCanBuyUpgrade();
        }

        private void CheckIfCanBuyUpgrade()
        {
            var currentCash = CurrencyManager.Cash;
            
            foreach (var u in upgradePrices)
            {
                if(currentCash<u) continue;
                uiAnimator.SetBool("Pulse", true);
                return;
            }
            
            uiAnimator.SetBool("Pulse", false);
        }
    }
}