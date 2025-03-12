using System;
using System.Threading;
using _GameAssets.Scripts.InteractionObjects;
using _GameAssets.Scripts.Player;
using CrazyLabsExtension;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoreMountains.NiceVibrations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _GameAssets.Scripts.Core
{
    public class Interactable : MonoBehaviour
    {
        [SerializeField] private bool enableInteractionAlways;
        [Space][Header("Ui")]
        [SerializeField] private GameObject floorCircle;
        [SerializeField] private GameObject interactionWheelHandler;
        [SerializeField] private Image interactionWheelFill;
        [Space][Header("Settings")]
        [SerializeField] private float interactionTime;
        [SerializeField] private bool dontHideUi;
        [SerializeField] private string animationBoolName;
        [SerializeField] private Animator workerAnimator;
        [SerializeField] private bool enablePlayerLookAt;
        
        public UnityEvent whatShouldBeDone;

        public CancellationTokenSource _cancellationTokenSource;
        [HideInInspector] public bool _interactionStarted;

        private bool _isThereUi;
        private Tween tweener;
        private bool _canInteract;
        private bool _playerOnTrigger;
        private bool _pauseInteraction;

        private bool _automaticMode;
        private SphereCollider _myCollider;

        private SingleQueueInteractionObject _myMainObject;
        
        private void Awake()
        {
            if (interactionWheelFill == null) return;
            _isThereUi = true;
            interactionWheelFill.fillAmount = 0;
            ToggleInteractionWheelVisibility(false, true);
        }

        private void Start()
        {
            if(enableInteractionAlways) MakeItAvailableToInteract();
        }

        public void Interact()
        {
            whatShouldBeDone.Invoke();
        }
        
        public void AddActionToInteract(UnityAction action)
        {
            whatShouldBeDone.AddListener(action);
        }
        
        public async UniTask StartInteraction()
        {
            if (_interactionStarted) return;
            if (!_canInteract) return;
            if (!enableInteractionAlways) _canInteract = false;
            
            _interactionStarted = true;
            _cancellationTokenSource = new CancellationTokenSource();

            while (_pauseInteraction)
            {
                await UniTask.Delay(250);
            }

            if (enablePlayerLookAt)
            {
                if(!_automaticMode) PlayerManager.instance.ForcePlayerLookAtOnPosition(transform.position + -transform.forward);
            }
            
            if (animationBoolName != "")
            {
                if (!_automaticMode)
                {
                    PlayerManager.instance.EnableAnimationBool(animationBoolName);
                }
                else
                {
                    workerAnimator.SetBool(animationBoolName, true);
                }
            }
            
            try
            {
                await TryInteract(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                if (_isThereUi)
                {
                    tweener.Kill();
                    interactionWheelFill.fillAmount = 0;
                    _canInteract = true;
                }
            }

            if (animationBoolName != "")
            {
                if (!_automaticMode)
                {
                    PlayerManager.instance.DisableAnimationBool(animationBoolName);
                }
                else
                {
                    workerAnimator.SetBool(animationBoolName, false);
                }
            }
            
            if (enablePlayerLookAt)
            {
                if(!_automaticMode) PlayerManager.instance.UnFocusPlayerLookAtOnPosition();
            }
            
            _interactionStarted = false;
            
        }

        private async UniTask TryInteract(CancellationToken cancellationToken)
        {
            if (_isThereUi)
            {
                if(_automaticMode) ToggleInteractionWheelVisibility(true,false);
                tweener = interactionWheelFill.DOFillAmount(1, interactionTime).SetEase(Ease.Linear);
                await UniTask.WaitUntil(() => interactionWheelFill.fillAmount >= 1, cancellationToken: cancellationToken);
            }
            else
            {
                await UniTask.Delay((int)interactionTime * 1000, cancellationToken: cancellationToken);
            }
            
            cancellationToken.ThrowIfCancellationRequested();

            Interact();
            if(!_automaticMode) HapticFeedbackController.TriggerHaptics(HapticTypes.LightImpact);
            else
                ToggleInteractionWheelVisibility(false,false);

            if (_isThereUi) interactionWheelFill.fillAmount = 0;
        }

        public void MakeItAvailableToInteract(bool enableInteraction = true)
        {
            _canInteract = true;
            
            if (_automaticMode)
            {
                if (!enableInteraction) return;
                StartInteraction().Forget();
            }
            
            if(_playerOnTrigger) StartInteraction().Forget();
        }

        public void ToggleInteractionWheelVisibility(bool enable, bool instant)
        {
            if (!_isThereUi) return;
            if (dontHideUi) return;
            
            var time = instant ? 0 : 0.25f;
            
            if (enable)
            {
                interactionWheelHandler.transform.DOScale(Vector3.one * 0.9f, time).SetEase(Ease.OutBack);
            }
            else
            {
                interactionWheelHandler.transform.DOScale(Vector3.zero, time).SetEase(Ease.InBack);
            }
        }

        public void TogglePlayerOnTrigger(bool on) => _playerOnTrigger = on;
        public bool IsAutomatic() => _automaticMode;
        public void EnableAutomaticInteraction(SingleQueueInteractionObject mainObject = null)
        {
            _myCollider = GetComponent<SphereCollider>();

            if (mainObject) _myMainObject = mainObject;
            
            _automaticMode = true;
            _myCollider.enabled = false;
            floorCircle.SetActive(false);
            ToggleInteractionWheelVisibility(false,false);
            _playerOnTrigger = false;
        }

        public void ToggleInteractionPause(bool enable) => _pauseInteraction = enable;
    }
}