using System;
using System.Collections.Generic;
using _GameAssets.Scripts.InteractionObjects;
using _GameAssets.Scripts.Items;
using _GameAssets.Scripts.Player;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GameAssets.Scripts.Guest
{
    public class AirportHelper : GuestSystem
    {
        [Space][Header("Helper Specific")]
        [SerializeField] private PlayerRefreshObjectSystem mySystem;
        [SerializeField] private List<Kiosk> myKiosks;

        [SerializeField] private ItemGarbageArea garbage;
        
        
        [SerializeField] private ItemPickupArea tShirtArea;
        [SerializeField] private ItemPickupArea foodArea;
        [SerializeField] private ItemPickupArea perfumesArea;
        [SerializeField] private ItemPickupArea coffeeArea;
        
        
        private List<Kiosk> whereTohelp = new();

        private bool _helpingStarted;
        private bool _startedWalkingWithItem;
        private Vector3 initialPos;
        
        private void Start()
        {
            foreach (var k in myKiosks)
            {
                k.SetHelper(this);
            }

            initialPos = transform.position;

            foreach (var kiosk in myKiosks)
            {
                if (!kiosk.gameObject.activeInHierarchy) continue;
                kiosk.CheckIfNeedHelp();
            }
        }

        public void AddToHelpList(Kiosk k)
        {
            whereTohelp.Add(k);
            if(!_helpingStarted) HelpAsync().Forget();
        }

        private async UniTask HelpAsync()
        {
            _helpingStarted = true;

            while (whereTohelp.Count>0)
            {
                var currentKiosk = whereTohelp[0];
                var type = currentKiosk.GiveAcceptingItemType();
                var firstPos = GiveFirstPosition(type);

                await SetNewDestinationWithWaiting(firstPos, Vector3.zero);
                
                if(whereTohelp.Count == 0)break;
                if(currentKiosk!= whereTohelp[0]) break;

                await SetNewDestinationWithWaiting(currentKiosk.transform.position, Vector3.zero);
                
                if (mySystem.HaveItemsInHand())
                    await SetNewDestinationWithWaiting(garbage.transform.position, Vector3.zero);
            }
            

            if (whereTohelp.Count > 0)
            {
                HelpAsync().Forget();
            }
            else
            {
                _helpingStarted = false;
                SetNewDestination(initialPos);
            }
        }

        public void TriggerKioskRestock(Kiosk k)
        {
            for (var i = whereTohelp.Count - 1; i >= 0; i--)
            {
                if(whereTohelp[i] == k)
                    whereTohelp.RemoveAt(i);
            }
        }
        
        private Vector3 GiveFirstPosition(ItemType t)
        {
            switch (t)
            {
                case ItemType.TShirt:
                    return tShirtArea.transform.position;
                case ItemType.Food:
                    return foodArea.transform.position;
                case ItemType.Perfumes:
                    return perfumesArea.transform.position;
                case ItemType.Coffe:
                    return coffeeArea.transform.position;
                case ItemType.AirplaneFoodPlate:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return Vector3.zero;
        }
        
    }
}