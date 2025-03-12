using System.Collections.Generic;
using _GameAssets.Scripts.Core;
using UnityEngine;

namespace _GameAssets.Scripts.Ui
{
    public class TopBarButtonsHandler : SingleMonoBehaviour<TopBarButtonsHandler>
    {
        [SerializeField] private GameObject allButtonsHandler;

        public void ShowButtons()
        {
            allButtonsHandler.SetActive(true);
        }

        public void HideButtons()
        {
            allButtonsHandler.SetActive(false);
        }
        
    }
}