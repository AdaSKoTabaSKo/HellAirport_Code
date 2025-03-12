using System;
using _GameAssets.Scripts.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GameAssets.Scripts.InteractionObjects
{
    [Serializable]
    public class ToiletCubicle : MonoBehaviour
    {
        public GameObject doors;
        public Transform wcPosition;
        [HideInInspector] public bool occupied;
        
        [Header("Cleaning Mechanic")]
        [SerializeField] private int howManyUsesBeforeBecomingDirty;
        [SerializeField] private GameObject dirtySkin;

        private bool _toiletIsDirty;
        private int _internalUsageCounter;

        public bool CanUseToilet() => !_toiletIsDirty;

        private void Start()
        {
            doors.transform.localRotation = new Quaternion(0, 0.64f, 0, 0.76f);
        }

        public void Occupy()
        {
            occupied = true;
            doors.transform.DOLocalRotateQuaternion(new Quaternion(0,0,0,1), 0.3f);
        }

        public async UniTask Exit()
        {
            RegisterUsage();
            doors.transform.DOLocalRotateQuaternion(new Quaternion(0,0.64f,0,0.76f), 0.3f);
            await UniTask.Delay(500);
            occupied = false;
        }

        private void RegisterUsage()
        {
            _internalUsageCounter++;

            if (_internalUsageCounter < howManyUsesBeforeBecomingDirty) return;
            MakeToiletDirty();
        }
        
        private void MakeToiletDirty()
        {
            _toiletIsDirty = true;
            dirtySkin.SetActive(true);
        }
        
        public void CleanToilet()
        {
            dirtySkin.SetActive(false);
            _internalUsageCounter = 0;
            _toiletIsDirty = false;
        }
    }
}