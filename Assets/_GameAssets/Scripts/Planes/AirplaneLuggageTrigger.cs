using System;
using System.Collections.Generic;
using _GameAssets.Scripts.Items;
using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Player;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Animations;

namespace _GameAssets.Scripts.Planes
{
    public class AirplaneLuggageTrigger : MonoBehaviour
    {
        [SerializeField] private ItemType acceptingItemType;

        [SerializeField] private List<Transform> animationPoints;

        [SerializeField] private bool isTutorial;
        
        private bool _isPlayerInside;
        private PlayerRefreshObjectSystem _player;

        private int _needThatManyLuggages;
        private int _luggageInPlane;
        
        public bool AllLuggagesAreInAirplane() => _luggageInPlane >= _needThatManyLuggages;

        public void SetNeededLuggages(int number) => _needThatManyLuggages = number;
        public void ResetLuggagesInPlane() => _luggageInPlane = 0;
        public Transform GiveFirstAnimationPlace() => animationPoints[0];
        
        private AirplaneLuggagesSetup _myAirplaneLuggageSetup;

        private bool tutorialIsDone;
        private int tutorialGuests;
        
        private void Start()
        {
            _myAirplaneLuggageSetup = GetComponentInParent<AirplaneLuggagesSetup>();
            
            if(isTutorial) tutorialIsDone = TutorialManager.instance.CheckIfTutorialIsDone(TutorialType.Baggages);
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerRefreshObjectSystem player)) return;
            if (!_player) _player = player;
            if (AllLuggagesAreInAirplane()) return;
            _isPlayerInside = true;
            TryGiveAsync().Forget();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out PlayerRefreshObjectSystem player)) return;
            _isPlayerInside = false;
        }

        private async UniTask TryGiveAsync()
        {
            while (_isPlayerInside)
            {
                if (!_player.CheckIfHaveThisItemType(acceptingItemType))
                {
                    _myAirplaneLuggageSetup.DisableTutorialArrow();
                    return;
                }
                GiveAsync();
                await UniTask.Delay(500);
            }
        }
        
        private void GiveAsync()
        {
            var item = _player.GiveItemOfType(acceptingItemType);
            item.ResetParent();
            AnimateLuggage(item, false).Forget();
            
            //Tutorial
            if (!isTutorial) return;
            if (tutorialIsDone) return;
            tutorialGuests++;
            if (tutorialGuests < 4) return;
            tutorialIsDone = true;
            TutorialManager.instance.SetStepAsDone(TutorialType.Baggages);
        }


        public void GiveAutomatic(ItemPickable item)
        {
            AnimateLuggage(item, true).Forget();
        }

        private async UniTask AnimateLuggage(ItemPickable item, bool automatic)
        {
            item.transform.DORotate(animationPoints[0].eulerAngles, 0.3f);
            if (automatic) await item.transform.DOMove(animationPoints[0].position, 1f).SetEase(Ease.Linear);
            else await item.transform.DOMove(animationPoints[1].position, 0.6f);

            var numberIteration = automatic ? 1 : 2;
            
            for (int i = numberIteration; i < animationPoints.Count; i++)
            {
                item.transform.DORotate(animationPoints[i].eulerAngles, 0.15f);
                await item.transform.DOMove(animationPoints[i].position, 1.5f).SetEase(Ease.Linear);
            }

            _luggageInPlane++;
            item.ResetItem();

            if (_luggageInPlane >= _needThatManyLuggages) _myAirplaneLuggageSetup.TryToStartThePlane();
        }
    }
}