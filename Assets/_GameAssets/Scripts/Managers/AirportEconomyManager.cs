using System;
using System.Collections.Generic;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.InteractionObjects;
using _GameAssets.Scripts.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace _GameAssets.Scripts.Managers
{
    public class AirportEconomyManager : SingleMonoBehaviour<AirportEconomyManager>
    {
        [Header("Airport Parts")]
        [SerializeField] private List<AirportPartSetting> airportParts;
        
        [Header("Airplanes")]
        [SerializeField] private List<AirplanesSetting> airplanesSettings;
        
        [Header("Coffee")]
        [SerializeField] private List<BuyArea> coffeeBuyAreas;
        [SerializeField] private int baseCoffeeKioskBuyCost;
        [SerializeField] private float coffeeKioskBuyCostMultiplier;
        [SerializeField] private int guestCostCoffee;

        [Header("Perfumes")]
        [SerializeField] private List<BuyArea> perfumesBuyAreas;
        [SerializeField] private int basePerfumesKioskBuyCost;
        [SerializeField] private float perfumesKioskBuyCostMultiplier;
        [SerializeField] private int guestCostPerfumes;
        
        [Header("Food")]
        [SerializeField] private List<BuyArea> foodBuyAreas;
        [SerializeField] private int baseFoodKioskBuyCost;
        [SerializeField] private float foodKioskBuyCostMultiplier;
        [SerializeField] private int guestCostFood;
        
        [Header("Shop")]
        [SerializeField] private List<BuyArea> shopBuyAreas;
        [SerializeField] private int baseShopKioskBuyCost;
        [SerializeField] private float shopKioskBuyCostMultiplier;
        [SerializeField] private int guestCostTshirt;
        
        [Header("Toilet")]
        [SerializeField] private List<BuyArea> toiletBuyAreas;
        [SerializeField] private int baseToiletBuyCost;
        [SerializeField] private float toiletBuyCostMultiplier;
        [SerializeField] private int guestCostToilet;
        
        protected override void Awake()
        {
            base.Awake();

            foreach (var aps in airportParts) aps.PartBuyArea.SetItemPrice(aps.PartPrice);
            foreach (var air in airplanesSettings) air.AirplaneBuyArea.SetItemPrice(air.AirplanePrice);
            
            //SetPricesInBuyAreas(airplanesBuyArenas,baseAirplaneBuyCost,airplaneBuyCostMultiplier);
            SetPricesInBuyAreas(coffeeBuyAreas,baseCoffeeKioskBuyCost,coffeeKioskBuyCostMultiplier);
            SetPricesInBuyAreas(perfumesBuyAreas,basePerfumesKioskBuyCost,perfumesKioskBuyCostMultiplier);
            SetPricesInBuyAreas(foodBuyAreas,baseFoodKioskBuyCost,foodKioskBuyCostMultiplier);
            SetPricesInBuyAreas(shopBuyAreas,baseShopKioskBuyCost,shopKioskBuyCostMultiplier);
            SetPricesInBuyAreas(toiletBuyAreas,baseToiletBuyCost,toiletBuyCostMultiplier);
        }

        private void SetPricesInBuyAreas(List<BuyArea> areas,int baseCost, float multi)
        {
            var price = baseCost;

            foreach (var a in areas)
            {
                a.SetItemPrice(price);
                price = (int)(price * multi);
            }
        }

        public int GetPriceForKiosk(ItemType type)
        {
            switch (type)
            {
                case ItemType.None:
                    Debug.LogError("DUPAAAAA");
                    return 0;
                case ItemType.TShirt:
                    return guestCostTshirt;
                case ItemType.Food:
                    return guestCostFood;
                case ItemType.Perfumes:
                    return guestCostPerfumes;
                case ItemType.Ticket:
                    break;
                case ItemType.Luggage:
                    break;
                case ItemType.Money:
                    break;
                case ItemType.Coffe:
                    return guestCostCoffee;
                case ItemType.AirplaneFoodPlate:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return 0;
        }

        public int GetPriceForToilet() => guestCostToilet;
    }

    [Serializable]
    internal class AirportPartSetting
    {
        public BuyArea PartBuyArea;
        public int PartPrice;
    }
    
    [Serializable]
    internal class AirplanesSetting
    {
        public BuyArea AirplaneBuyArea;
        public int AirplanePrice;
    }
}