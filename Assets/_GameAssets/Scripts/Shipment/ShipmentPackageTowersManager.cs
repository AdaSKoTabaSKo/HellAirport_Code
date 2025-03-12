using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _GameAssets.Scripts.Shipment
{
    public class ShipmentPackageTowersManager : MonoBehaviour
    {
        [SerializeField] private List<ShipmentPackageTower> myShipmentTowers;
        
        [Header("Settings")]
        [SerializeField] private GameObject invicibleWall;
        [SerializeField] private Transform dockedPosition;
        [SerializeField] private Transform undockedPosition;
        
        public async UniTask SpawnNewTruck()
        {
            foreach (var tower in myShipmentTowers)
            {
                tower.RespawnBoxes();
            }

            await transform.DOMove(dockedPosition.position, 4f);
            invicibleWall.SetActive(false);
        }

        public void EnableWall() => invicibleWall.SetActive(true);

        public async UniTask DespawnTruck()
        {
            invicibleWall.SetActive(true);
            await transform.DOMove(undockedPosition.position, 4f);
        }
    }
}