using System;
using _GameAssets.Scripts.Player;
using UnityEngine;

namespace _GameAssets.Scripts.Items
{
    public class ItemPickable : MonoBehaviour
    {
        [SerializeField] private ItemType myType;
        [SerializeField] private float itemHeight;
        [Space]
        [SerializeField] private bool shipmentItem;

        private bool _itemTaken;
        private Transform _initialParent;
        private Vector3 _initialPos;
        public bool IsItemTaken() => _itemTaken;
        public void SetAsTaken() => _itemTaken = true;
        public ItemType GiveItemType() => myType;
        public float GiveItemHeight() => itemHeight;

        private void Awake()
        {
            _initialParent = transform.parent;
            if(shipmentItem) _initialPos = transform.localPosition;
        }

        public void ResetParent() => transform.parent = _initialParent;
        
        public void ResetItem()
        {
            ResetParent();
            
            transform.localPosition = shipmentItem ? _initialPos : Vector3.zero;
            
            gameObject.SetActive(false);
        }
    }
}