using System;
using System.Threading;
using _GameAssets.Scripts.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _GameAssets.Scripts.Player
{
    public class PlayerInteractionSystem : MonoBehaviour
    {
        private Interactable _objectToInteract;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out Interactable inter)) return;
            inter.TogglePlayerOnTrigger(true);
            if (inter._interactionStarted) return;
            _objectToInteract = inter;
            inter.ToggleInteractionWheelVisibility(true,false);
            inter.StartInteraction().Forget();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out Interactable inter)) return;
            inter._cancellationTokenSource?.Cancel();
            _objectToInteract.ToggleInteractionWheelVisibility(false,false);
            _objectToInteract = null;
            inter.TogglePlayerOnTrigger(false);
        }
    }
}