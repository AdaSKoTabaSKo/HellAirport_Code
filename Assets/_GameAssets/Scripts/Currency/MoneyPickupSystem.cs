using System.Collections.Generic;
using System.Linq;
using _GameAssets.Scripts.Core;
using UnityEngine;

namespace _GameAssets.Scripts.Currency
{
    public class MoneyPickupSystem : SingleMonoBehaviour<MoneyPickupSystem>
    {
        [SerializeField] private MoneyPickUp moneyPickupPrefab;

        private List<MoneyPickUp> _spawnedMoney = new List<MoneyPickUp>();

        public MoneyPickUp GiveAvailableMoneyPickup()
        {
            foreach (var mpu in _spawnedMoney.Where(mpu => !mpu.gameObject.activeInHierarchy))
                return mpu;

            var p = Instantiate(moneyPickupPrefab, transform);
            p.gameObject.SetActive(false);
            _spawnedMoney.Add(p);
            return p;
        }
    }
}