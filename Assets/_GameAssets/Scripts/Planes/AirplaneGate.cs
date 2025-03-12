using System;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.InteractionObjects;
using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Other;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ExternPropertyAttributes;
using UnityEngine;

namespace _GameAssets.Scripts.Planes
{
    public class AirplaneGate : SingleQueueInteractionObject
    {
        [SerializeField] private Interactable playerInteractableArea;
        [SerializeField] private GameObject workerObject;
        [SerializeField] private bool isTutorial;
        [ShowIf(nameof(isTutorial))][SerializeField] private TutorialArrow tutorialArrow;
        
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
            if(isTutorial) tutorialIsDone = TutorialManager.instance.CheckIfTutorialIsDone(TutorialType.AirplaneBoarding);
        }

        public override async UniTask DoThingsWithGuest()
        {
            //tutaj mozna dac jakas animacje?
            await base.DoThingsWithGuest();
            await UniTask.Delay(250);
            
            //Tutorial
            if (isTutorial)
            {
                if (!tutorialIsDone)
                {
                    tutorialGuests++;
                    if(tutorialGuests == 1) tutorialArrow.HideAndDisableArrow();
                    if (tutorialGuests >= 4)
                    {
                        tutorialIsDone = true;
                        TutorialManager.instance.SetStepAsDone(TutorialType.AirplaneBoarding);
                    }
                } 
            }
            
            //Tutorial end
            
            myCurrentGuest.CanBeDestroyed();
            myCurrentGuest.GoToPlaneAndSit().Forget();
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

        public void ToggleInteractableVisibility(bool enable) => playerInteractableArea.gameObject.SetActive(enable);
        public void GateInteractionCirclePauseToggle(bool enable) => playerInteractableArea.ToggleInteractionPause(enable);
    }
}