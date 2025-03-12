using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Player;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GameAssets.Scripts.Planes
{
    public class AirplaneLuggagesResetTrigger : MonoBehaviour
    {
        [SerializeField] private ItemType acceptingItemType;
        [SerializeField] private AirplaneLuggagePickupArea myLuggagePickupArea;
        
        private PlayerRefreshObjectSystem _player;
        
        void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerRefreshObjectSystem player)) return;
            if (!_player) _player = player;
            if (!_player.CheckIfHaveThisItemType(acceptingItemType)) return;
            TryGiveAsync().Forget();
        }
        
        private async UniTask TryGiveAsync()
        {
            if (!_player) _player = PlayerManager.instance.GivePlayerRefreshSystem();
            while (_player.CheckIfHaveThisItemType(acceptingItemType))
            {
                ResetAsync();
                Debug.Log("resetting luggage");
                await UniTask.Delay(50);
            }
        }
        
        private void ResetAsync()
        {
            var item = _player.GiveItemOfType(acceptingItemType);
            item.ResetParent();
            myLuggagePickupArea.MakeNewLuggage(item).Forget();
        }

        public async UniTask ForceToGiveEverytingBack() => await TryGiveAsync();
    }
}