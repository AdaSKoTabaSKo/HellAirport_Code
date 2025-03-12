using System;
using System.Collections.Generic;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.Items;
using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Other;
using _GameAssets.Scripts.Player;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GameAssets.Scripts.Planes
{
    public class AirplaneLuggagePickupArea : MonoBehaviour
    {
        [SerializeField] private int pickupIntervalMilliseconds;
        [SerializeField] private bool isTutorial;
        [ShowIf(nameof(isTutorial))][SerializeField] private TutorialArrow tutorialArrow;
        
        [Header("Area settings")]
        [SerializeField] private Collider myCollider;
        [SerializeField] private Transform firstItemPosition;
        [SerializeField] private bool pickUpWhenRunning;
        [SerializeField] private bool randomModels;
        [HideIf(nameof(randomModels))]
        [SerializeField] private ItemPickable itemPrefab;
        [ShowIf(nameof(randomModels))]
        [SerializeField] private List<ItemPickable> moreItemPrefab;
        [ShowIf(nameof(firstItemPosition))]
        [SerializeField] private List<Transform> animationPlaces;

        private List<ItemPickable> _myItemPool = new(); 
        private List<ItemPickable> _availableItems = new(); 
        
        private bool _IsPlayerStanding => !PlayerManager.instance.playerMoving;
        private PlayerRefreshObjectSystem _player;
        private bool _isPlayerInside;
        private bool _playerPickedUp;
        private bool _tutorialCompleted;

        private int _maxLuggagesForPlane;
        private int _spawnedLuggagesForPlane;

        private AirplaneLuggagesSetup _myAirplaneLuggageSetup;

        public event Action OnLuggageAdded;

        private void Start()
        {
            _myAirplaneLuggageSetup = GetComponentInParent<AirplaneLuggagesSetup>();
            
            if(isTutorial) _tutorialCompleted = TutorialManager.instance.CheckIfTutorialIsDone(TutorialType.Baggages);
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
            if (isTutorial && !_tutorialCompleted)
            {
                _tutorialCompleted = true;
                tutorialArrow.HideAndDisableArrow();
                _myAirplaneLuggageSetup.EnableTutorialArrow();
            }
            
            if (!pickUpWhenRunning)
            {
                while(_isPlayerInside)
                {
                    if(_IsPlayerStanding)
                    {
                        if (!_player.CanPickUpItem()) return;
                        
                        _player.PickUpItem(GiveAvailableItem());
                        await UniTask.Delay(pickupIntervalMilliseconds);
                    }

                    if (_availableItems.Count <= 0) return;
                    if (!gameObject.activeInHierarchy) return;
                    await UniTask.Yield();
                }
            }
            else
            {
                while (_isPlayerInside)
                {
                    if (!_player.CanPickUpItem()) return;
                    _player.PickUpItem(GiveAvailableItem());
                    await UniTask.Delay(pickupIntervalMilliseconds);
                    if (_availableItems.Count <= 0) return;
                    if (!gameObject.activeInHierarchy) return;
                }
            }
        }

        public void MakeAutomatic()
        {
            myCollider.enabled = false;

            var aItems = _availableItems.Count;
            
            for (int i = 0; i < aItems; i++)
            {
                _myAirplaneLuggageSetup.MoveLuggageToPlaneAutomatic().Forget();
            }
        }

        public ItemPickable GiveAvailableItem()
        {
            var item = _availableItems[^1];
            _availableItems.RemoveAt(_availableItems.IndexOf(item));
            return item;
        }

        public void ResetLuggages()
        {
            _spawnedLuggagesForPlane = 0;
        }

        public void SetMaxLuggages(int number) => _maxLuggagesForPlane = number;
        
        public async UniTask MakeNewLuggage(ItemPickable pickedItem = null)
        {
            var height = 0f;

            ItemPickable item;
            item = pickedItem != null ? pickedItem : GiveItemModel();

            if (!_myAirplaneLuggageSetup.IsSystemAutomatic())
            {
                if (_availableItems.Count != 0)
                {
                    var lastItem = _availableItems[^1];
                    height = lastItem.transform.position.y + lastItem.GiveItemHeight();
                }
            }

            item.gameObject.SetActive(true);
            item.transform.position = animationPlaces[0].position;
            await item.transform.DOMove(animationPlaces[1].position, 0.3f);
            if (!_myAirplaneLuggageSetup.IsSystemAutomatic()) 
                await item.transform.DOJump(firstItemPosition.position.With(y:height),
                    1, 1, 0.3f);
                
                
            _availableItems.Add(item);
            OnLuggageAdded?.Invoke();
        }
        
        private ItemPickable GiveItemModel()
        {
            ItemPickable pickedItem;
            
            foreach (var iPool in _myItemPool)
            {
                if(iPool.gameObject.activeInHierarchy) continue;
                if(iPool.IsItemTaken()) continue;
                pickedItem = iPool;
                return pickedItem;
            }
            
            if (randomModels)
            {
                var randomIndex = Random.Range(0, moreItemPrefab.Count);
                var randomItemPrefab = moreItemPrefab[randomIndex];
                var i2 = Instantiate(randomItemPrefab, transform);
                i2.gameObject.SetActive(false);
                _myItemPool.Add(i2);
                pickedItem = i2;
            }
            else
            {
                var i = Instantiate(itemPrefab, transform);
                i.gameObject.SetActive(false);
                _myItemPool.Add(i);
                pickedItem = i;
            }

            return pickedItem;
        }
        
    }
}