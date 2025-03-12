using System;
using System.Collections.Generic;
using System.Linq;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.InteractionObjects;
using _GameAssets.Scripts.Planes;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GameAssets.Scripts.Managers
{
    public class AirplanesManager : SingleMonoBehaviour<AirplanesManager>
    {
        [SerializeField] private List<Airplane> airplanes;
        [SerializeField] private List<WaitingArea.WaitingArea> waitingAreas;
        
        [Header("Ticket Tv")]
        [SerializeField] private TicketTv ticketTv;
        
        [Header("Upgrades")]
        [SerializeField] private List<UpgradeSetting> ticketUpgrades;
        [SerializeField] private List<UpgradeSetting> seatsUpgrades;
        [SerializeField] private List<UpgradeType> upgradesOrder;
        
        [SerializeField] private float priceMultiplier;
        [SerializeField] private float airplaneInterval;
        
        
        private void Start()
        {
            if(airplanes.Count == 0) Debug.LogError("There is no plane attatched in Planes Manager!!!!");
            
            ticketTv.RefreshTvScreenUi(airplanes, true).Forget();
        }

        public UpgradeSetting GiveTicketUpgradeSetting(int index) => ticketUpgrades[index];
        public UpgradeSetting GiveSeatsUpgradeSetting(int index) => seatsUpgrades[index];
        public UpgradeType GiveCurrentUpgradeType(int index) => upgradesOrder[index];
        public bool IsAirplaneUpgradedToMax(int level) => level >= upgradesOrder.Count;
        
        public bool CanSellTicket() => airplanes.Where(a => a.gameObject.activeInHierarchy).Any(a => a.AvailableSeatsInPlane() > 0)
                                       || waitingAreas.Where(w => w.gameObject.activeInHierarchy).Any(w => w.IsThereFreeSeat());

        public int GiveTicketsPool() => airplanes.Where(a => a.gameObject.activeInHierarchy).Sum(a => a.AvailableSeatsInPlane()) 
                                        + waitingAreas.Where(w => w.gameObject.activeInHierarchy).Sum(w => w.AvailableSeats());

        public async UniTask RegisterTicketBuy(Guest.GuestSystem g)
        {
            var done = false;
            
            while (!done)
            {
                var eligibleAirplanes = airplanes.Where(a => a.gameObject.activeInHierarchy && a.AvailableSeatsInPlane()>0).ToList();

                if (eligibleAirplanes.Count > 0)
                {
                    var selectedAirplane = eligibleAirplanes[Random.Range(0, eligibleAirplanes.Count)];
                    selectedAirplane.RegisterGuest(g);
                    done = true;
                }
                else
                {
                    var eligibleWaitingAreas = waitingAreas.Where(a => a.gameObject.activeInHierarchy && a.AvailableSeats()>0).ToList();
                
                    if (eligibleWaitingAreas.Count > 0)
                    {
                        var availableAirplanes = airplanes.Where(a => a.gameObject.activeInHierarchy).ToList();
                        availableAirplanes[Random.Range(0, availableAirplanes.Count)].RegisterGuest(g);
                        
                        eligibleWaitingAreas[Random.Range(0, eligibleWaitingAreas.Count)].RegisterGuest(g);
                        done = true;
                    }
                }

                if(!done) await UniTask.Delay(500);
            }
            
        }

        public float GivePriceMultiplier(Airplane p) => Math.Max(1, airplanes.IndexOf(p) * priceMultiplier); 
        public List<UpgradeSetting> TicketUpgrades => ticketUpgrades;
        public List<UpgradeSetting> SeatsUpgrades => seatsUpgrades;
        public List<Airplane> GiveListOfAllAirplanes => airplanes;
        public float AirplaneInterval => airplaneInterval;
        
        public void RefreshTicketTv() => ticketTv.RefreshTvScreenUi(airplanes).Forget();
        
        [Serializable]
        public class UpgradeSetting
        {
            public int upgradePrice;
            public int upgradeValue;
        }
    }

    public enum UpgradeType
    {
        TicketPrice,
        Seats,
    }
}