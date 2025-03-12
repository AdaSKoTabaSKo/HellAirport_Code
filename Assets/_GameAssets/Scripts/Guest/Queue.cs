using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _GameAssets.Scripts.Guest
{
    public class Queue : MonoBehaviour
    {
        [SerializeField] private List<Transform> positionTransforms;

        [SerializeField] private bool moveThroughLastPosition;
        
        [ShowInInspector]
        private List<GuestSystem> _myGuests = new List<GuestSystem>();
        private List<Vector3> _myPositions = new List<Vector3>();
        private Vector3 entrancePosition;

        private GuestSystem _currentGuestSystem;
        
        private bool _isGuestAtFront;
        
        public event Action OnGuestAdded;
        public event Action OnGuestAtFront;
        public event Action OnFreeSpotInQueue;
        
        private void Awake()
        {
            foreach (var p in positionTransforms)
            {
                _myPositions.Add(p.position);
            }

            entrancePosition = _myPositions[^1];
            OnGuestAtFront += () => _isGuestAtFront = true;

        }

        public bool IsGuestAtFront() => _isGuestAtFront;
        
        public bool CanAddGuests() => _myGuests.Count < _myPositions.Count;
        public int GetNumberOfQueuePoints() => _myPositions.Count;

        public Vector3 GetPosOfFirstPoint() => _myPositions[0];
        
        public async UniTask AddGuestToQueue(GuestSystem g)
        {
            _myGuests.Add(g);
            var curIndx = _myGuests.IndexOf(g);
            OnGuestAdded?.Invoke();

            if (moveThroughLastPosition) await g.SetNewDestinationWithWaiting(entrancePosition, Vector3.zero);

            var indx = _myGuests.IndexOf(g);
            await g.SetNewDestinationWithWaiting(_myPositions[indx], positionTransforms[indx].forward);

            if (curIndx != 0) return;
            OnGuestAtFront?.Invoke();
        }

        public GuestSystem GetFirstInQueue()
        {
            if (_myGuests.Count == 0) return null;
            _currentGuestSystem = _myGuests[0];
            return _currentGuestSystem;
        }
        
        public void ReleaseFirstInQueue()
        {
            if (_myGuests.Count == 0) return;

            _myGuests.RemoveAt(_currentGuestSystem == null ? 0 : _myGuests.IndexOf(_currentGuestSystem));

            RelocateAllGuests();
            _isGuestAtFront = false;
        }

        private void RelocateAllGuests()
        {
            for (int i = 0; i < _myGuests.Count; i++)
            {
                if(i==0) _myGuests[i].SetNewDestinationWithWaitingAndAction(_myPositions[i], OnGuestAtFront).Forget();
                else _myGuests[i].SetNewDestination(_myPositions[i]);
            }

            _currentGuestSystem = null;
            OnFreeSpotInQueue?.Invoke();
        }

    }
}