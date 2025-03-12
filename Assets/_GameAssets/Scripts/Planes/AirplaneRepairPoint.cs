using System;
using _GameAssets.Scripts.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GameAssets.Scripts.Planes
{
    public class AirplaneRepairPoint : MonoBehaviour
    {
        [SerializeField] private Interactable myInteractablePoint;
        [SerializeField] private ParticleSystem flamesParticle;
        [SerializeField] private GameObject explosionParticle;
        
        private AirplaneRepairSystem _myRepairSystem;

        private void Start()
        {
            myInteractablePoint.AddActionToInteract(ProxyFinishRepair);
            gameObject.SetActive(false);
        }

        public async UniTask EnableRepair(AirplaneRepairSystem repairSystem)
        {
            gameObject.SetActive(true);
            _myRepairSystem = repairSystem;
            
            explosionParticle.SetActive(true);
            await UniTask.Delay(100);
            flamesParticle.gameObject.SetActive(true);
            flamesParticle.Play();
            
        }
        
        public void EnableRepairInteraction() => myInteractablePoint.gameObject.SetActive(true);

        private void ProxyFinishRepair() => RepairPart().Forget();
        private async UniTask RepairPart()
        {
            flamesParticle.Stop();
            _myRepairSystem.RegisterRepair();
            myInteractablePoint.gameObject.SetActive(false);

            await UniTask.Delay(1000);
            
            flamesParticle.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}