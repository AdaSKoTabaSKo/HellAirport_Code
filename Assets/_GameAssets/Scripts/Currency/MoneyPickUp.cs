using System;
using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Player;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GameAssets.Scripts.Currency
{
    public class MoneyPickUp : MonoBehaviour
    {
        private int _cashValue;

        private Rigidbody _myRigidbody;
        private PlayerPickupArea _player;

        private bool _canBePickedUp;
        
        private void Awake()
        {
            _myRigidbody = GetComponent<Rigidbody>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_canBePickedUp) return;
            if (!other.TryGetComponent(out PlayerPickupArea player)) return;
            if (!_player) _player = player;
            PickupMoney().Forget();
        }

        private async UniTask PickupMoney()
        {
            await transform.DOJump(_player.transform.position, 1, 1, 0.2f);
            CurrencyManager.instance.AddCoinsWithoutAnimation(_cashValue);
            gameObject.SetActive(false);
        }

        public async UniTask SpawnMe(int pickupValue, Vector3 spawnPoint, bool delayedPickup = false)
        {
            _canBePickedUp = !delayedPickup;
            
            gameObject.SetActive(true);
            _cashValue = pickupValue;
            transform.position = spawnPoint;
            
            _myRigidbody.AddForce(
                new Vector3(Random.Range(-1f, 1f), Random.Range(1f, 2f),
                    Random.Range(-1f, 1f)) * Random.Range(2f, 3f), ForceMode.Impulse);

            if (delayedPickup)
            {
                _canBePickedUp = false;
                await UniTask.Delay(500);
                _canBePickedUp = true;
            }
        }
    }
}