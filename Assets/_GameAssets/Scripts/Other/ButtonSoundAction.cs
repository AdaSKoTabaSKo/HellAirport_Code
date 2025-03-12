using System;
using UnityEngine;
using UnityEngine.UI;

namespace _GameAssets.Scripts.Other
{
    public class ButtonSoundAction : MonoBehaviour
    {
        [SerializeField] private Button myButton;

        private void Start()
        {
            //myButton.GetComponent<Button>();
            myButton.onClick.AddListener(PlayAudio);
        }

        private void PlayAudio() => AudioManager.instance.PlaySound(SoundType.ButtonClick);
    }
}