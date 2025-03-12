using System.Collections.Generic;
using _GameAssets.Scripts.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace _GameAssets.Scripts.Player
{
    public class PlayerSkinManager : SingleMonoBehaviour<PlayerSkinManager>
    {
        [SerializeField] private GameObject classicSkin;
        
        [Header("Fireman Suit")]
        [SerializeField] private List<GameObject> fireManItems;
        
        public void EnableMechanicSkin()
        {
            classicSkin.SetActive(false);
            foreach (var i in fireManItems) i.SetActive(true);
        }

        public void EnableClassicLook()
        {
            foreach (var i in fireManItems) i.SetActive(false);
            classicSkin.SetActive(true);
        }
    }
}