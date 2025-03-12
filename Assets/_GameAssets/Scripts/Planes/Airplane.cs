using System;
using System.Collections.Generic;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.Guest;
using _GameAssets.Scripts.InteractionObjects;
using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Player;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GameAssets.Scripts.Planes
{
    public class Airplane : MonoBehaviour
    {
        [Header("IMPORTANT")] [SerializeField] private string saveName;

        [Space] [Header("Setup")]
        [SerializeField] private AirplaneGate myGate;
        [SerializeField] private AirplaneUpgradeTrigger myUpgradeTrigger;
        [SerializeField] private AirplaneLuggagesSetup myLuggagesSetup;
        [SerializeField] private BuyArea myBuyArea;
        [SerializeField] private AirplaneRefuel myAirplaneRefuel;
        [SerializeField] private AirplaneRepairSystem myAirplaneRepairSystem;

        [Space] [SerializeField] private GameObject airplaneParent;
        [SerializeField] private GameObject airplaneRoofParent;
        [SerializeField] private CinemachineVirtualCamera airplaneUpgradeCamera;
        [SerializeField] private GameObject stairs;

        [Header("Airplane visuals")]
        [SerializeField] private List<GameObject> seatsObjects;
        [SerializeField] private List<GameObject> airplaneSizeModels;
        [SerializeField] private List<GameObject> airplaneRoofModels;
        [SerializeField] private GameObject airplaneStairs;
        [SerializeField] private GameObject invisibleWall;
        
        [Header("Airplane timer")]
        [SerializeField] private GameObject timerParent;
        [SerializeField] private Image timerfiller;
        [SerializeField] private TextMeshProUGUI timerTmp;

        [Header("Animation Positions")] [SerializeField]
        private Transform airplaneDockedPos;

        [SerializeField] private Transform airplaneUndockedPos;
        [SerializeField] private Transform stairsDockedPos;
        [SerializeField] private Transform stairsUndockedPos;

        private bool _isPlaneDocked;
        private bool _isBroken;
        
        private int _boardedGuests;
        private int _ticketPrice;
        private int _boughtSeats;
        private int _guestsInSeat;
        private int _registeredTickets;

        private float _baseAirplaneInterval;
        private float _intervalTimer;

        private List<GuestSystem> _registeredGuests = new();
        private List<GuestSystem> _registeredWaitingGuests = new();
        private List<GuestSystem> _spawnedArrivalGuests = new();
        private List<Material> _roofMaterials = new();

        private AirportManager _airportManager;
        
        public int AirplaneLevel
        {
            get => GameManager.Save.LoadValue($"AirplaneLevel_{saveName}", 0);
            private set => GameManager.Save.SaveValueAndSync($"AirplaneLevel_{saveName}", value);
        }
        
        public int TicketPriceUpgradeLevel
        {
            get => GameManager.Save.LoadValue($"TicketPriceUpgradeLevel_{saveName}", 0);
            private set => GameManager.Save.SaveValueAndSync($"TicketPriceUpgradeLevel_{saveName}", value);
        }

        public int SeatsUpgradeLevel
        {
            get => GameManager.Save.LoadValue($"SeatsUpgradeLevel_{saveName}", 0);
            private set => GameManager.Save.SaveValueAndSync($"SeatsUpgradeLevel_{saveName}", value);
        }

        public int AllSeatsInPlane() => _boughtSeats;

        public int AvailableSeatsInPlane()
        {
            return _boughtSeats - _registeredTickets;
        }

        public int GiveTicketPrice() => _ticketPrice;

        private void Start()
        {
            MakeRoofsMaterialsCopy();
            RefreshAirplaneSeats();
            RefreshTicketPrice();
            _airportManager = AirportManager.instance;
            _baseAirplaneInterval = AirplanesManager.instance.AirplaneInterval;
            myLuggagesSetup.RegisterMyPlane(this);
            ShowPlane(true).Forget();

            if (myBuyArea.gameObject.activeInHierarchy)
            {
                myBuyArea.AddActionToAfterBuy(() => SpawnGuestAfterBuy().Forget());
            }
            else
            {
                SpawnGuestAfterBuy().Forget();
            }
        }

        private async UniTask SpawnGuestAfterBuy()
        {
            await UniTask.Delay(500);

            SpawnArrivalGuests();
            LetFreeAllArrivalGuests();
        }

        private void SpawnArrivalGuests()
        {
            for (int i = 0; i < _boughtSeats/2; i++)
            {
                var g = _airportManager.SpawnArrivalGuest(airplaneParent.transform, seatsObjects[i].transform);
                g.SetGuestType(GuestType.Arrival);
                g.ToggleAi(false, Vector3.zero);
                _airportManager.GiveArrivalGuestDestinationsList(g);
                _spawnedArrivalGuests.Add(g);
            }
        }

        private void LetFreeAllArrivalGuests()
        {
            foreach (var g in _spawnedArrivalGuests)
            {
                g.ToggleAi(true, g.transform.position);
                g.transform.parent = _airportManager.GiveArrivalGuestParent();
                g.transform.localScale = Vector3.one;
                g.PerformNextStep().Forget();
            }

            _spawnedArrivalGuests.Clear();
        }

        public void RegisterGuest(GuestSystem guest)
        {
            if (_registeredGuests.Count < _boughtSeats)
            {
                _registeredGuests.Add(guest);
            }
            else
            {
                _registeredWaitingGuests.Add(guest);
            }

            guest.SetGuestAirplane(this);
        }

        public void UnregisterGuest(GuestSystem guest, bool isWaitingGuest)
        {
            if (!isWaitingGuest)
            {
                _registeredGuests.RemoveAt(_registeredGuests.IndexOf(guest));
            }
            else
            {
                _registeredWaitingGuests.RemoveAt(_registeredWaitingGuests.IndexOf(guest));
            }
            
            AirplanesManager.instance.RefreshTicketTv();
        }

        public void RegisterTicketBuy(GuestSystem guest, bool forTutorial = false)
        {
            if (guest.IsCriminal()) return;
            _registeredTickets++;
            myLuggagesSetup.MakeNewLuggage(forTutorial).Forget();
        }

        public void RegisterGuestInSeat()
        {
            _guestsInSeat++;
            if (_guestsInSeat < _boughtSeats) return;
            TryToStartPlane();
        }

        public void TryToStartPlane()
        {
            if (!myAirplaneRefuel.IsAirplaneFueled()) return;
            if (!myLuggagesSetup.AllLuggagesOnPlane()) return;
            if (_guestsInSeat < _boughtSeats) return;
            //Here we can check in loop if player is in airplane
            DisablePlane().Forget();
        }

        public Transform ShowCorrectSeatPosition(Guest.GuestSystem g)
        {
            var guestIndex = _registeredGuests.IndexOf(g);
            _boardedGuests++;
            if (_boardedGuests >= AllSeatsInPlane()) EnableInvisibleWall();
            return seatsObjects[guestIndex].transform;
        }

        private async UniTask ShowPlane(bool atStart = false)
        {
            if (!atStart)
            {
                _boardedGuests = 0;
                _guestsInSeat = 0;
                _registeredTickets = 0;
                myLuggagesSetup.ResetLuggagesInPlane();
                DisableInvisibleWall();
                _registeredGuests.Clear();

                if (!_isBroken)
                {
                    while (_registeredGuests.Count < _boughtSeats && _registeredWaitingGuests.Count > 0)
                    {
                        var guest = _registeredWaitingGuests[0];
                        _registeredWaitingGuests.RemoveAt(0);
                        _registeredGuests.Add(guest);
                    
                        guest.SwitchToNormalPassenger().Forget();
                        RegisterTicketBuy(guest);
                    }
                }
                
                airplaneParent.SetActive(true);

                SpawnArrivalGuests();
                airplaneParent.transform.DOMove(airplaneDockedPos.position, 3);
                await UniTask.Delay(2500);
                _roofMaterials[SeatsUpgradeLevel].DOFade(0, 0.7f);
                await UniTask.Delay(500);
                stairs.transform.DOMove(stairsDockedPos.position, 1.5f);
            }
            else
            {
                _roofMaterials[SeatsUpgradeLevel].DOFade(0, 1f);
            }

            _isPlaneDocked = true;
            if (myUpgradeTrigger.IsPlayerInsideUpgradeTrigger()) AirplaneUpgradeUi.instance.TryToRefreshUpgradeWindow();
            airplaneRoofParent.SetActive(false);
            
            
            if (!atStart) LetFreeAllArrivalGuests();

            AirplanesManager.instance.RefreshTicketTv();

            if (_isBroken)
            {
                myGate.ToggleInteractableVisibility(false);
                myGate.GateInteractionCirclePauseToggle(true);
                var upgradeTriggerActive = myUpgradeTrigger.gameObject.activeInHierarchy;
                if (upgradeTriggerActive) myUpgradeTrigger.gameObject.SetActive(false);

                await myAirplaneRepairSystem.StartRepair(this);
                
                while (_isBroken)
                {
                    await UniTask.Delay(250);
                }
                
                while (_registeredGuests.Count < _boughtSeats && _registeredWaitingGuests.Count > 0)
                {
                    var guest = _registeredWaitingGuests[0];
                    _registeredWaitingGuests.RemoveAt(0);
                    _registeredGuests.Add(guest);
                    
                    guest.SwitchToNormalPassenger().Forget();
                    RegisterTicketBuy(guest);
                }
                
                if (upgradeTriggerActive) myUpgradeTrigger.gameObject.SetActive(true);
                myGate.ToggleInteractableVisibility(true);
                myGate.GateInteractionCirclePauseToggle(false);
            }
            
            myAirplaneRefuel.MakeAirplaneFuelTankEmpty();
        }

        private async UniTask DisablePlane()
        {
            _isPlaneDocked = false;
            if (myUpgradeTrigger.IsPlayerInsideUpgradeTrigger()) AirplaneUpgradeUi.instance.TryToRefreshUpgradeWindow();

            airplaneRoofParent.SetActive(true);
            _roofMaterials[SeatsUpgradeLevel].DOFade(0, 0);
            await _roofMaterials[SeatsUpgradeLevel].DOFade(1, 1f);

            foreach (var guest in _registeredGuests) Destroy(guest.gameObject);

            await stairs.transform.DOMove(stairsUndockedPos.position, 1.5f);
            await airplaneParent.transform.DOMove(airplaneUndockedPos.position, 3);
            airplaneParent.SetActive(false);

            _intervalTimer = _baseAirplaneInterval;
            var s = Mathf.FloorToInt(_intervalTimer % 60);
            timerTmp.text = $"{s}";
            timerParent.transform.localScale = Vector3.zero;
            timerParent.SetActive(true);
            timerParent.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            while (_intervalTimer > 0)
            {
                var seconds = Mathf.FloorToInt(_intervalTimer % 60);
                timerTmp.text = $"{seconds}";
                timerfiller.fillAmount = _intervalTimer / _baseAirplaneInterval;

                _intervalTimer -= Time.deltaTime;
                await UniTask.Yield();
            }

            ShowPlane().Forget();

            timerTmp.text = $"0";
            await UniTask.Delay(500);
            await timerParent.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
            timerParent.gameObject.SetActive(false);
        }

        private void RefreshAirplaneSeats()
        {
            _boughtSeats = AirplanesManager.instance.GiveSeatsUpgradeSetting(SeatsUpgradeLevel).upgradeValue;
            for (var i = 0; i < seatsObjects.Count; i++) seatsObjects[i].SetActive(i <= _boughtSeats - 1);
            for (var i = 0; i < airplaneSizeModels.Count; i++) airplaneSizeModels[i].SetActive(i == SeatsUpgradeLevel);
            for (var i = 0; i < airplaneRoofModels.Count; i++) airplaneRoofModels[i].SetActive(i == SeatsUpgradeLevel);
            myLuggagesSetup.SetNewStats(_boughtSeats);
        }

        private void RefreshTicketPrice() => _ticketPrice =
            AirplanesManager.instance.GiveTicketUpgradeSetting(TicketPriceUpgradeLevel).upgradeValue;

        public bool TryToShowUpgradeTrigger(bool cachedIndex)
        {
            if (cachedIndex) return true;

            myUpgradeTrigger.gameObject.SetActive(true);
            return false;
        }
        
        public void MakeUpgrade(UpgradeType type)
        {
            BuyAreaUnlockManager.instance.RegisterAirplaneUpgradeBought();
            AirplaneLevel++;

            AirplaneUpgradeUi.instance.HideUpgradeUi().Forget();
            myUpgradeTrigger.gameObject.SetActive(false);
            
            switch (type)
            {
                case UpgradeType.TicketPrice:
                    UpgradeTicketsLevel();
                    break;
                case UpgradeType.Seats:
                    UpgradeSeatsLevel();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            
            SDKManager.MiniLevelCompleted();
            SDKManager.MiniLevelStarted();
        }
        
        public void UpgradeTicketsLevel()
        {
            TicketPriceUpgradeLevel++;
            RefreshTicketPrice();
            myUpgradeTrigger.SetUpgradePrices();
        }

        public void UpgradeSeatsLevel()
        {
            SeatsUpgradeLevel++;
            RefreshAirplaneSeats();
            
            while (_registeredGuests.Count < _boughtSeats && _registeredWaitingGuests.Count > 0)
            {
                var guest = _registeredWaitingGuests[0];
                _registeredWaitingGuests.RemoveAt(0);
                _registeredGuests.Add(guest);
                    
                guest.SwitchToNormalPassenger().Forget();
                RegisterTicketBuy(guest);
            }
            
            AirplanesManager.instance.RefreshTicketTv();
            myUpgradeTrigger.SetUpgradePrices();
        }

        private void MakeRoofsMaterialsCopy()
        {
            foreach (var roofModel in airplaneRoofModels)
            {
                var renderer = roofModel.GetComponent<Renderer>();
                if (!renderer) continue;
                var materialCopy = new Material(renderer.material);
                _roofMaterials.Add(materialCopy);
                renderer.material = materialCopy;
            }
        }

        public void ShowUpgradeCamera()
        {
            airplaneUpgradeCamera.m_Priority = 11;
            PlayerManager.instance.FocusToCameraAndLockPlayer(airplaneUpgradeCamera);
        }

        public bool CheckIfGuestIsOnRegisteredList(Guest.GuestSystem g) => _registeredGuests.Contains(g);
        public void DisableUpgradeCamera() => airplaneUpgradeCamera.m_Priority = 0;
        public bool IsPlaneDocked() => _isPlaneDocked;
        public AirplaneGate GiveMeAirplaneGate() => myGate;
        private void EnableInvisibleWall() => invisibleWall.SetActive(true);
        private void DisableInvisibleWall() => invisibleWall.SetActive(false);

        public GameObject UpgradeTriggerObject() => myUpgradeTrigger.gameObject;

        public void SetAirplaneAsBroken(int numberOfPartsToBroke)
        {
            _isBroken = true;
            myAirplaneRepairSystem.SetNumberOfPartsToRepair(numberOfPartsToBroke);
        }

        public void RepairWholeAirplane() => _isBroken = false;
    }
}