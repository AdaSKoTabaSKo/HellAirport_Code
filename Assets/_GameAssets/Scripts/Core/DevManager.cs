using System;
using _GameAssets.Scripts.Currency;
using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Player;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GameAssets.Scripts.Core
{
    public class DevManager : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha0)) LevelManager.instance.AddXpForHax();
            if (Input.GetKeyDown(KeyCode.Alpha9)) CurrencyManager.instance.AddCoinsWithoutAnimation(100);
            if (Input.GetKeyDown(KeyCode.Alpha8)) SpawnPickedUpMoney().Forget();
        }

        private async UniTask SpawnPickedUpMoney()
        {
            for (var i = 0; i < 10; i++)
            {
                MoneyPickupSystem.instance.GiveAvailableMoneyPickup().SpawnMe(5, PlayerManager.instance.transform.position + Vector3.zero.With(y: 0.5f), true).Forget();
                await UniTask.Delay(10);
            } 
        }
    }
}