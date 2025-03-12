using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.Events;
using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Other;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _GameAssets.Scripts.InteractionObjects
{
    public class SecurityGate : SingleQueueInteractionObject
    {
        [SerializeField] private Interactable playerInteractableArea;
        [SerializeField] private GameObject workerObject;
        
        [SerializeField] private bool isTutorial;
        [ShowIf(nameof(isTutorial))][SerializeField] private TutorialArrow tutorialArrow;
        
        [Header("Animation Points")]
        [SerializeField] private Transform afterGatePoint;
        [SerializeField] private Transform endPoint;
        [SerializeField] private GameObject greenLamp;
        [SerializeField] private GameObject redLamp;
        [Space]
        [SerializeField] private GameObject baggage;
        [SerializeField] private Transform baggageStartPoint;
        [SerializeField] private Transform baggageEndPoint;
        
        private bool _startCheck;
        
        private bool tutorialIsDone;
        private int tutorialGuests;
        
        private void Awake()
        {
            playerInteractableArea.AddActionToInteract(()=>ManageGuest().Forget());
            RegisterToOnGuestAtFrontEvent(()=> playerInteractableArea.MakeItAvailableToInteract());
            DelayAutomaticGuest().Forget();
        }
        
        private void Start()
        {
            if(isTutorial) tutorialIsDone = TutorialManager.instance.CheckIfTutorialIsDone(TutorialType.RefuellingAirplane);
        }

        public override async UniTask DoThingsWithGuest()
        {
            if (myCurrentGuest.IsCriminal())
            {
                await CriminalGuestEvent.instance.StartShowingCriminalWithCamera();
            }
            
            myCurrentGuest.SetNewDestinationWithWaiting(afterGatePoint.position, Vector3.zero).Forget();
            baggage.transform.position = baggageStartPoint.position;
            baggage.SetActive(true);
            baggage.transform.DOMove(baggageEndPoint.position, 1f);
            await UniTask.Delay(500);

            if (myCurrentGuest.IsCriminal())
            {
                redLamp.SetActive(true);
            }
            else
            {
                greenLamp.SetActive(true);
            }
            
            
            await UniTask.Delay(500);
            baggage.SetActive(false);
            greenLamp.SetActive(false);
            redLamp.SetActive(false);
            
            //Tutorial
            if (!isTutorial) return;
            if (!tutorialIsDone)
            {
                tutorialGuests++;
                if (tutorialGuests == 1) tutorialArrow.HideAndDisableArrow();
                if (tutorialGuests >= 4)
                {
                    tutorialIsDone = true;
                    TutorialManager.instance.SetStepAsDone(TutorialType.RefuellingAirplane);
                }
            }
        }

        public override void FinishManagingGuest()
        {
            if (myCurrentGuest.IsCriminal())
            {
                StartWithCriminal().Forget();
            }
            else
            {
                MyQueue().ReleaseFirstInQueue();
                myCurrentGuest.PerformNextStep().Forget();
            }
        }

        private async UniTask StartWithCriminal()
        {
            myCurrentGuest.SecurityCheck();
            await CriminalGuestEvent.instance.StartCriminalEvent();
            MyQueue().ReleaseFirstInQueue();
        }
        
        public void EnableAutomaticGuestManagement()
        {
            playerInteractableArea.EnableAutomaticInteraction(this);
            if (workerObject) workerObject.SetActive(true);
            if (!_startCheck) return;
            if (IsGuestAtFrontInMyQueue()) playerInteractableArea.MakeItAvailableToInteract();
        }
        
        private async UniTask DelayAutomaticGuest()
        {
            await UniTask.Delay(1000);
            _startCheck = true;
        }
    }
}