using System;
using System.Collections.Generic;
using _GameAssets.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _GameAssets.Scripts.Planes
{
    public class AirplaneUpgradeUiButton : MonoBehaviour
    {
        [SerializeField] private GameObject normalState;
        [SerializeField] private GameObject maxState;
        
        [Header("Button tickets")]
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TextMeshProUGUI upgradeNameTmp;
        [SerializeField] private TextMeshProUGUI additionValueTmp;
        [SerializeField] private TextMeshProUGUI upgradePriceTmp;
        
        [Header("Images")]
        [SerializeField] private Image buttonBackground;
        [SerializeField] private Image dollarIcon;
        [SerializeField] private Image upgradeIconEnable;
        [SerializeField] private Image upgradeIconDisable;
        [Space]
        [SerializeField] private Sprite buttonBackgroundColor;
        [SerializeField] private Sprite buttonBackgroundGray;
        [Space]
        [SerializeField] private Sprite ticketUpgradeIconColor;
        [SerializeField] private Sprite ticketUpgradeIconGray;
        [Space]
        [SerializeField] private Sprite seatsUpgradeIconColor;
        [SerializeField] private Sprite seatsUpgradeIconGray;
        [Space]
        [SerializeField] private Sprite dollarIconColor;
        [SerializeField] private Sprite dollarIconGray;
        

        private string _additionalText;
        public void AssignUpgradeFunction(UnityAction upgradeFunc) => upgradeButton.onClick.AddListener(upgradeFunc);

        public void RefreshButton(Airplane currentPlane, int currentUpgradeLevel, List<AirplanesManager.UpgradeSetting> list, UpgradeType type)
        {
            if (AirplanesManager.instance.IsAirplaneUpgradedToMax(currentPlane.AirplaneLevel))
            {
                Debug.Log($"{currentPlane}'s upgraded to MAX! UWU!");
                upgradeButton.interactable = false;
                maxState.SetActive(true);
                normalState.SetActive(false);
                return;
            }
            
            maxState.SetActive(false);
            normalState.SetActive(true);

            switch (type)
            {
                case UpgradeType.TicketPrice:
                    upgradeNameTmp.text = $"Tickets";
                    _additionalText = "$";
                    upgradeIconEnable.sprite = ticketUpgradeIconColor;
                    upgradeIconDisable.sprite = ticketUpgradeIconGray;
                    break;
                case UpgradeType.Seats:
                    upgradeNameTmp.text = $"Seats";
                    _additionalText = "";
                    upgradeIconEnable.sprite = seatsUpgradeIconColor;
                    upgradeIconDisable.sprite = seatsUpgradeIconGray;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            
            var multi = AirplanesManager.instance.GivePriceMultiplier(currentPlane);

            var currentValue = list[currentUpgradeLevel].upgradeValue;
            var nextValue = list[currentUpgradeLevel+1].upgradeValue;
            var nextUpgradePrice = list[currentUpgradeLevel+1].upgradePrice;
            
            
            additionValueTmp.text = $"+{_additionalText}{nextValue-currentValue}";
            upgradePriceTmp.text = $"${nextUpgradePrice * multi}";
            
            CheckIfCanBeBought(CurrencyManager.instance.CanSpendCash((int)(nextUpgradePrice * multi)));
        }

        private void CheckIfCanBeBought(bool enable)
        {
            upgradeButton.interactable = enable;
            buttonBackground.sprite = enable ? buttonBackgroundColor : buttonBackgroundGray;
            upgradeIconEnable.gameObject.SetActive(enable);
            upgradeIconDisable.gameObject.SetActive(!enable);
            dollarIcon.sprite = enable ? dollarIconColor : dollarIconGray;
        }
    }
}