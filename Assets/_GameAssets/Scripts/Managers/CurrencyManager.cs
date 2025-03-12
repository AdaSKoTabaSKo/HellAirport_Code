using System;
using System.Collections.Generic;
using System.Linq;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.Player;
using CrazyLabsExtension;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoreMountains.NiceVibrations;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GameAssets.Scripts.Managers
{
    /// <summary>
    /// The CurrencyManager class is responsible for managing the in-game currency.
    /// It handles adding and spending coins, checking if there is enough cash available, and updating the cash bar.
    /// </summary>
    public class CurrencyManager : SingleMonoBehaviour<CurrencyManager>
    {
        public GameObject coinModel;
        [SerializeField] private Transform coinBarParticlePosition;
        
        private List<GameObject> cashSpawnedModel = new List<GameObject>();
        
        [Header("Ui")] [SerializeField]
        private TextMeshProUGUI cashValueTmp;
        
        public event Action OnCoinUpdate;

        private float _refreshBarTimer;
        private bool _delayRefreshCoinBarStarted;
        
        private void Start()
        {
            for (int i = 0; i < 96; i++)
            {
                var spwnd = Instantiate(coinModel, transform);
                cashSpawnedModel.Add(spwnd);
                spwnd.transform.localScale = Vector3.zero;
                spwnd.SetActive(false);
            }
            RefreshCashBar();
        }

        /// <summary>
        /// Adds coins to the player's cash and triggers a coin animation from the specified start position to the specified end position.
        /// </summary>
        /// <param name="howMuch">The amount of coins to add.</param>
        /// <param name="startPos">The starting position of the coin animation.</param>
        /// <param name="finalPos">The ending position of the coin animation.</param>
        /// <returns>A UniTask representing the asynchronous operation.</returns>
        public async UniTask AddCoins(int howMuch, Vector3 startPos, Vector3 finalPos, float secondsDelay = 0, bool enableHaptics = true)
        {
            Cash += howMuch;
            CashLifetimeStat += howMuch;
            await MoveParticle(startPos, finalPos, true, secondsDelay);

            _refreshBarTimer = 1f;
            if (!_delayRefreshCoinBarStarted) DelayedBarRefresh(enableHaptics).Forget();
        }
        
        private async UniTask DelayedBarRefresh(bool enableHaptics)
        {
            _delayRefreshCoinBarStarted = true;
            
            while (_refreshBarTimer > 0)
            {
                _refreshBarTimer -= Time.deltaTime;
                await UniTask.Yield();
            }
            
            if(enableHaptics) HapticFeedbackController.TriggerHaptics(HapticTypes.LightImpact);
            RefreshCashBar();
            
            _delayRefreshCoinBarStarted = false;
        }
        
        public void AddCoinsWithoutAnimation(int howMuch)
        {
            Cash += howMuch;
            CashLifetimeStat += howMuch;
            RefreshCashBar();
        }
        
        public void SpendCoins(int howMuch)
        {
            Cash -= howMuch;
            RefreshCashBar();
        }
        /// <summary>
        /// Spends the specified amount of coins and moves particles accordingly.
        /// </summary>
        /// <param name="howMuch">The amount of coins to spend.</param>
        /// <param name="startPos">The starting position of the particles.</param>
        /// <param name="finalPos">The final position of the particles.</param>
        public async UniTask SpendCoinsWithParticles(int howMuch, Vector3 startPos, Vector3 finalPos)
        {
            Cash -= howMuch;
            await MoveParticle(startPos, finalPos, false);
            RefreshCashBar();
        }

        /// <summary>
        /// Determines whether the player can spend the specified amount of cash.
        /// </summary>
        /// <param name="howMuchToSpend">The amount of cash to spend.</param>
        /// <returns>true if the player can spend the cash; otherwise, false.</returns>
        public bool CanSpendCash(int howMuchToSpend) => Cash - howMuchToSpend >= 0;

        /// <summary>
        /// Move a particle from the start position to the final position, either towards the player or away from the player.
        /// </summary>
        /// <param name="startPos">The starting position of the particle.</param>
        /// <param name="finalPos">The final position of the particle.</param>
        /// <param name="toPlayer">True if the particle should move towards the player, false if it should move away from the player.</param>
        /// <returns>The asynchronous task representing the particle movement.</returns>
        private async UniTask MoveParticle(Vector3 startPos, Vector3 finalPos, bool toPlayer, float animationDelay = 0)
        {
            GameObject obj = cashSpawnedModel.FirstOrDefault(c => !c.activeInHierarchy);

            if (obj == null)
            {
                var spwnd = Instantiate(coinModel, transform);
                cashSpawnedModel.Add(spwnd);
                spwnd.transform.localScale = Vector3.zero;
                obj = spwnd;
            }
            
            obj.transform.position = startPos;
            obj.SetActive(true);
            obj.transform.DORotateQuaternion(Random.rotation, 0.6f);

            if (toPlayer)
            {
                obj.transform.DOScale(1,0.3f).SetEase(Ease.OutBack);
                await obj.transform.DOMove(obj.transform.position + (Random.insideUnitSphere * 0.25f).With(y:2), 0.25f + animationDelay);
                await UniTask.Delay(25);
                await obj.transform.DOJump(PlayerManager.instance.transform.position.With(y: 1), 1,1, 0.25f).SetEase(Ease.InQuint);
                await obj.transform.DOScale(Vector3.zero, 0.1f);
            }
            else
            {
                obj.transform.DOScale(Vector3.one, 0.2f);
                obj.transform.DOJump(finalPos, Random.Range(1.5f, 3f), 1, 0.5f);
                await UniTask.Delay(400);
                await obj.transform.DOScale(Vector3.zero, 0.2f);
            }
            
            obj.SetActive(false);
        }

        /// <summary>
        /// Refreshes the cash bar UI element with the current cash value.
        /// </summary>
        private void RefreshCashBar()
        {
            cashValueTmp.text = $"{Cash}";
            OnCoinUpdate?.Invoke();
        }
        
        //~~~~~//
        //SAVES//
        //~~~~~//
        
        public static int Cash
        {
            get => GameManager.Save.LoadValue("Cash", 0);
            private set => GameManager.Save.SaveValueAndSync("Cash", value);
        }
        
        public static int CashLifetimeStat
        {
            get => GameManager.Save.LoadValue("CashLifetimeStat", 0);
            private set => GameManager.Save.SaveValueAndSync("CashLifetimeStat", value);
        }
    }
}