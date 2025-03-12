using System;
using System.Collections.Generic;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.Currency;
using _GameAssets.Scripts.Managers;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GameAssets.Scripts.InteractionObjects
{
    public class Toilet : SingleQueueInteractionObject
    {
        [SerializeField] private CurrencyArea myCurrencyArea;
        [SerializeField] private List<ToiletCubicle> wc;
        [Space]
        [SerializeField] private float toiletOccupancyTime;
        
        private int _priceForMyItem;
        
        private void Start()
        {
            _priceForMyItem = AirportEconomyManager.instance.GetPriceForToilet();
            RegisterToOnGuestAtFrontEvent(()=>ManageGuest().Forget());
        }

        public override async UniTask DoThingsWithGuest()
        {
            bool canGo = false;
            ToiletCubicle currentWc = null;

            while (!canGo)
            {
                foreach (var w in wc)
                {
                    if(!w.CanUseToilet()) continue;
                    if(w.occupied) continue;
                    canGo = true;
                    currentWc = w;
                    currentWc.occupied = true;
                    break;
                }
                
                if(!canGo) await UniTask.Delay(250);
            }
            
            DoGuest(myCurrentGuest, currentWc).Forget();
            MakeFirstQueuePlaceFree();
        }

        public override void FinishManagingGuest()
        {
            //this should do nothing in toilets
        }

        private async UniTask DoGuest(Guest.GuestSystem g, ToiletCubicle w)
        {
            await g.SetNewDestinationWithWaiting(w.wcPosition.position, Vector3.zero);
            w.Occupy();
            await g.EnableInteractionUi(toiletOccupancyTime);
            w.Exit().Forget();
            
            myCurrencyArea.AddCash(_priceForMyItem);
            g.PerformNextStep().Forget();
        }
    }
}