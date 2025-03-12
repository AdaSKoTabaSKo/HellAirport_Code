using System;
using System.Collections.Generic;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.Events;
using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Player;
using Cinemachine;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GameAssets.Scripts.Planes
{
    public class AirplaneRepairSystem : MonoBehaviour
    {
        [SerializeField] private List<AirplaneRepairPoint> repairPoints;
        [SerializeField] private CinemachineVirtualCamera airplaneRepairCamera;
        

        private int _partsToRepair;
        private int _repairedParts;

        private Airplane _myAirplane;

        public void SetNumberOfPartsToRepair(int number) => _partsToRepair = Math.Min(number, repairPoints.Count);

        public async UniTask StartRepair(Airplane a)
        { 
            SDKManager.InGameEventStarted("AirplaneOnFireEvent");
            
            AirplanesRepairEvent.instance.TryEnablingFireManLocker();
            
            _myAirplane = a;
            _repairedParts = 0;

            var random = new System.Random();
            repairPoints.Sort((x, y) => random.Next(-1, 2));

            while (BuyAreaUnlockManager.instance.IsCameraInUse()) await UniTask.Delay(500);
            airplaneRepairCamera.m_Priority = 11;
            airplaneRepairCamera.gameObject.SetActive(true);
            PlayerManager.instance.FocusToCameraAndLockPlayer(airplaneRepairCamera);
            await UniTask.Delay(1500);

            for (var i = 0; i < _partsToRepair; i++)
            {
                repairPoints[i].EnableRepair(this).Forget();
            }
            
            AirplanesRepairEvent.instance.EnableInteractionWithMechanicSuit();
            
            await UniTask.Delay(1500);
            airplaneRepairCamera.m_Priority = 0;
            AirplanesRepairEvent.instance.GetMechanicCamera().m_Priority = 11;
            await UniTask.Delay(2000);
            AirplanesRepairEvent.instance.GetMechanicCamera().m_Priority = 0;
            PlayerManager.instance.FocusToNormalPlayerCamera();

            while (!AirplanesRepairEvent.instance.IsPlayerAMechanic())
            {
                await UniTask.Delay(500);
            }

            for (var i = 0; i < _partsToRepair; i++)
            {
                repairPoints[i].EnableRepairInteraction();
            }
        }

        public void RegisterRepair()
        {
            _repairedParts++;

            if (_repairedParts < _partsToRepair) return;
            _myAirplane.RepairWholeAirplane();
            AirplanesRepairEvent.instance.FinishEvent();
        }
    }
}