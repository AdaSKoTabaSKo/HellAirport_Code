using System;
using System.Collections.Generic;
using System.Linq;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.InteractionObjects;
using _GameAssets.Scripts.Player;
using _GameAssets.Scripts.Ui;
using CrazyLabsExtension;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoreMountains.NiceVibrations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _GameAssets.Scripts.Managers
{
    public class LevelManager : SingleMonoBehaviour<LevelManager>
    {
        [SerializeField] private List<LevelSetting> levelSettings;
        [SerializeField] private List<AirportExpandSettings> airportExpandSettings;
        
        [Header("Xp values Config")]
        [SerializeField] private int waitingAreaBuyXp;
        [SerializeField] private int kioskBuyXp;
        [SerializeField] private int airplaneBuyXp;
        [SerializeField] private int airplaneUpgradeBuyXp;
        
        [Header("Canvas UI")]
        [SerializeField] private CanvasGroup levelUpCanvas;
        [SerializeField] private TextMeshProUGUI prizeTmp;
        [SerializeField] private Button claimButton;
        
        [Header("Canvas UI")]
        [SerializeField] private Image levelBarFill;
        [SerializeField] private TextMeshProUGUI xpRatioTmp;
        [SerializeField] private TextMeshProUGUI levelTmp;
        [SerializeField] private GameObject particlePrefab;
        [SerializeField] private Canvas topBarCanvas;
        [SerializeField] private RectTransform particleFinalPoint;
        [SerializeField] private GameObject normalState;
        [SerializeField] private GameObject maxState;
        
        private bool _diffrentCanvasIsEnabled;
        private bool _thisCanvasIsEnabled;
        private bool _levelUpStarted;

        private bool _claimButtonClicked;
        
        private List<GameObject> _particlePool = new List<GameObject>();

        private PlayerManager _player;
        
        public event Action OnLevelUp;
        
        public int CurrentPlayerLevel
        {
            get => GameManager.Save.LoadValue("CurrentPlayerLevel", 0);
            set => GameManager.Save.SaveValueAndSync("CurrentPlayerLevel", value);
        }
        
        public int CurrentPlayerXp
        {
            get => GameManager.Save.LoadValue("CurrentPlayerXp", 0);
            set => GameManager.Save.SaveValueAndSync("CurrentPlayerXp", value);
        }

        private void Start()
        {
            _player = PlayerManager.instance;
            claimButton.onClick.AddListener(() => _claimButtonClicked = true);
            
            foreach (var a in airportExpandSettings)
            {
                a.expandBuyArea.SetItemUnlockAt(a.levelToUnlock);
                a.expandBuyArea.Initialize();
                a.expandBuyArea.gameObject.SetActive(false);
            }

            foreach (var a in airportExpandSettings)
            {
                if (a.levelToShowOn - 1 <= CurrentPlayerLevel)
                {
                    if(a.expandBuyArea.IsItemBought()) continue;
                    a.expandBuyArea.gameObject.SetActive(true);
                }
            }
            

            if (CurrentPlayerLevel >= levelSettings.IndexOf(levelSettings[^1]))
            {
                maxState.SetActive(true);
                normalState.SetActive(false);
                return;
            }

            levelTmp.text = $"{CurrentPlayerLevel+1}";
            RefreshLevelUi(false).Forget();
        }

        private async UniTask RefreshLevelUi(bool withParticles, int value = 0)
        {
            var curXp = CurrentPlayerXp;
            var maxXp = levelSettings[CurrentPlayerLevel].xpForNextLevel;
            
            var ratio =  (float)curXp/maxXp;

            if (withParticles)
            {
                for (int i = 0; i < value; i++)
                {
                    var p = GiveParticle();
                    p.SetActive(true);
                    AnimateParticle(p).Forget();
                    await UniTask.Delay(100);
                }
                
                await UniTask.Delay(100);
            }
            
            levelBarFill.DOFillAmount(ratio, 0.1f);
            xpRatioTmp.text = $"{curXp}/{maxXp}";
        }

        private async UniTask AnimateParticle(GameObject particle)
        {
            particle.transform.parent = Camera.main.transform;
            particle.transform.localPosition = topBarCanvas.transform.localPosition + Random.insideUnitSphere * 0.1f;
            await particle.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);

            var dupa = topBarCanvas.transform.TransformPoint(particleFinalPoint.localPosition);
            
            await particle.transform.DOLocalMove(Camera.main.transform.InverseTransformPoint(dupa), 0.5f);

            await UniTask.Delay(150);
            await particle.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack);
            HapticFeedbackController.TriggerHaptics(HapticTypes.LightImpact);
            AudioManager.instance.PlaySound(SoundType.LevelStar);
            particle.SetActive(false);
            
            particle.transform.parent = transform;
        }

        private async UniTask AddXp(int value, string nameOfItem)
        {
            Debug.Log($"Dalem xp: (value), z miejsca: {nameOfItem}");
            CurrentPlayerXp += value;
            _thisCanvasIsEnabled = true;
            
            if (CurrentPlayerLevel >= levelSettings.IndexOf(levelSettings[^1])) return;
            await RefreshLevelUi(true, value);
            
            if (CurrentPlayerXp < levelSettings[CurrentPlayerLevel].xpForNextLevel)
            {
                _thisCanvasIsEnabled = false;
                return;
            }
            
            if (_levelUpStarted) return;
            
            prizeTmp.text = $"{levelSettings[CurrentPlayerLevel].cashForLevelingUp}";
            LevelUp().Forget();
        }

        public void AddXpForWaitingAreaBuy() => AddXp(waitingAreaBuyXp, "waitingArea").Forget();
        public void AddXpForKioskBuy() => AddXp(kioskBuyXp, "kiosk").Forget();
        public void AddXpForAirplaneBuy() => AddXp(airplaneBuyXp,"airplane").Forget();
        public void AddXpForAirplaneUpgradeBuy() => AddXp(airplaneUpgradeBuyXp, "upgrade").Forget();
        public void AddXpForHax() => AddXp(1, "hax").Forget();

        public int XpValueForKiosk() => kioskBuyXp;
        public int XpValueForAirplane() => airplaneBuyXp;
        public int XpValueForAirplaneUpgrade() => airplaneUpgradeBuyXp;
        
        private async UniTask LevelUp()
        {
            _levelUpStarted = true;
            
            _claimButtonClicked = false;
            
            while (_diffrentCanvasIsEnabled)
            {
                await UniTask.Delay(1000);
            }

            _thisCanvasIsEnabled = true;
            _player.LockPlayerMovement();
            
            TopBarButtonsHandler.instance.HideButtons();
            
            levelUpCanvas.gameObject.SetActive(true);
            AudioManager.instance.PlaySound(SoundType.LevelUp);
            await levelUpCanvas.DOFade(1, 0.3f);
            levelUpCanvas.interactable = true;

            while (!_claimButtonClicked)
            {
                await UniTask.Delay(250);
            }
            
            CurrentPlayerXp -= levelSettings[CurrentPlayerLevel].xpForNextLevel;
            CurrentPlayerLevel++;
            SDKManager.LevelUp();
            
            if (CurrentPlayerLevel >= levelSettings.IndexOf(levelSettings[^1]))
            {
                maxState.SetActive(true);
                normalState.SetActive(false);
            }
            
            CurrencyManager.instance.AddCoinsWithoutAnimation(levelSettings[CurrentPlayerLevel-1].cashForLevelingUp);
            HapticFeedbackController.TriggerHaptics(HapticTypes.Success);
            await levelUpCanvas.DOFade(0, 0.3f);
            levelUpCanvas.interactable = false;
            levelUpCanvas.gameObject.SetActive(false);
            
            _player.UnlockPlayerMovement();
            
            TopBarButtonsHandler.instance.ShowButtons();
            
            levelTmp.text = $"{CurrentPlayerLevel+1}";
            RefreshLevelUi(false).Forget();
            
            OnLevelUp?.Invoke();

            foreach (var a in airportExpandSettings
                         .Where(a => a.levelToShowOn - 1 == CurrentPlayerLevel))
            {
                a.expandBuyArea.gameObject.SetActive(true);
            }
            
            if (CurrentPlayerXp >= levelSettings[CurrentPlayerLevel].xpForNextLevel)
            {
                LevelUp().Forget();
                return;
            }
            
            await UniTask.DelayFrame(1);
            _thisCanvasIsEnabled = false;
            _levelUpStarted = false;
        }

        public void RegisterOtherUiOpen()
        {
            _diffrentCanvasIsEnabled = true;
            TopBarButtonsHandler.instance.HideButtons();
        }

        public void RegisterOtherUiClosed()
        {
            _diffrentCanvasIsEnabled = false;
            TopBarButtonsHandler.instance.ShowButtons();
        }

        public bool IsLevelUpCanvasEnabled() => _thisCanvasIsEnabled;
        
        private GameObject GiveParticle()
        {
            foreach (var iPool in _particlePool)
            {
                if (!iPool.activeInHierarchy) return iPool;
            }

            var i = Instantiate(particlePrefab, transform);
            i.transform.localScale = Vector3.zero;
            _particlePool.Add(i);
            return i;
        }
    }

    [Serializable]
    internal class LevelSetting
    {
        public int xpForNextLevel;
        public int cashForLevelingUp;
    }
    
    [Serializable]
    internal class AirportExpandSettings
    {
        public BuyArea expandBuyArea;
        public int levelToShowOn;
        public int levelToUnlock;
    }
}