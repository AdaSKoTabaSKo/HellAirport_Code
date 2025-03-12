using System;
using System.Collections.Generic;
using System.Threading;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.Events;
using _GameAssets.Scripts.InteractionObjects;
using _GameAssets.Scripts.Planes;
using _GameAssets.Scripts.Scriptables;
using _GameAssets.Scripts.WaitingArea;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _GameAssets.Scripts.Guest
{
    public class GuestSystem : MonoBehaviour
    {
        [SerializeField] private bool randomizeLook = true;
        [SerializeField] private GuestsModelsSo guestsModelsSo;
        [SerializeField] private Transform modelPlace;
        
        [Header("Guest Systems")]
        [SerializeField] private GuestEscortTrigger escortTrigger;
        
        [Header("Luggage")]
        [SerializeField] private GameObject luggageParent;
        
        [Header("Interaction Ui")]
        [SerializeField] private GameObject interactionCanvas;
        [SerializeField] private Image interactionWheelFill;

        private GameObject _spawnedLuggage;
        private Animator _guestAnimator;
        private NavMeshAgent _navAgent;
        private GuestType _guestType;

        private List<InteractionObject> possibleObjectsToGo = new List<InteractionObject>();
        private SingleQueueInteractionObject _middleGate;
        private Airplane _myAirplane;
        
        private WaitingArea.WaitingArea _myWaitingArea;
        private WaitingSeat _myWaitingSeat;
        private bool _isWaitingGuest;
        private bool _isSitting;
        private bool _isCriminal;
        
        private bool _wentToMiddleGate;
        private bool _destroyMe;
        private bool _interactionStarted;

        private CancellationTokenSource _cancellationTokenSource;
        
        private void Awake()
        {
            _navAgent = GetComponent<NavMeshAgent>();
            if (randomizeLook)
            {
                RandomizeGuestLook();
            }
            else
            {
                _guestAnimator = GetComponentInChildren<Animator>();
            }
        }

        private void RandomizeGuestLook()
        {
            var newModel = Instantiate(guestsModelsSo.GiveRandomGuestModel(), modelPlace);
            _guestAnimator = newModel.GetComponent<Animator>();

            _spawnedLuggage = Instantiate(guestsModelsSo.GiveRandomLuggageModel(), luggageParent.transform);
        }

        private void Update()
        {
            var a = _navAgent.velocity.magnitude;
            _guestAnimator.SetFloat("Speed", a);
        }
        
        private void LateUpdate()
        {
            if(_interactionStarted) interactionCanvas.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        
        public async UniTask EnableInteractionUi(float secondsToFill)
        {
            _interactionStarted = true;
            interactionCanvas.SetActive(true);
            interactionWheelFill.fillAmount = 0;
            interactionCanvas.transform.DOScale(1, 0.3f);

            await interactionWheelFill.DOFillAmount(1, secondsToFill).SetEase(Ease.Linear);

            await interactionCanvas.transform.DOScale(0, 0.3f);
            interactionCanvas.SetActive(false);
            _interactionStarted = false;
        }

        public void SetGuestType(GuestType type)
        {
            var t = 0;
            
            switch (type)
            {
                case GuestType.Arrival:
                    t = 9; //walkable i arrivalonly w bitach
                    _guestType = GuestType.Arrival;
                    break;
                case GuestType.Departure:
                    t = 17;  //walkable i departure only w bitach
                    _guestType = GuestType.Departure;
                    _guestAnimator.SetTrigger("BaggageOn");
                    CriminalGuestEvent.instance.TryRegisterAsCriminal(this);
                    break;
                case GuestType.Tutorial:
                    t = 17;
                    _guestType = GuestType.Tutorial;
                    Destroy(_spawnedLuggage);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            _navAgent.areaMask = t;
        }

        public void ToggleAi(bool enable, Vector3 warpPos)
        {
            _navAgent.enabled = enable;
            if (!enable) return;
            _navAgent.Warp(warpPos);
            _guestAnimator.SetTrigger("Stand");
        }

        public void SetDestinationsList(List<InteractionObject> possibleSteps, SingleQueueInteractionObject finalDestiny)
        {
            possibleObjectsToGo = possibleSteps;
            _middleGate = finalDestiny;
            
            possibleObjectsToGo.Sort( (x,y)=> Random.Range(-1, 2));
            
            while(possibleObjectsToGo.Count > 1)
            {
                possibleObjectsToGo.RemoveAt(possibleObjectsToGo.Count - 1);
            }
        }

        public async UniTask PerformNextStep()
        {
            if (_destroyMe) return;
            if(_guestType == GuestType.Departure) PerformStepForDeparture().Forget();  
            if(_guestType == GuestType.Arrival) PerformStepForArrival().Forget(); 
            if(_guestType == GuestType.Tutorial) _myAirplane.GiveMeAirplaneGate().AddNewGuest(this);  
        }
        
        private async UniTask PerformStepForArrival()
        {
            if (TryGoGoPlaces()) return;
            
            while (!_wentToMiddleGate)
            {
                if (_middleGate.CanAddGuest())
                {
                    _middleGate.AddNewGuest(this);
                    _wentToMiddleGate = true;
                    return;
                }
                await UniTask.Delay(250);
            }
        }
        
        private async UniTask PerformStepForDeparture()
        {
            if (_middleGate == null || _myAirplane == null) 
            {
                if (!IsCriminal())
                {
                    Debug.LogError("Missing references in ShowNextStep method.");
                    return;
                }
            }

            while (!_wentToMiddleGate)
            {
                if (_middleGate.CanAddGuest())
                {
                    _middleGate.AddNewGuest(this);
                    _wentToMiddleGate = true;
                    return;
                }
                await UniTask.Delay(250);
            }
            
            if (TryGoGoPlaces()) return;

            if (_isWaitingGuest)
            {
                GoSitInWaitingArea().Forget();
                return;
            }
            
            _myAirplane.GiveMeAirplaneGate().AddNewGuest(this);
        }
        
        private bool TryGoGoPlaces()
        {
            if (possibleObjectsToGo.Count == 0) return false;
            var objectToGo = possibleObjectsToGo[0];
            possibleObjectsToGo.Remove(objectToGo);
            return objectToGo.AddNewGuest(this);
        }
        
        public void SetNewDestination(Vector3 place)
        {
            _navAgent.SetDestination(place);
        }
        
        public async UniTask SetNewDestinationWithWaiting(Vector3 place, Vector3 lookPosition)
        {
            _navAgent.SetDestination(place);
            await UniTask.WaitUntil(() =>
                !_navAgent.pathPending && _navAgent.remainingDistance <= 0.2f);
            
            if(lookPosition != Vector3.zero) await transform.DOLookAt(transform.position + lookPosition, 0.2f, AxisConstraint.Y);
        }
        
        public async UniTask SetNewDestinationWithWaitingAndKillAtTheEndLol(Vector3 place, Action action = null)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            
            _navAgent.SetDestination(place);
            await UniTask.WaitUntil(() =>
                !_navAgent.pathPending && _navAgent.remainingDistance <= 0.2f, cancellationToken: _cancellationTokenSource.Token);
            await UniTask.Delay(100);
            _navAgent.enabled = false;
            action?.Invoke();
            Kill().Forget();
        }
        
        public async UniTask SetNewDestinationWithWaitingAndAction(Vector3 place, Action a)
        {
            _navAgent.SetDestination(place);
            await UniTask.WaitUntil(() =>
                !_navAgent.pathPending && _navAgent.remainingDistance <= 0.2f);
            a?.Invoke();
        }

        public async UniTask GoToPlaneAndSit()
        {
            var seatPos = _myAirplane.ShowCorrectSeatPosition(this);
            await SetNewDestinationWithWaiting(seatPos.position, seatPos.forward);
            _navAgent.enabled = false;
            transform.position+= Vector3.zero.With(y:0.25f);
            _guestAnimator.SetTrigger("Sit");
            _myAirplane.RegisterGuestInSeat();
        }

        public void RegisterAsWaitingGuest(WaitingArea.WaitingArea area)
        {
            _isWaitingGuest = true;
            _myWaitingArea = area;
        }
        
        public async UniTask GoSitInWaitingArea()
        {
            _myWaitingSeat = _myWaitingArea.GiveFreeSeat();
            _myWaitingSeat.Occupy();
            await SetNewDestinationWithWaiting(_myWaitingSeat.transform.position, _myWaitingSeat.transform.forward);
            if (!_isWaitingGuest) return;
            _isSitting = true;
            _navAgent.enabled = false;
            transform.DOMoveY(transform.position.y + 0.25f, 0.3f);
            _guestAnimator.SetTrigger("Sit");
        }

        public async UniTask SwitchToNormalPassenger()
        {
            _isWaitingGuest = false;
            
            if (_myWaitingSeat)
            {
                _myWaitingSeat.Exit();
                if (_isSitting)
                {
                    _isSitting = false;
                    _guestAnimator.SetTrigger("Stand");
                    await UniTask.Delay(250);
                    _navAgent.enabled = true;
                }
                PerformNextStep().Forget();
            }
            else
            {
                _myWaitingArea.UnregisterGuest();
            }
        }
        
        public GameObject GiveLuggage(bool modifyAnimation)
        {
            var bag = _spawnedLuggage;
            _spawnedLuggage = null;
            bag.transform.parent = null;
            if(modifyAnimation) _guestAnimator.SetTrigger("BaggageOff");
            return bag;
        }
        
        public void AttachLuggage(GameObject bag)
        {
            _guestAnimator.SetTrigger("BaggageOn");
            bag.transform.parent = luggageParent.transform;
            bag.transform.localEulerAngles = Vector3.zero;
            bag.transform.localPosition = Vector3.zero;
            _spawnedLuggage = bag;
        }
        
        public void CanBeDestroyed() => _destroyMe = true;
        public void SetGuestAirplane(Airplane a) => _myAirplane = a;
        public Airplane GiveGuestAirplane() => _myAirplane;
        public int GiveAirplaneTicketPrice() => _myAirplane.GiveTicketPrice();
        public void SetAsCriminal() => _isCriminal = true;
        public bool IsCriminal() => _isCriminal;

        public void SecurityCheck()
        {
            if (_isCriminal) return;
            if (!_isWaitingGuest) _myAirplane.UnregisterGuest(this, false);
            else
            {
                _myAirplane.UnregisterGuest(this, true);
                _myWaitingArea.UnregisterGuest();
            }
        }

        public void EnableEscortTrigger() => escortTrigger.EnableTrigger();
        public void DisableEscortTrigger() => escortTrigger.DisableTrigger().Forget();
        
        private async UniTask Kill()
        {
            await UniTask.Delay(5000);
            Destroy(gameObject);
        }
        
        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
        }
    }

    public enum GuestType
    {
        Arrival,
        Departure,
        Tutorial
    }
}
