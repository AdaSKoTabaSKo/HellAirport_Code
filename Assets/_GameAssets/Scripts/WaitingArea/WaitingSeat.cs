using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GameAssets.Scripts.WaitingArea
{
    public class WaitingSeat : MonoBehaviour
    {
        [SerializeField] private WaitingArea myWaitingArea;
        private bool _occupied;

        public bool IsSeatFree() => !_occupied;

        public void Occupy() => _occupied = true;

        public void Exit()
        {
            _occupied = false;
            myWaitingArea.UnregisterGuest();
        }
    }
}