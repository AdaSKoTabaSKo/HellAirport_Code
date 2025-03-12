using System;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.Currency;
using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Other;
using _GameAssets.Scripts.Player;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _GameAssets.Scripts.InteractionObjects
{
    public class TicketDesk : SingleQueueInteractionObject
    {
        [SerializeField] private CurrencyArea myCurrencyArea;
        [SerializeField] private Interactable playerInteractableArea;
        [Header("Points")]
        [SerializeField] private Transform bagStartPoint;
        [SerializeField] private Transform bagEndPoint;
        
        [SerializeField] private GameObject workerObject;
        
        [SerializeField] private bool isTutorial;
        [ShowIf(nameof(isTutorial))][SerializeField] private TutorialArrow tutorialArrow;
        
        private bool _canSell;
        private bool _checkingTickets;
        private bool _startCheck;

        private bool tutorialIsDone;
        private int tutorialGuests;
        
        private void Awake()
        {
            playerInteractableArea.AddActionToInteract(()=>base.ManageGuest().Forget());
            RegisterToOnGuestAtFrontEvent(()=> CheckIfCanSellTicket().Forget());
        }

        private void OnEnable()
        {
            DelayAutomaticGuest().Forget();
        }

        private void Start()
        {
            if(isTutorial) tutorialIsDone = TutorialManager.instance.CheckIfTutorialIsDone(TutorialType.TicketBuy);
        }

        public override async UniTask DoThingsWithGuest()
        {
            //Tutorial
            if (isTutorial)
            {
                if (!tutorialIsDone)
                {
                    tutorialGuests++;
                    if (tutorialGuests == 1) tutorialArrow.HideAndDisableArrow();
                    if (tutorialGuests >= 4)
                    {
                        tutorialIsDone = true;
                        TutorialManager.instance.SetStepAsDone(TutorialType.TicketBuy);
                    }
                } 
            }
            
            base.DoThingsWithGuest().Forget();
            await UniTask.Delay(100);
            if(!myCurrentGuest.IsCriminal()) await AirplanesManager.instance.RegisterTicketBuy(ForceGettingFirstInQueue());
            
            var bag = myCurrentGuest.GiveLuggage(true);
            AnimateLuggage(bag.transform).Forget();

            if (myCurrentGuest.IsCriminal()) return;
            
            if(myCurrentGuest.GiveGuestAirplane().CheckIfGuestIsOnRegisteredList(myCurrentGuest)) myCurrentGuest.GiveGuestAirplane().RegisterTicketBuy(myCurrentGuest);
            
            AirplanesManager.instance.RefreshTicketTv();
            var ticketPrice = myCurrentGuest.GiveAirplaneTicketPrice();
            
            if(playerInteractableArea.IsAutomatic()) myCurrencyArea.AddCash(ticketPrice);
            else CurrencyManager.instance.AddCoins(ticketPrice, GetPosOfFirstQueuePoint(),
                PlayerManager.instance.transform.position, enableHaptics: false).Forget();
        }
        
        private async UniTask CheckIfCanSellTicket()
        {
            _checkingTickets = true;
            
            while (!_canSell)
            {
                
                _canSell = AirplanesManager.instance.CanSellTicket();
                if (!_canSell) await UniTask.Delay(500);
            }
            
            playerInteractableArea.MakeItAvailableToInteract();
            _canSell = false;
            _checkingTickets = false;
        }

        private async UniTask AnimateLuggage(Transform bag)
        {
            bag.DORotate(bagStartPoint.transform.eulerAngles, 0.75f);
            await bag.DOJump(bagStartPoint.position, 1f, 1, 1f);
            await bag.DOMove(bagEndPoint.position, 3f).SetEase(Ease.Linear);
            Destroy(bag.gameObject);
        }

        public void EnableAutomaticGuestManagement()
        {
            playerInteractableArea.EnableAutomaticInteraction(this);
            if (workerObject) workerObject.SetActive(true);
            if (!_startCheck) return;
            if (_checkingTickets) return;
            if (IsGuestAtFrontInMyQueue()) playerInteractableArea.MakeItAvailableToInteract();
        }

        private async UniTask DelayAutomaticGuest()
        {
            await UniTask.Delay(1000);
            _startCheck = true;
        }
    }
}