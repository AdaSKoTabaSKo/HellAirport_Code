using _GameAssets.Scripts.Managers;
using CrazyLabsExtension;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _GameAssets.Scripts.Settings
{
    public class SettingsManager : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private CanvasGroup settingsCanvasGroup;
        [SerializeField] private Button openSettingsButton;
        [SerializeField] private Button closeSettingsButton;
        [SerializeField] private Button soundButton;

        [Header("Button States Sprites")]
        public Sprite soundEnabledSprite;
        public Sprite soundDisabledSprite;

        [Header("Audio Managers")]
        private AudioManager audioManager;

        public bool AudioSave 
        {
            get => GameManager.Save.LoadValue("AudioSave", true);
            private set => GameManager.Save.SaveValueAndSync("AudioSave", value);
        }

        private void Start()
        {
            audioManager = AudioManager.instance;
        
            openSettingsButton.onClick.AddListener(OpenSettingsPanel);
            closeSettingsButton.onClick.AddListener(CloseSettingsPanel);
            
            soundButton.onClick.AddListener(() => ToggleSound());
            
            soundButton.image.sprite = AudioSave ? soundEnabledSprite : soundDisabledSprite;
            audioManager.ToggleSound(AudioSave);
        }

        public void OpenSettingsPanel()
        {
            openSettingsButton.gameObject.SetActive(false);
            settingsCanvasGroup.gameObject.SetActive(true);
            settingsCanvasGroup.DOFade(1, 0.2f);
        }

        public void CloseSettingsPanel()
        {
            settingsCanvasGroup.DOFade(0, 0.2f).OnComplete(()=> settingsCanvasGroup.gameObject.SetActive(false));
            openSettingsButton.gameObject.SetActive(true);
        }

        private void ToggleSound()
        {
            AudioSave = !AudioSave;
            audioManager.ToggleSound(AudioSave);
            soundButton.image.sprite = AudioSave ? soundEnabledSprite : soundDisabledSprite;
        }

    }
}