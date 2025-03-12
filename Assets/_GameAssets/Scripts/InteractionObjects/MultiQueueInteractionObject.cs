using System;
using System.Collections.Generic;
using System.Linq;
using _GameAssets.Scripts.Guest;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GameAssets.Scripts.InteractionObjects
{
    public class MultiQueueInteractionObject : InteractionObject
    {
        [SerializeField] private List<Queue> myQueues;
        
        public async UniTask ManageGuest(Guest.GuestSystem g)
        {
            await DoThingsWithGuest(g);
            FinishManagingGuest(g);
        }
        
        public virtual async UniTask DoThingsWithGuest(Guest.GuestSystem g)
        {
            
        }
        
        public void FinishManagingGuest(Guest.GuestSystem g)
        {
            foreach (var q in myQueues.Where(q => g == q.GetFirstInQueue())) q.ReleaseFirstInQueue();

            g.PerformNextStep().Forget();
        }
        
        public override bool AddNewGuest(Guest.GuestSystem g)
        {
            if (!CanAddGuest())
            {
                //Debug.Log("There is no place to add new guest");
                return false;
            }

            foreach (var q in myQueues)
            {
                if (!q.CanAddGuests()) continue;
                ManageGuestInternal(g, q).Forget();
                return true;
            }

            return false;
        }

        private async UniTask ManageGuestInternal(Guest.GuestSystem g, Queue q)
        {
            await q.AddGuestToQueue(g);
            ManageGuest(g).Forget(); 
        }
        
        public int GetNumberOfQueueSpots() => myQueues.Sum(q => q.GetNumberOfQueuePoints());

        public override bool CanAddGuest()
        {
            foreach (var q in myQueues)
            {
                if (q.CanAddGuests()) return true;
            }

            return false;
        }
    }
}