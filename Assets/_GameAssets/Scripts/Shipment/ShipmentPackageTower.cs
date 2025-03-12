using System;
using System.Collections.Generic;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.Items;
using _GameAssets.Scripts.Player;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace _GameAssets.Scripts.Shipment
{
    public class ShipmentPackageTower : MonoBehaviour
    {
        [SerializeField] private int pickupIntervalMilliseconds;
        [Space]
        [Header("Area settings")]
        [SerializeField] private List<ItemPickable> myItemPool;
        private List<ItemPickable> _availableItems = new(); 
        [Space]
        [SerializeField] private ShipmentTutorial tutorial;
        
        private PlayerRefreshObjectSystem _player;
        private bool _isPlayerInside;
        private bool _tutorialDone;
        
        private void Start()
        {
            _tutorialDone = tutorial.TutorialDone;
        }

        void OnTriggerEnter(Collider other)
        {
            if (_availableItems.Count <= 0) return;
            if (!other.TryGetComponent(out PlayerRefreshObjectSystem player)) return;
            if (!_player) _player = player;
            _isPlayerInside = true;
            TryPickupAsync().Forget();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out PlayerRefreshObjectSystem player)) return;
            _isPlayerInside = false;
        }

        private async UniTask TryPickupAsync()
        {
            while (_isPlayerInside)
            {
                if (!_player.CanPickUpItem()) return;
                _player.PickUpItem(GiveAvailableItem());
                if(!_tutorialDone) tutorial.MoveToNextTutorialStep();
                await UniTask.Delay(pickupIntervalMilliseconds);
                if (_availableItems.Count <= 0) return;
                if (!gameObject.activeInHierarchy) return;
            }
        }
        
        public ItemPickable GiveAvailableItem()
        {
            var item = _availableItems[^1];
            _availableItems.RemoveAt(_availableItems.IndexOf(item));
            return item;
        }

        public void RespawnBoxes()
        {
            foreach (var iPool in myItemPool)
            {
                iPool.ResetItem();
                _availableItems.Add(iPool);
                iPool.gameObject.transform.rotation = Quaternion.identity;
                iPool.gameObject.SetActive(true);
            }
        }
    }
}