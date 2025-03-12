using System;
using _GameAssets.Scripts.Guest;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GameAssets.Scripts.InteractionObjects
{
    public class SingleQueueInteractionObject : InteractionObject
    {
        [SerializeField] private Queue myQueue;
        
        protected bool _guestAtFront;

        protected GuestSystem myCurrentGuest;
        
        private void Awake()
        {
            RegisterToOnGuestAtFrontEvent(()=>_guestAtFront = true);
        }

        public override async UniTask ManageGuest()
        {
            StartManageGuest();
            await UniTask.Yield();
            await DoThingsWithGuest();
            FinishManagingGuest();
        }

        public Guest.GuestSystem ForceGettingFirstInQueue() => myQueue.GetFirstInQueue();
        
        private void StartManageGuest()
        {
            myCurrentGuest = myQueue.GetFirstInQueue();
            _guestAtFront = false;
        }

        public virtual async UniTask DoThingsWithGuest()
        {
            
        }
        
        public virtual void FinishManagingGuest()
        {
            myQueue.ReleaseFirstInQueue();
            myCurrentGuest.PerformNextStep().Forget();
        }

        public override bool AddNewGuest(Guest.GuestSystem g)
        {
            if (!CanAddGuest())
            {
                Debug.LogWarning($"There is no place to add new guest: {gameObject.name}");
                return false;
            }
            else
            {
                myQueue.AddGuestToQueue(g).Forget();
                return true;
            }
        }

        public void RegisterToQueueOnFreeSpotEvent(Action a) => myQueue.OnFreeSpotInQueue += a;
        public void RegisterToOnGuestAtFrontEvent(Action a) => myQueue.OnGuestAtFront += a;

        public Vector3 GetPosOfFirstQueuePoint() => myQueue.GetPosOfFirstPoint();
        public int GetNumberOfQueueSpots() => myQueue.GetNumberOfQueuePoints();
        public override bool CanAddGuest() => myQueue.CanAddGuests();

        public bool IsGuestAtFrontInMyQueue() => myQueue.IsGuestAtFront();

        public Queue MyQueue() => myQueue;
        
        /// <summary>
        /// Release first place in queue. For example in toilets.
        /// </summary>
        public void MakeFirstQueuePlaceFree() => myQueue.ReleaseFirstInQueue();
    }
}