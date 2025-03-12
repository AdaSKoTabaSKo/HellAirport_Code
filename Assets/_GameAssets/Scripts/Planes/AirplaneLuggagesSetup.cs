using System;
using _GameAssets.Scripts.Other;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _GameAssets.Scripts.Planes
{
    public class AirplaneLuggagesSetup : MonoBehaviour
    {
        [SerializeField] private AirplaneLuggageTrigger luggageTrigger;
        [SerializeField] private AirplaneLuggagePickupArea luggagePickupArea;
        [SerializeField] private AirplaneLuggagesResetTrigger luggageResetArea;
        [SerializeField] private TutorialArrow tutorialArrow;
        [SerializeField] private GameObject additionalLineObject;
        [Space]
        [SerializeField] private float newBagDelayInSeconds;

        private bool isAutomatic;
        private Airplane _myPlane;

        private int _baggagesToMake;
        private bool _baggagesInMaking;
        
        public void RegisterMyPlane(Airplane p) => _myPlane = p;

        public void TryToStartThePlane() => _myPlane.TryToStartPlane();
        public bool AllLuggagesOnPlane() => luggageTrigger.AllLuggagesAreInAirplane();
        
        public bool IsSystemAutomatic() => isAutomatic;

        public void EnableTutorialArrow()
        {
            tutorialArrow?.gameObject.SetActive(true);
            tutorialArrow?.ShowArrow();
        }

        public void DisableTutorialArrow() => tutorialArrow?.HideAndDisableArrow();
        
        public void ResetLuggagesInPlane()
        {
            luggageTrigger.ResetLuggagesInPlane();
            luggagePickupArea.ResetLuggages();
        }
        
        public async UniTask MakeNewLuggage(bool forTutorial = false)
        {
            _baggagesToMake++;
            
            if (_baggagesInMaking) return;
            _baggagesInMaking = true;

            for (int i = 0; i < _baggagesToMake; i++)
            {
                if (!forTutorial) await UniTask.Delay((int)(newBagDelayInSeconds * 1000));
                else await UniTask.Delay(1000);
                
                luggagePickupArea.MakeNewLuggage().Forget();
            }
            
            _baggagesInMaking = false;
            _baggagesToMake = 0;
        }

        public void SetNewStats(int number)
        {
            luggageTrigger.SetNeededLuggages(number);
            luggagePickupArea.SetMaxLuggages(number);
        }

        public void MakeSystemAutomatic()
        {
            AutomaticModeOn().Forget();
        }

        private async UniTask AutomaticModeOn()
        {
            DisableTutorialArrow();
            await luggageResetArea.ForceToGiveEverytingBack();
            luggageResetArea.gameObject.SetActive(false);
            isAutomatic = true;
            additionalLineObject.SetActive(true);
            luggagePickupArea.MakeAutomatic();
            luggagePickupArea.OnLuggageAdded += ()=> MoveLuggageToPlaneAutomatic().Forget();
        }

        public async UniTask MoveLuggageToPlaneAutomatic()
        {
            var item = luggagePickupArea.GiveAvailableItem();
            //await item.transform.DOMove(firstPointAutomaticAnim.position, 1).SetEase(Ease.Linear);
            luggageTrigger.GiveAutomatic(item);

        }
    }
}