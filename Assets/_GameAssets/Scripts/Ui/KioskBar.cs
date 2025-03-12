using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GameAssets.Scripts.Ui
{
    public class KioskBar : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private GameObject noStockImage;

        [Header("Colors")]
        [SerializeField] private Color fillColor50;
        [SerializeField] private Color fillColor25;
        [SerializeField] private Color fillColor0;
        
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI textTmp;

        private Sequence _seq;

        public void RefreshProgress(float value)
        {
            _seq?.Kill(true);
            
            _seq.Append(fillImage.DOFillAmount(value, 0.2f).OnComplete(()=> textTmp.text = $"{(int)(value * 100)}%"));
            _seq.Join(fillImage.DOColor(value >= 0.5f ? fillColor50 : value >= 0.25f ? fillColor25 : fillColor0, 0.1f));

            _seq.Play();
            
            noStockImage.SetActive(value <= 0);
        }
    }
}