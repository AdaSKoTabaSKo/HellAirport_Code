using System;
using System.Collections.Generic;
using _GameAssets.Scripts.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
//using Tabtale.TTPlugins;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _GameAssets.Scripts.Managers
{
    public class GameManager : SingleMonoBehaviour<GameManager>
    {
        public static Save Save { private set; get;}

        [Header("Player")]
        [SerializeField] private GameObject playerObject;
        [Space]
        [Header("Scenes")]
        [SerializeField] private String loadingSceneName;
        [SerializeField] private List<String> worldScenesNames;
        
        [Header("Fake Loading")]
        [SerializeField] private CanvasGroup fakeLoadingGroup;
        //[SerializeField] private Image loadingBar;

        public AirportManager currentLoadedWorld;
        
        public static event Action OnSceneStartLoading;
        public static event Action OnSceneFinishLoading;
        
        public int CurrentWorldIndex
        {
            get => Save.LoadValue("CurrentWorldIndex", 0);
            set => Save.SaveValueAndSync("CurrentWorldIndex", value);
        }
        
        public static int BoughtStuff
        {
            get => Save.LoadValue("BoughtStuff", 1);
            set => Save.SaveValueAndSync("BoughtStuff", value);
        }
        
        protected override void Awake()
        {
            //TTPCore.Setup();
           
            Application.targetFrameRate = 60;
            Save = new Save("HellAirport101");
            base.Awake();
            
            FakeLoading().Forget();
        }

        private void Start()
        {
            //LoadSceneAsyncWithProgressBar(CurrentWorldIndex, true).Forget();
        }

        private async UniTask FakeLoading()
        {
            fakeLoadingGroup.gameObject.SetActive(true);
            fakeLoadingGroup.alpha = 1;
            //await loadingBar.DOFillAmount(1, 0.4f);
            await UniTask.Delay(2000);
            fakeLoadingGroup.DOFade(0, 0.3f).OnComplete(()=>fakeLoadingGroup.gameObject.SetActive(false));
            await UniTask.Delay(300);
            
            OnSceneFinishLoading?.Invoke();
        }

        public async UniTaskVoid LoadSceneAsyncWithProgressBar(int levelIndex, bool onStart)
        {
            OnSceneStartLoading?.Invoke();
            currentLoadedWorld = null;
            await SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);

            await LoadSceneManager.instance.ShowCanvas(onStart);
            
            if(!onStart) SceneManager.UnloadSceneAsync(worldScenesNames[0]);
            
            var worldSceneOperation = SceneManager.LoadSceneAsync(worldScenesNames[levelIndex], LoadSceneMode.Additive);
            worldSceneOperation.allowSceneActivation = false;

            await LoadSceneManager.instance.progressBar.DOFillAmount(0.5f, 0.6f).SetEase(Ease.Linear);
            
            while (!worldSceneOperation.isDone)
            {
                var progress = worldSceneOperation.progress;
                LoadSceneManager.instance.UpdateProgressBar(progress);

                if (progress >= 0.9f)
                {
                    worldSceneOperation.allowSceneActivation = true;
                }

                await UniTask.Yield();
            }
            
            currentLoadedWorld = FindObjectOfType<AirportManager>();
            await UniTask.WaitWhile(()=>currentLoadedWorld == null);
            
            OnSceneFinishLoading?.Invoke();
            
            await LoadSceneManager.instance.progressBar.DOFillAmount(1f, 0.6f).SetEase(Ease.Linear);
            
            await LoadSceneManager.instance.HideCanvas();
            SceneManager.UnloadSceneAsync(loadingSceneName);
        }
    }
}