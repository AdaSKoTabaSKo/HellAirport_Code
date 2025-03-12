using System;
using System.Collections.Generic;
using System.Resources;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Player;
using CrazyLabsExtension;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoreMountains.NiceVibrations;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _GameAssets.Scripts.InteractionObjects
{
    public class BuyArea : MonoBehaviour
    {
        [Header("IMPORTANT!")] [SerializeField]
        private string triggerSaveName;

        [SerializeField] private bool setTrueIfThisIsFree;

        [Header("Ui")]
        [SerializeField] private TextMeshPro cashTmp;
        [SerializeField] private TextMeshPro cashExpandTmp;
        [SerializeField] private GameObject uiHandler;
        [SerializeField] private GameObject normalState;
        [SerializeField] private GameObject expandState;
        [SerializeField] private GameObject lockedState;
        [SerializeField] private TextMeshPro levelUnlockTmp;

        [Header("Xp Ui")]
        [SerializeField] private GameObject xpUiObject;
        [SerializeField] private TextMeshPro xpValueTmp;
        
        [Space] [Header("Item Settings")]
        [SerializeField] private bool isTutorial;
        [SerializeField] private int itemPrice;
        [SerializeField] private bool lockedBehindLevel;
        [ShowIf(nameof(lockedBehindLevel))][SerializeField] private int unlockAtLevel;
        [ShowIf(nameof(lockedBehindLevel))][SerializeField] private bool sendAsFirebaseEvent = true;
        [SerializeField] private ItemBuildTypeEnum buildType;
        
        [HideIf("buildType", ItemBuildTypeEnum.EventOnly)]
        [SerializeField] private GameObject mainItem;

        [ShowIf("buildType", ItemBuildTypeEnum.Change_Event)]
        [ShowIf("buildType", ItemBuildTypeEnum.Change)]
        [SerializeField] private GameObject itemToBeDisable; //wall for example

        [Space]
        [SerializeField] private UnityEvent actionAfterBuy;
        
        private bool _isPlayerInside;
        private bool _IsPlayerStanding => !PlayerManager.instance.playerMoving;
        private CurrencyManager _currencyManager;
        private bool _itemBought;
        private Vector3 startingPos;

        private BoxCollider _myCollider;

        private bool _wasInitialized;
        
        private int CashPayed
        {
            get => GameManager.Save.LoadValue($"CashPayed_{triggerSaveName}", 0);
            set => GameManager.Save.SaveValueAndSync($"CashPayed_{triggerSaveName}", value);
        }

        public void SetItemPrice(int newPrice) => itemPrice = newPrice;
        public void SetItemUnlockAt(int newLevel)
        {
            unlockAtLevel = newLevel;
            levelUnlockTmp.text = $"{unlockAtLevel}";
        }

        public bool IsItemBought() => _itemBought;

        private void Start()
        {
            if (setTrueIfThisIsFree)
            {
                Initialize();
            }
            
            if (!isTutorial) return;
            Initialize();
            //gameObject.SetActive(false);
        }

        public void Initialize()
        {
            _myCollider = GetComponent<BoxCollider>();
            _myCollider.size = (Vector3.one * 1.1f).With(lockedBehindLevel?2.25f:1.1f);
            startingPos = transform.position;
            
            if (CashPayed >= itemPrice)
            {
                MakePurchase(true);
            }
            else
            {
                ManageBuildTypeAtStart();
                RefreshUi();
            }
            
            if (lockedBehindLevel)
            {
                if (LevelManager.instance.CurrentPlayerLevel + 1 < unlockAtLevel)
                {
                    if (!_wasInitialized) LevelManager.instance.OnLevelUp += TryToUnlockArea;
                    _myCollider.enabled = false;
                    normalState.SetActive(false);
                    expandState.SetActive(false);
                    lockedState.SetActive(true);
                }
                else
                {
                    normalState.SetActive(false);
                    expandState.SetActive(true);
                    lockedState.SetActive(false);
                    if (_itemBought)
                    {
                        gameObject.SetActive(false);
                        return;
                    }
                    
                    TryToUnlockArea();
                }
            }

            _wasInitialized = true;
            
            _currencyManager = CurrencyManager.instance;
            if (_itemBought) gameObject.SetActive(false);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void TryToUnlockArea()
        {
            if (LevelManager.instance.CurrentPlayerLevel + 1 < unlockAtLevel) return;
            
            if (lockedBehindLevel)
            {
                expandState.SetActive(true);
                lockedState.SetActive(false);
            }
            else
            {
                normalState.SetActive(false);
            }
            
            lockedState.SetActive(false);
            _myCollider.enabled = true;

            if(_wasInitialized) BuyAreaUnlockManager.instance.ShowExpandBuyArea(this).Forget();
            LevelManager.instance.OnLevelUp -= TryToUnlockArea;
        }

        public void ManageBuildTypeAtStart()
        {
            switch (buildType)
            {
                case ItemBuildTypeEnum.Build:
                    mainItem.SetActive(false);
                    mainItem.transform.localScale = Vector3.one.With(y:0);
                    break;
                case ItemBuildTypeEnum.Disable:
                    break;
                case ItemBuildTypeEnum.Change:
                    mainItem.SetActive(false);
                    mainItem.transform.localScale = Vector3.one.With(y:0);
                    itemToBeDisable.SetActive(true);
                    //itemToBeDisable.transform.localScale = Vector3.one;
                    break;
                case ItemBuildTypeEnum.EventOnly:
                    break;
                case ItemBuildTypeEnum.Build_Event:
                    mainItem.SetActive(false);
                    mainItem.transform.localScale = Vector3.one.With(y:0);
                    break;
                case ItemBuildTypeEnum.Change_Event:
                    mainItem.SetActive(false);
                    mainItem.transform.localScale = Vector3.one.With(y:0);
                    itemToBeDisable.SetActive(true);
                    //itemToBeDisable.transform.localScale = Vector3.one;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerManager player)) return;
            _isPlayerInside = true;
            TryPurchaseAsync().Forget();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out PlayerManager player)) return;
            _isPlayerInside = false;
        }

        private async UniTask TryPurchaseAsync()
        {
            while (_isPlayerInside)
            {
                while (_IsPlayerStanding)
                {
                    await PurchaseAsync();
                    if (_itemBought) return;
                    if (!_currencyManager.CanSpendCash(1)) return;
                }

                await UniTask.Yield();
            }
        }

        private async UniTask PurchaseAsync()
        {
            if (_itemBought) return;

            if (CashPayed < itemPrice)
            {
                var paymentAmount = 1;
                
                if (itemPrice - CashPayed > 10 && _currencyManager.CanSpendCash(10)) paymentAmount = 10;
                
                if (_currencyManager.CanSpendCash(paymentAmount))
                {
                    _currencyManager.SpendCoinsWithParticles(paymentAmount, PlayerManager.instance.transform.position, startingPos).Forget();
                    CashPayed += paymentAmount;
                    RefreshUi();
                    if (CashPayed >= itemPrice) _itemBought = true;
                }
            }
            
            if (_itemBought)
            {
                MakePurchase();
                return;
            }

            await UniTask.Delay(50);
        }

        private void MakePurchase(bool instant = false)
        {
            ModifyItem(instant).Forget();
            uiHandler.transform.DOScale(0, 0.2f);
        }
        
        private void RefreshUi()
        {
            if (!lockedBehindLevel) cashTmp.text = $"{itemPrice - CashPayed}";
            else cashExpandTmp.text = $"{itemPrice - CashPayed}";
        }

        private async UniTask ModifyItem(bool instant)
        {
            var time = instant ? 0 : 0.3f;
            switch (buildType)
            {
                case ItemBuildTypeEnum.Build:
                    mainItem.SetActive(true);
                    mainItem.transform.DOScale(1, time).SetEase(Ease.OutBack);
                    break;
                case ItemBuildTypeEnum.Disable:
                    mainItem.transform.DOScale(0, time).SetEase(Ease.InBack)
                        .OnComplete(() => mainItem.SetActive(false));
                    break;
                case ItemBuildTypeEnum.Change:
                    mainItem.SetActive(true);
                    itemToBeDisable.transform.DOScaleY(0, time).SetEase(Ease.OutBack);
                    await mainItem.transform.DOScaleY(1, time).SetEase(Ease.InBack);
                    itemToBeDisable.SetActive(false);
                    break;
                case ItemBuildTypeEnum.EventOnly:
                    break;
                case ItemBuildTypeEnum.Build_Event:
                    mainItem.SetActive(true);
                    mainItem.transform.DOScale(1, time).SetEase(Ease.OutBack);
                    break;
                case ItemBuildTypeEnum.Change_Event:
                    mainItem.SetActive(true);
                    itemToBeDisable.transform.DOScaleY(0, time).SetEase(Ease.OutBack);
                    await mainItem.transform.DOScaleY(1, time).SetEase(Ease.InBack);
                    itemToBeDisable.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!instant)
            {
                AudioManager.instance.PlaySound(SoundType.BuyArea);
                HapticFeedbackController.TriggerHaptics(HapticTypes.MediumImpact);

                if (!isTutorial)
                {
                    SDKManager.MiniLevelCompleted();
                
                    if (lockedBehindLevel)
                    {
                       if (sendAsFirebaseEvent) AirportManager.instance.RegisterAirportExpand();
                    }
                
                    SDKManager.MiniLevelStarted();
                }
            }
            actionAfterBuy?.Invoke();
            actionAfterBuy?.RemoveAllListeners();
            gameObject.SetActive(false);
        }

        public void AddActionToAfterBuy(UnityAction action) => actionAfterBuy.AddListener(action);
        public void AddXpToAfterBuy(UnityAction action, int xpValue)
        {
            xpValueTmp.text = $"+{xpValue}";
            xpUiObject.SetActive(true);
            actionAfterBuy.AddListener(action);
        }
    }
    
    internal enum ItemBuildTypeEnum
    {
        Build,
        Disable,
        Change,
        EventOnly,
        Build_Event,
        Change_Event
    }
}
    
