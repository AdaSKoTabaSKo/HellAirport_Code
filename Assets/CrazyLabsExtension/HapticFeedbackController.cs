using System;
using UnityEngine;
using MoreMountains.NiceVibrations;

namespace CrazyLabsExtension
{
    public class HapticFeedbackController : MonoBehaviour
    {
        public static event Action<bool> OnHapticsEnabledChanged = delegate{ };

        private static HapticFeedbackController _instance;

        private float _hapticTimer     = 0f;
        private bool  _currentlyActive = false;
        private bool  _hapticsPaused   = false;

        private const float _hapticMinimumDelay = 0.1f;

        private void Awake()
        {
            _instance = this;

            _currentlyActive = PlayerPrefs.GetInt( "HapticsEnabled", 1 ) == 1;

            //UI
            HapticFeedbackUIButton.OnHapticButtonPressed += OnButtonPressed;
            HapticFeedbackToggle.OnHapticsToggled        += OnHapticsToggled;

#if TTP_CORE
            //Tabtale.TTPlugins.TTPCore.PauseGameMusicEvent += OnPauseMusicRequested;
#endif
        }

        private void OnDestroy()
        {
            //UI
            HapticFeedbackUIButton.OnHapticButtonPressed -= OnButtonPressed;
            HapticFeedbackToggle.OnHapticsToggled        -= OnHapticsToggled;

#if TTP_CORE
            //Tabtale.TTPlugins.TTPCore.PauseGameMusicEvent -= OnPauseMusicRequested;
#endif
        }

        private void Update()
        {
            _hapticTimer -= Time.deltaTime;
        }

        private void Start()
        {
#if UNITY_IOS
            MMVibrationManager.iOSInitializeHaptics( );
#endif
        }

        private void OnHapticsToggled( bool value )
        {
            _currentlyActive = value;

            PlayerPrefs.SetInt( "HapticsEnabled", _currentlyActive ? 1 : 0 );

            OnHapticsEnabledChanged.Invoke( _currentlyActive );
        }

        private void OnPauseMusicRequested( bool shouldPause )
        {
            _hapticsPaused = shouldPause;
        }

        private void OnButtonPressed( HapticTypes buttonHapticType )
        {
            TriggerHaptics( buttonHapticType, true );
        }

        public static void TriggerHaptics( HapticTypes type, bool force = false )
        {
            if ( _instance._hapticsPaused )
                return;

            if ( !_instance._currentlyActive )
                return;

            if ( _instance._hapticTimer > 0 && !force )
                return;

            MMVibrationManager.Haptic( type );
            _instance._hapticTimer = _hapticMinimumDelay;
        }
    }
}