using DG.Tweening;
using UnityEngine;

namespace _GameAssets.Scripts.Other
{
    public class TutorialArrow : MonoBehaviour
    {
        [SerializeField] private GameObject arrowObject; // The GameObject that will be shown/hidden
        [SerializeField] private bool enableAtStart;

        private void Start()
        {
            if(enableAtStart) ShowArrow();
        }

        public void ShowArrow()
        {
            arrowObject.transform.DOScale(Vector3.one, 0.6f);
        }

        public void HideAndDisableArrow()
        {
            arrowObject.transform.DOScale(Vector3.zero, 0.6f).OnComplete(() =>
                gameObject.SetActive(false));
        }
    }
}