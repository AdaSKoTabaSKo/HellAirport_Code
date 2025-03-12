using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _GameAssets.Scripts.WaitingArea
{
    public class WaitingArea : MonoBehaviour
    {
        [SerializeField] private List<WaitingSeat> myWaitingSeats;

        private int _seatsTaken;

        public bool IsThereFreeSeat() => _seatsTaken < myWaitingSeats.Count;
        public int AvailableSeats() => myWaitingSeats.Count - _seatsTaken;
        
        public WaitingSeat GiveFreeSeat()
        {
            foreach (var s in myWaitingSeats.Where(s => s.IsSeatFree())) return s;

            Debug.LogError("There is no seat available in waitingArea");
            return null;
        }
        
        public void RegisterGuest(Guest.GuestSystem g)
        {
            _seatsTaken++;
            g.RegisterAsWaitingGuest(this);
        }

        public void UnregisterGuest() => _seatsTaken--;
    }
}