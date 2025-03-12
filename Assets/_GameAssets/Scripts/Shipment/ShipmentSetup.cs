using System;
using _GameAssets.Scripts.Currency;
using _GameAssets.Scripts.Managers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace _GameAssets.Scripts.Shipment
{
    public class ShipmentSetup : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private int numberOfBoxes = 18;
        [SerializeField] private int cashPerBox;
        [SerializeField] private float delayBetweenShipmentsInSeconds;
        
        [Header("Settings")]
        [SerializeField] private ShipmentPackageTowersManager shipmentTruck;
        [SerializeField] private ShipmentPackagesTrigger shipmentTrigger;
        [SerializeField] private CurrencyArea myCurrencyArea;

        private bool isAutomatic;
        private Transform firstPointAutomaticAnim;
        
        
        public void AddCashToArea() => myCurrencyArea.AddCash(cashPerBox);

        private void Start()
        {
            StartNewShipment().Forget();
        }

        private async UniTask StartNewShipment()
        {
            //Tutaj podjezdza samolociarz
            
            SetNewStats(numberOfBoxes);
            
            shipmentTruck.SpawnNewTruck().Forget();
        }
        
        public async UniTask FinishShipment()
        {
            //tutaj odjezdza samolociarza
            shipmentTruck.DespawnTruck().Forget();
            
            ResetLuggagesInPlane();
            await UniTask.Delay((int)(delayBetweenShipmentsInSeconds * 1000));
            StartNewShipment().Forget();
        }

        public void ResetLuggagesInPlane()
        {
            shipmentTrigger.ResetShipmentPackagesInPlane();
        }
        
        public void SetNewStats(int number)
        {
            shipmentTrigger.SetNeededShipmentPackages(number);
        }

        public void EnableInvisibleWal() => shipmentTruck.EnableWall();
    }
}