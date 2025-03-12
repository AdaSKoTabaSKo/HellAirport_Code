using _GameAssets.Scripts.Player;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GameAssets.Scripts.Items
{
    public class ItemGarbageArea : MonoBehaviour
    {
        private bool _IsPlayerStanding => !PlayerManager.instance.playerMoving;
        private PlayerRefreshObjectSystem _player;
        private bool _isPlayerInside;

        void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerRefreshObjectSystem player)) return;
            if (!_player) _player = player;
            _isPlayerInside = true;
            TryRemoveAsync().Forget();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out PlayerRefreshObjectSystem player)) return;
            _isPlayerInside = false;
        }

        private async UniTask TryRemoveAsync()
        {
            while (_isPlayerInside)
            {
                if(_IsPlayerStanding)
                {
                    _player.RemoveAllItem().Forget();
                    return;
                }

                await UniTask.Yield();
            }
        }
    }
}