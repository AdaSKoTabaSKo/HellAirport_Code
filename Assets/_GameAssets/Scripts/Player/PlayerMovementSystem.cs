using System;
using _GameAssets.Scripts.Core;
using _GameAssets.Scripts.Managers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace _GameAssets.Scripts.Player
{
    public class PlayerMovementSystem : MonoBehaviour
    {
        [Header("Components")]
        public Joystick joystick;
        public GameObject infiniteFingerObject;
        
        [Header("Settings")]
        [SerializeField] private float playerSpeed;
        [SerializeField] private LayerMask groundLayer;
        
        private Vector3 _currentSpeed;
        private CharacterController _controller;
        private bool _playerIsPaused;
        private bool _ftuxShown;
        private bool _forceFocus;
        private bool _infiniteFingerEnabled = true;

        private Vector3 _initialPos;
        private Vector3 _interactionPos;
        
        protected void Awake()
        {
            _controller = GetComponent<CharacterController>();
            
            GameManager.OnSceneStartLoading += () => _playerIsPaused = true;
            GameManager.OnSceneFinishLoading += () => _playerIsPaused = false;
        }

        private void Start()
        {
            _initialPos = transform.position;
        }

        public void UnlockPlayerMovement() => _playerIsPaused = false;
        public void LockPlayerMovement()
        {
            _playerIsPaused = true;
            PlayerManager.instance.playerAnimator.SetFloat("Speed", 0);
        }

        private void Update()
        {
            if (_playerIsPaused) return;
            
            if (!_controller.isGrounded)
            {
                _controller.Move(Vector3.zero.With(y: -9.81f * Time.deltaTime));

                if (transform.position.y < -3f) transform.position = _initialPos;
            }
            
            _currentSpeed = new Vector3(joystick.Horizontal, 0, joystick.Vertical);
            
            PlayerManager.instance.playerAnimator.SetFloat("Speed", _currentSpeed.magnitude);

            if (joystick.Direction == Vector2.zero) return;
            MovePlayer();

            if (!_infiniteFingerEnabled) return;
            _infiniteFingerEnabled = false;
            infiniteFingerObject.SetActive(false);
        }

        private void MovePlayer()
        {
            var currentSpeed = playerSpeed * Time.deltaTime;
            var direction = new Vector3(joystick.Horizontal, 0, joystick.Vertical);
            direction = Camera.main.transform.TransformDirection(direction);
            direction.y = 0; 
            var newPos = direction.normalized * currentSpeed;
            _controller.Move(newPos);

            if (joystick.Direction == Vector2.zero) return;
            
            var lookPos = _forceFocus ? _interactionPos : transform.position + newPos;
            transform.DOLookAt(lookPos, 0.1f, AxisConstraint.Y);
        }

        public void EnableLookAtInteraction(Vector3 pos)
        {
            _forceFocus = true;
            _interactionPos = pos;
            
            if (joystick.Direction != Vector2.zero) return;
            transform.DOLookAt(_interactionPos, 0.1f, AxisConstraint.Y);
        }

        public void DisableLookAtInteraction() => _forceFocus = false;

        private bool CheckGround(Vector3 newPosition)
        {
            //var position = newPosition;
            var rayDown = new Ray(transform.position + newPosition.With(y:0.5f), Vector3.down);
            return Physics.Raycast(rayDown, out _, 1.8f, groundLayer);
        }

    }
}