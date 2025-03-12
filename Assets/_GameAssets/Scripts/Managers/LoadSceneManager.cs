using _GameAssets.Scripts.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _GameAssets.Scripts.Managers
{
    public class LoadSceneManager : SingleMonoBehaviour<LoadSceneManager>
    {
        public Image progressBar;
        [SerializeField] private CanvasGroup allInCanvas;
        
        public async UniTask ShowCanvas(bool instant) => await allInCanvas.DOFade(1, instant ? 0 : 0.3f);
        public async UniTask HideCanvas() => await allInCanvas.DOFade(0, 0.3f);
        public void UpdateProgressBar(float progress) => progressBar.fillAmount = progress;
    }
}