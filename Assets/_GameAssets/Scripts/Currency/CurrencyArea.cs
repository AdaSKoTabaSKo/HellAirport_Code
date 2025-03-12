using System;
using System.Collections.Generic;
using System.Linq;
using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Player;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GameAssets.Scripts.Currency
{
    public class CurrencyArea : MonoBehaviour
    {
        [Header("IMPORTANT!")] [SerializeField]
        private string triggerSaveName;

        [Header("Cash Models")] [SerializeField]
        private List<GameObject> cashModelsList;

        private bool _isPlayerInside;
        private bool _cashIsAvailable;

        private CurrencyManager _currencyManager;

        private Vector3 startingPos;

        private int CashPool
        {
            get => GameManager.Save.LoadValue($"CashPool_{triggerSaveName}", 0);
            set => GameManager.Save.SaveValueAndSync($"CashPool_{triggerSaveName}", value);
        }

        private void Start()
        {
            DelayedStart().Forget();
        }

        private async UniTask DelayedStart()
        {
            await UniTask.DelayFrame(2);

            startingPos = transform.position;
            _currencyManager = CurrencyManager.instance;

            foreach (var c in cashModelsList) c.SetActive(false);

            if (CashPool > 0) _cashIsAvailable = true;

            ActivateMoneyVisually(true);
        }

        public void AddCash(int value)
        {
            CashPool += value;
            _cashIsAvailable = true;
            ActivateMoneyVisually(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerManager player)) return;
            _isPlayerInside = true;
            TryTakeAsync().Forget();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out PlayerManager player)) return;
            _isPlayerInside = false;
        }

        private async UniTask TryTakeAsync()
        {
            while (_isPlayerInside)
            {
                await TakeAsync();
                await UniTask.Delay(100);
            }
        }

        private async UniTask TakeAsync()
        {
            if (!_cashIsAvailable) return;

            var coinsToGive = CashPool;
            CashPool = 0;
            _cashIsAvailable = false;

            var playerPos = PlayerManager.instance.transform.position;


            var activeCount = cashModelsList.Count(model => model.activeInHierarchy);

            for (int i = 0; i < activeCount; i++)
            {
                DelayedCashWithAnimation(PosOfLastModel(), playerPos).Forget();
                await UniTask.Yield();
            }

            coinsToGive -= activeCount;

            DelayCash(coinsToGive).Forget();

            if (CashPool <= 0) _cashIsAvailable = false;
        }

        private async UniTask DelayedCashWithAnimation(Vector3 cashPos, Vector3 playerPos)
        {
            await UniTask.Delay(5);
            _currencyManager.AddCoins(1, cashPos, playerPos, Random.Range(0.0f, 0.3f))
                .Forget();
        }

        private async UniTask DelayCash(int amount)
        {
            await UniTask.Delay(50);
            _currencyManager.AddCoinsWithoutAnimation(amount);
        }

        private void ActivateMoneyVisually(bool atStart)
        {
            var visualPool = Math.Clamp(CashPool, 0, cashModelsList.Count);
            
            if (atStart)
            {
                foreach (var c in cashModelsList) c.SetActive(false);
                for (int i = 0; i < visualPool; i++) cashModelsList[i].SetActive(true);
            }

            if (CashPool <= visualPool)
            {
                for (int i = 0; i < visualPool; i++)
                {
                    if (cashModelsList[i].activeInHierarchy) continue;
                    cashModelsList[i].SetActive(true);
                    cashModelsList[i].transform.DOJump(cashModelsList[i].transform.position, 1, 1, 0.2f);
                }
            }
            else
            {
                
                transform.DOJump(startingPos, 1, 1, .2f);
            }
        }

        private Vector3 PosOfLastModel()
        {
            var lastIndex = cashModelsList.FindLastIndex(model => model.activeInHierarchy);

            if (lastIndex != -1)
            {
                cashModelsList[lastIndex].SetActive(false);
                return cashModelsList[lastIndex].transform.position;
            }

            return Vector3.zero;
        }
    }
}