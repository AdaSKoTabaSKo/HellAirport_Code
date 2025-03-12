using System;
using _GameAssets.Scripts.Player;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace _GameAssets.Scripts.Guest
{
    public class GuestEscortTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject interactionCircleImage;
        [SerializeField] private SphereCollider myCollider;
        [SerializeField] private NavMeshAgent myAgent;
        
        private bool _triggerEnabled;
        private Transform _mainCameraTransform;
        
        private void Start()
        {
            interactionCircleImage.transform.localScale = Vector3.zero;
            interactionCircleImage.SetActive(false);
            myCollider.enabled = false;
            _mainCameraTransform = Camera.main.transform;
        }

        public void EnableTrigger()
        {
            PauseMoving();
            myCollider.enabled = true;
            _triggerEnabled = true;
            interactionCircleImage.SetActive(true);
            interactionCircleImage.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        public async UniTask DisableTrigger()
        {
            myCollider.enabled = false;
            await interactionCircleImage.transform.DOScale(0, 0.3f).SetEase(Ease.InBack);
            _triggerEnabled = false;
            interactionCircleImage.SetActive(false);
            ResumeMoving();
        }
        
        private void LateUpdate()
        {
            //if(_triggerEnabled) interactionCircleImage.transform.rotation = Quaternion.Euler(90, 0, 0);
            if (_triggerEnabled)
                interactionCircleImage.transform.DOLookAt(_mainCameraTransform.position, 0.1f, AxisConstraint.Y);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerManager player)) return;
            ResumeMoving();
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out PlayerManager player)) return;
            PauseMoving();
        }
        
        public void PauseMoving() => myAgent.isStopped = true;

        public void ResumeMoving() => myAgent.isStopped = false;
    }
}