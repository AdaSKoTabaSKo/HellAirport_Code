using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.InteractionObjects;
using _GameAssets.Scripts.Items;
using _GameAssets.Scripts.Planes;
using CrazyLabsExtension;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoreMountains.NiceVibrations;
using UnityEngine;

namespace _GameAssets.Scripts.Player
{
    public class PlayerRefreshObjectSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int maxItemsInHand;
        [SerializeField] private Transform firstPosition;
        [SerializeField] private bool enableSoundOnPickup;
        
        [SerializeField] private Animator animator;

        private List<ItemPickable> _itemsInHand = new();
        private int _currentItemsInHand;
        
        public bool CanPickUpItem() => _currentItemsInHand < maxItemsInHand;

        public void PickUpItem(ItemPickable itemObject)
        {
            var height = 0f;
            if(_currentItemsInHand == 0) animator.SetBool("PickupBox", true);
            else
            {
                var lastItem = _itemsInHand[_currentItemsInHand - 1];
                height = lastItem.transform.localPosition.y + lastItem.GiveItemHeight();
            }
            
            itemObject.SetAsTaken();
            itemObject.gameObject.SetActive(true);
            itemObject.transform.SetParent(firstPosition);
            itemObject.transform.localEulerAngles = Vector3.zero;
            itemObject.transform.localPosition = Vector3.zero.With(y: height);

            _currentItemsInHand++;
            _itemsInHand.Add(itemObject);


            if (enableSoundOnPickup)
            {
                HapticFeedbackController.TriggerHaptics(HapticTypes.LightImpact);
                AudioManager.instance.PlaySound(SoundType.ItemPickup);
            }
        }

        public async UniTask DeliverPickedItemToKiosk(ItemType kioskNeeds, Kiosk kiosk)
        {
            if (!CheckIfHaveThisItemType(kioskNeeds)) return;
            
            while (CheckIfHaveThisItemType(kioskNeeds))
            {
                if (kiosk.IsKioskFull()) return;
                kiosk.RestockKiosk();
                DestroyItemOfType(kioskNeeds);
                await UniTask.Yield();
            }
        }

        public bool CheckIfHaveThisItemType(ItemType t) => _itemsInHand.Any(i => i.GiveItemType() == t);

        public async UniTask RemoveAllItem()
        {
            Debug.Log($"Usuwam wszystkie przedmioty");
            for (int i = _currentItemsInHand - 1; i >= 0; i--)
            {
                var item = _itemsInHand[i];

                // Check if the item type is matching
                if (item.GiveItemType() == ItemType.Luggage) continue;

                // Save the height of the item being removed, for use later
                float itemHeight = item.GiveItemHeight();

                // Reset and remove item from _itemsInHand
                item.ResetItem();
                _itemsInHand.RemoveAt(i);
                _currentItemsInHand--;

                // Update position of the items above the removed one
                for (int j = i; j < _currentItemsInHand; j++)
                {
                    _itemsInHand[j].transform.localPosition -= new Vector3(0, itemHeight, 0);
                }
            }
            
            if(_currentItemsInHand == 0) animator.SetBool("PickupBox", false);
        }

        public ItemPickable GiveItemOfType(ItemType t)
        {
            for (int i = _currentItemsInHand - 1; i >= 0; i--)
            {
                var item = _itemsInHand[i];
                if (item.GiveItemType() != t) continue;
                
                var itemHeight = item.GiveItemHeight();

                _itemsInHand.RemoveAt(i);
                _currentItemsInHand--;
                
                if(_currentItemsInHand == 0) animator.SetBool("PickupBox", false);

                for (int j = i; j < _currentItemsInHand; j++)
                {
                    _itemsInHand[j].transform.localPosition -= new Vector3(0, itemHeight, 0);
                }

                return item;
            }

            return null;
        }

        private void DestroyItemOfType(ItemType t)
        {
            // Traverse the list from last to first
            for (int i = _currentItemsInHand - 1; i >= 0; i--)
            {
                var item = _itemsInHand[i];

                // Check if the item type is matching
                if (item.GiveItemType() != t)
                    continue;

                // Save the height of the item being removed, for use later
                float itemHeight = item.GiveItemHeight();

                // Reset and remove item from _itemsInHand
                item.ResetItem();
                _itemsInHand.RemoveAt(i);
                _currentItemsInHand--;
                Debug.Log($"Resetuje przedmiot typu: {t}");
                
                // Update position of the items above the removed one
                for (int j = i; j < _currentItemsInHand; j++)
                {
                    _itemsInHand[j].transform.localPosition -= new Vector3(0, itemHeight, 0);
                }
                // Exit the loop since the item is found and removed
                break;
            }
            
            if(_currentItemsInHand == 0) animator.SetBool("PickupBox", false);
        }

        public bool HaveItemsInHand() => _currentItemsInHand > 0;

    }
    
    public enum ItemType
    {
        None,
        TShirt,
        Food,
        Perfumes,
        Ticket,
        Luggage,
        Money,
        Coffe,
        AirplaneFoodPlate,
        ShipmentPackage,
    }
}