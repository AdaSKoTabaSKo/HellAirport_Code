using System.Collections.Generic;
using _GameAssets.Scripts.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace _GameAssets.Scripts.InteractionObjects
{
    public class BaggageClaimArea : SingleQueueInteractionObject
    {
        [Header("Time settings")]
        [SerializeField] private float timeOfBagMoving;
        
        [Header("Points")]
        [SerializeField] private Transform bagStartPoint;
        [SerializeField] private Transform midMidPoint;
        [SerializeField] private Transform bagEndPoint;
        [SerializeField] private Transform takePoint;
        [SerializeField] private Transform fakeGetawayPoint;

        private List<GameObject> currentBags = new();
        
        private void Awake()
        {
            RegisterToOnGuestAtFrontEvent(()=> ManageGuest().Forget());
        }

        public override async UniTask DoThingsWithGuest()
        {
            base.DoThingsWithGuest().Forget();
            //idzie to punktu odbioru bagzu
            await myCurrentGuest.SetNewDestinationWithWaiting(takePoint.position, Vector3.zero);
            await MoveLuggage();
            //wypierdala z punktu i robi wolne miejscoweczka
            myCurrentGuest.CanBeDestroyed();
            myCurrentGuest.SetNewDestinationWithWaitingAndKillAtTheEndLol(fakeGetawayPoint.position).Forget();
        }

        public void TakeLuggage(GameObject bag)
        {
            bag.transform.parent = transform;
            bag.transform.position = bagStartPoint.position;
            bag.transform.rotation = new Quaternion(0.707106829f, -0.707106829f, 0, 0);
            bag.transform.localScale = Vector3.one;
            currentBags.Add(bag);
        }

        public async UniTask MoveLuggage()
        {
            var bag = currentBags[0];
            currentBags.RemoveAt(0);

            bag.transform.DORotateQuaternion(new Quaternion(0.5f,-0.5f,-0.5f,0.5f), timeOfBagMoving).SetEase(Ease.Linear);
            await bag.transform.DOMove(midMidPoint.position, timeOfBagMoving/2).SetEase(Ease.Linear);
            await bag.transform.DOMove(bagEndPoint.position, timeOfBagMoving/2).SetEase(Ease.Linear);
            
            myCurrentGuest.AttachLuggage(bag);
        }
        
        //automatyczne nakurwianie 
    }
}