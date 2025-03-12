using System.Collections.Generic;
using _GameAssets.Scripts.Items;
using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Planes;
using _GameAssets.Scripts.Player;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _GameAssets.Scripts.Shipment
{
    public class ShipmentPackagesTrigger : MonoBehaviour
    {
        [SerializeField] private ItemType acceptingItemType;

        [SerializeField] private List<Transform> animationPoints;
        [SerializeField] private ShipmentTutorial tutorial;

        private bool _isPlayerInside;
        
        private ShipmentSetup _myShipmentSystem;
        private PlayerRefreshObjectSystem _player;

        private int _needThatManyShipmentPackages;
        private int _shipmentPackagesInPlane;

        private bool _tutorialDone;
        
        public bool AllShipmentAreInAirplane() => _shipmentPackagesInPlane >= _needThatManyShipmentPackages;
        public void SetNeededShipmentPackages(int number) => _needThatManyShipmentPackages = number;
        public void ResetShipmentPackagesInPlane() => _shipmentPackagesInPlane = 0;
        

        private void Start()
        {
            _myShipmentSystem = GetComponentInParent<ShipmentSetup>();
            _tutorialDone = tutorial.TutorialDone;
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerRefreshObjectSystem player)) return;
            if (!_player) _player = player;
            if (AllShipmentAreInAirplane()) return;
            _isPlayerInside = true;
            TryGiveAsync().Forget();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out PlayerRefreshObjectSystem player)) return;
            _isPlayerInside = false;
        }

        private async UniTask TryGiveAsync()
        {
            while (_isPlayerInside)
            {
                if (!_player.CheckIfHaveThisItemType(acceptingItemType)) return;
                GiveAsync();
                if(!_tutorialDone) tutorial.FinishTutorial();
                _myShipmentSystem.AddCashToArea();
                await UniTask.Delay(500);
            }
        }
        
        private void GiveAsync()
        {
            var item = _player.GiveItemOfType(acceptingItemType);
            item.ResetParent();
            AnimateLuggage(item).Forget();
        }

        private async UniTask AnimateLuggage(ItemPickable item)
        {
            _shipmentPackagesInPlane++;
            if (_shipmentPackagesInPlane >= _needThatManyShipmentPackages) _myShipmentSystem.EnableInvisibleWal();
            
            item.transform.DORotate(animationPoints[0].eulerAngles, 0.3f);
            await item.transform.DOJump(animationPoints[0].position, 1, 1, 0.7f);

            for (int i = 1; i < animationPoints.Count; i++)
            {
                item.transform.DORotate(animationPoints[i].eulerAngles, 0.15f);
                await item.transform.DOMove(animationPoints[i].position, 1.5f).SetEase(Ease.Linear);
            }

            
            item.ResetItem();

            if (_shipmentPackagesInPlane >= _needThatManyShipmentPackages) _myShipmentSystem.FinishShipment().Forget();
        }
    }
}