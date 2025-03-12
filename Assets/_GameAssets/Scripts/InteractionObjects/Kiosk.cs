using System;
using System.Collections.Generic;
using System.Linq;
using _GameAssets.Scripts.Currency;
using _GameAssets.Scripts.Guest;
using _GameAssets.Scripts.Managers;
using _GameAssets.Scripts.Player;
using _GameAssets.Scripts.Ui;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace _GameAssets.Scripts.InteractionObjects
{
    public class Kiosk : MultiQueueInteractionObject
    {
        [SerializeField] private ItemType acceptingItemType;
        [SerializeField] private CurrencyArea myCurrencyArea;
        [SerializeField] private List<GameObject> itemsObjects;
        [Space]
        [SerializeField] private KioskBar kioskBar;

        [SerializeField] private bool tutorialKiosk;
        
        private int _numberOfObjectsInKiosk;
        private int _maxNumberOfObjectsInKiosk;

        private bool tutorialIsDone;

        private AirportHelper _myHelper;

        private int _priceForMyItem;
        private int _restockValue;

        public bool IsKioskFull() => _numberOfObjectsInKiosk >= _maxNumberOfObjectsInKiosk;
        
        private void Start()
        {
            _priceForMyItem = AirportEconomyManager.instance.GetPriceForKiosk(acceptingItemType);
            
            _numberOfObjectsInKiosk = itemsObjects.Count;
            _maxNumberOfObjectsInKiosk = itemsObjects.Count;

            _restockValue = (int)(_maxNumberOfObjectsInKiosk * 0.35f);
            kioskBar.RefreshProgress((float)_numberOfObjectsInKiosk/_maxNumberOfObjectsInKiosk);

            if (tutorialKiosk)
            {
                tutorialIsDone = TutorialManager.instance.CheckIfTutorialIsDone(TutorialType.PerfumesStacking);
                if (!tutorialIsDone)
                {
                    foreach (var o in itemsObjects) o.SetActive(false);
                    _numberOfObjectsInKiosk = 0;
                }
                else
                {
                    tutorialKiosk = false;
                }
            }
            
            kioskBar.RefreshProgress((float)_numberOfObjectsInKiosk / _maxNumberOfObjectsInKiosk);
        }

        public override async UniTask DoThingsWithGuest(Guest.GuestSystem g)
        {
            if (_numberOfObjectsInKiosk <= 0)
            {
                await UniTask.Delay(500);
                return;
            }
            
            await g.EnableInteractionUi(1);
            
            foreach (var i in itemsObjects)
            {
                if(!i.activeInHierarchy) continue;
                i.SetActive(false);
                _numberOfObjectsInKiosk--;
                kioskBar.RefreshProgress((float)_numberOfObjectsInKiosk/_maxNumberOfObjectsInKiosk);
                myCurrencyArea.AddCash(_priceForMyItem);
                if (_numberOfObjectsInKiosk <= 0) TriggerHelperHelp();
                break;
            }
        }
        void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerRefreshObjectSystem player)) return;

            if (_numberOfObjectsInKiosk >= _maxNumberOfObjectsInKiosk) return;
            player.DeliverPickedItemToKiosk(acceptingItemType, this);
        }
        
        public void RestockKiosk()
        {
            if (tutorialKiosk)
            {
                if (!tutorialIsDone)
                {
                    tutorialIsDone = true;
                    TutorialManager.instance.SetStepAsDone(TutorialType.PerfumesStacking);
                }
            }
            
            _numberOfObjectsInKiosk = Mathf.Clamp(_numberOfObjectsInKiosk + _restockValue, 0, _maxNumberOfObjectsInKiosk);

            for (var i = 0; i < _numberOfObjectsInKiosk; i++)
            {
                if(itemsObjects[i].activeInHierarchy) continue;
                itemsObjects[i].SetActive(true);
                itemsObjects[i].transform.DOLocalJump(itemsObjects[i].transform.localPosition, 1,1,0.2f);
            }
            
            kioskBar.RefreshProgress((float)_numberOfObjectsInKiosk/_maxNumberOfObjectsInKiosk);

            if (_numberOfObjectsInKiosk < _maxNumberOfObjectsInKiosk) return;
            if (IsHelperActive()) _myHelper.TriggerKioskRestock(this);
        }

        public void SetHelper(AirportHelper helper) => _myHelper = helper;
        public ItemType GiveAcceptingItemType() => acceptingItemType;
        private void TriggerHelperHelp()
        {
            if (!IsHelperActive()) return;
            _myHelper.AddToHelpList(this);
        }

        public void CheckIfNeedHelp()
        {
            if (_numberOfObjectsInKiosk > 0) return;
            TriggerHelperHelp();
        }

        private bool IsHelperActive() => _myHelper != null && _myHelper.isActiveAndEnabled;
    }
}