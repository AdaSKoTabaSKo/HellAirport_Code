using System;
using System.Collections.Generic;
using _GameAssets.Scripts.Guest;
using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Player;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GameAssets.Scripts.Items
{
    public class ItemPickupArea : MonoBehaviour
    {
        [SerializeField] private int pickupIntervalMilliseconds;
        [SerializeField] private bool isTutorial;
        
        [Space]
        [Header("Area settings")]
        [SerializeField] private bool pickUpWhenRunning;
        [SerializeField] private bool randomModels;
        [HideIf(nameof(randomModels))]
        [SerializeField] private ItemPickable itemPrefab;
        [ShowIf(nameof(randomModels))]
        [SerializeField] private List<ItemPickable> moreItemPrefab;

        private List<ItemPickable> _myItemPool = new(); 
        
        private bool _IsPlayerStanding => !PlayerManager.instance.playerMoving;
        private PlayerRefreshObjectSystem _player;
        private bool _isPlayerInside;
        private bool _playerPickedUp;
        private bool _tutorialIsDone;

        private void Start()
        {
            if(isTutorial) _tutorialIsDone = TutorialManager.instance.CheckIfTutorialIsDone(TutorialType.PerfumesStacking);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerManager player))
            {
                if (!_player) _player = player.GetComponent<PlayerRefreshObjectSystem>();
                _isPlayerInside = true;
                TryPickupAsync(_player).Forget();
            }
            
            if (other.TryGetComponent(out AirportHelper helper))
            {
                _isPlayerInside = true;
                TryPickupAsync(helper.GetComponent<PlayerRefreshObjectSystem>()).Forget();
            }
            
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out PlayerRefreshObjectSystem player)) return;
            _isPlayerInside = false;
        }

        private async UniTask TryPickupAsync(PlayerRefreshObjectSystem p)
        {
            if (!pickUpWhenRunning)
            {
                while(_isPlayerInside)
                {
                    if (p == _player)
                    {
                        if(_IsPlayerStanding)
                        {
                            if (!_player.CanPickUpItem()) return;
                            _player.PickUpItem(GiveItemModel());
                            
                            if (isTutorial &&!_tutorialIsDone)
                            {
                                _tutorialIsDone = true;
                                TutorialManager.instance.StartMidTutorialAfterPickingUpPerfumes().Forget();
                                //return;
                            }
                            
                            await UniTask.Delay(pickupIntervalMilliseconds);
                        }
                    }
                    else
                    {
                        if (!p.CanPickUpItem()) return;
                        p.PickUpItem(GiveItemModel());
                        await UniTask.Delay(pickupIntervalMilliseconds);
                    }
                    

                    if (!gameObject.activeInHierarchy) return;
                    await UniTask.Yield();
                }
            }
            else
            {
                while (_isPlayerInside)
                {
                    if (!_player.CanPickUpItem()) return;
                    _player.PickUpItem(GiveItemModel());
                    await UniTask.Delay(pickupIntervalMilliseconds);
                    if (!gameObject.activeInHierarchy) return;
                }
            }
            
        }
        
        private ItemPickable GiveItemModel()
        {
            foreach (var iPool in _myItemPool)
            {
                if(iPool.IsItemTaken()) continue;
                return iPool;
            }
            
            if (randomModels)
            {
                var randomIndex = Random.Range(0, moreItemPrefab.Count);
                var randomItemPrefab = moreItemPrefab[randomIndex];
                var i2 = Instantiate(randomItemPrefab, transform);
                _myItemPool.Add(i2);
                return i2;
            }
            
            var i = Instantiate(itemPrefab, transform);
            _myItemPool.Add(i);
            return i;
        }
    }
}