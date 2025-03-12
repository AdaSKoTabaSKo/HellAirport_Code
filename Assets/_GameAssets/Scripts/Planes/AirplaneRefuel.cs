using System;
using System.Collections.Generic;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Other;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _GameAssets.Scripts.Planes
{
    public class AirplaneRefuel : MonoBehaviour
    {
        [SerializeField] private Interactable myInteractablePoint;
        [Space]
        [SerializeField] private GameObject hoseOn;
        [SerializeField] private GameObject hoseOff;
        [SerializeField] private GameObject tankObject;
        [Space]
        [SerializeField] private float refuellingTime;
        [Space]
        [SerializeField] private GameObject timerHandler;
        [SerializeField] private Image timerFill;
        
        [SerializeField] private bool isTutorial;
        [ShowIf(nameof(isTutorial))][SerializeField] private TutorialArrow tutorialArrow;
        
        private Airplane _myAirplane;

        private bool _airplaneIsFueled;
        
        private bool tutorialIsDone;
        
        public bool IsAirplaneFueled() => _airplaneIsFueled;

        private void Start()
        {
            _myAirplane = GetComponentInParent<Airplane>();
            
            timerHandler.gameObject.SetActive(false);
            timerHandler.transform.localScale = Vector3.zero;
            
            hoseOff.SetActive(true);
            hoseOn.SetActive(false);
            
            if(isTutorial) tutorialIsDone = TutorialManager.instance.CheckIfTutorialIsDone(TutorialType.RefuellingAirplane);
        }
        
        public void MakeAirplaneFuelTankEmpty()
        {
            _airplaneIsFueled = false;
            myInteractablePoint.gameObject.SetActive(true);
        }

        public void RefuelThePlane()
        {
            timerFill.fillAmount = 0;
            hoseOff.SetActive(false);
            hoseOn.SetActive(true);
            timerHandler.SetActive(true);
            myInteractablePoint.gameObject.SetActive(false);
            RefuelThePlaneAsync().Forget();
            
            if (!isTutorial) return;
            if (!tutorialIsDone) tutorialArrow.HideAndDisableArrow();
        }

        private async UniTask RefuelThePlaneAsync()
        {
            var tween = tankObject.transform.DOScaleY(1.3f, 0.2f).SetLoops(-1, LoopType.Yoyo);
            await timerHandler.transform.DOScale(1, 0.2f);
            timerFill.DOFillAmount(1, refuellingTime).SetEase(Ease.Linear);

            await UniTask.Delay((int)(refuellingTime * 0.25f) * 1000);
            
            if (isTutorial)
            {
                if (!tutorialIsDone)
                {
                    tutorialIsDone = true;
                    TutorialManager.instance.SetStepAsDone(TutorialType.RefuellingAirplane);
                }
            }
            
            await UniTask.Delay((int)(refuellingTime *0.75f) * 1000);
            
            _airplaneIsFueled = true;
            
            hoseOff.SetActive(true);
            hoseOn.SetActive(false);
            
            tween.Kill(true);
            tankObject.transform.DOScaleY(1.19f, 0.2f);
            _myAirplane.TryToStartPlane();

            
            
            await timerHandler.transform.DOScale(0, 0.2f);
            timerHandler.gameObject.SetActive(false);
        }
    }
}