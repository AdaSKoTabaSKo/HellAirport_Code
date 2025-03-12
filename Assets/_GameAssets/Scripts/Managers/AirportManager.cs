using System;
using System.Collections.Generic;
using System.Linq;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.Guest;
using _GameAssets.Scripts.InteractionObjects;
using _GameAssets.Scripts.Planes;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace _GameAssets.Scripts.Managers
{
    public class AirportManager : SingleMonoBehaviour<AirportManager>
    {
        [FormerlySerializedAs("guestPrefab")]
        [Header("Guests Prefabs")]
        [SerializeField] private Guest.GuestSystem guestSystemPrefab;
        
        [Header("Main Interaction Objects")]
        [SerializeField] private List<TicketDesk> ticketDesks;
        [SerializeField] private List<SecurityGate> securityGates;
        [SerializeField] private BaggageClaimArea baggageClaimArea;
        
        [Header("Other Interaction Objects")]
        [SerializeField] private List<InteractionObject> otherInteractionObjects;

        [Header("Guests Spawns/Parents")]
        [SerializeField] private Transform departureGuestParent;
        [SerializeField] private Transform arrivalGuestParent;

        private bool _guestSpawnedInSecondDesk;
        
        public static int AirportExpandStatus
        {
            get => GameManager.Save.LoadValue("AirportExpandStatus", 1);
            private set => GameManager.Save.SaveValueAndSync("AirportExpandStatus", value);
        }

        protected override void Awake()
        {
            base.Awake();
            SDKManager.MissionStarted(AirportExpandStatus,$"Airport expand");
        }

        private void Start()
        {
            foreach (var td in ticketDesks)
            {
                td.RegisterToQueueOnFreeSpotEvent(()=>SpawnDepartureGuest(td, true).Forget());
            }
            
            SpawnDepartureGuestsAtStart().Forget();
        }

        public void SpawnDepartureGuestsForTutorial()
        {
            for (int i = 0; i < 4; i++)
            {
                var g = Instantiate(guestSystemPrefab, Vector3.zero.With(x: 4, z: 8), quaternion.identity, departureGuestParent);
                g.SetGuestType(GuestType.Tutorial);
                AirplanesManager.instance.RegisterTicketBuy(g).Forget();
                g.GiveGuestAirplane().RegisterTicketBuy(g, true);
                AirplanesManager.instance.RefreshTicketTv();
                g.PerformNextStep().Forget();
            }
        }
        
        private async UniTask SpawnDepartureGuestsAtStart()
        {
            await UniTask.DelayFrame(60);
            
            for (int i = 0; i < ticketDesks[0].GetNumberOfQueueSpots(); i++)
            {
                SpawnDepartureGuest(ticketDesks[0],false).Forget();
                await UniTask.Delay((int)Random.Range(1f, 3f) * 1000);
            }
        }

        public void ProxyToSpawnGuestAfterTicketDeskBuy(TicketDesk td) => SpawnDepartureAfterDeskBuy(td).Forget();

        private async UniTask SpawnDepartureAfterDeskBuy(TicketDesk td)
        {
            Debug.Log($"Respie gosci w drugiej kasie");
            
            if (_guestSpawnedInSecondDesk) return;
            _guestSpawnedInSecondDesk = true;
            
            await UniTask.DelayFrame(60);
            
            for (int i = 0; i < td.GetNumberOfQueueSpots(); i++)
            {
                SpawnDepartureGuest(td,false).Forget();
                await UniTask.Delay((int)Random.Range(1f, 3f) * 1000);
            }
        }
        
        private async UniTask SpawnDepartureGuest(TicketDesk td, bool withDelay)
        {
            var g = Instantiate(guestSystemPrefab, departureGuestParent.position, quaternion.identity, departureGuestParent);
            td.AddNewGuest(g);
            g.SetGuestType(GuestType.Departure);
            if(withDelay) await UniTask.Delay((int)Random.Range(1f, 3f) * 1000);
            var ranGate = securityGates[Random.Range(0, securityGates.Count(gate => gate.gameObject.activeInHierarchy))];
            await UniTask.Delay(1000);
            g.SetDestinationsList(CreateRandomizedDestinationList(), ranGate);
        }
        
        public Guest.GuestSystem SpawnArrivalGuest(Transform parent, Transform seatPos)
        {
            var g = Instantiate(guestSystemPrefab, seatPos.position, Quaternion.LookRotation(seatPos.forward), parent);
            baggageClaimArea.TakeLuggage(g.GiveLuggage(false));
            return g;
        }
        
        public void GiveArrivalGuestDestinationsList(Guest.GuestSystem g) => g.SetDestinationsList(CreateRandomizedDestinationList(), baggageClaimArea);
        public Transform GiveArrivalGuestParent() => arrivalGuestParent;
        
        private List<InteractionObject> CreateRandomizedDestinationList()
        {
            var randomizedList = new List<InteractionObject>();

            foreach (var obj in otherInteractionObjects)
            {
                if (!obj.gameObject.activeInHierarchy) continue;
                if (Random.Range(0, 1001) > 500)
                {
                    randomizedList.Add(obj);
                }
            }

            return randomizedList;
        }

        public void RegisterAirportExpand()
        {
            SDKManager.MissionComplete(AirportExpandStatus,$"Airport expand");
            AirportExpandStatus++;
            GameManager.BoughtStuff = 1;
            SDKManager.MissionStarted(AirportExpandStatus,$"Airport expand");
        }
    }
}