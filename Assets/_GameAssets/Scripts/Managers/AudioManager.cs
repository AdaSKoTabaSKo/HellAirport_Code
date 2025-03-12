using System.Linq;
using System.Collections.Generic;
using _GameAssets.Scripts.Core;
using UnityEngine;

public enum SoundType
{
    ButtonClick,
    ItemPickup,
    LevelUp,
    Upgrade,
    BuyArea,
    LevelStar,
}

[System.Serializable]
public class Sound
{
    public AudioClip clip;
    public SoundType type;
    public bool pitchVariety;
}

public class AudioManager : SingleMonoBehaviour<AudioManager>
{
    [SerializeField] private List<Sound> sounds;
    private bool _isSoundEnabled = true;
    
    private AudioSource _globalAudioSource;
    private AudioSource _backgroundMusicSource;

    [SerializeField] private AudioClip backgroundMusic;


    protected override void Awake()
    {
        base.Awake();
        _globalAudioSource = gameObject.AddComponent<AudioSource>();
        _backgroundMusicSource = gameObject.AddComponent<AudioSource>();

        _globalAudioSource.volume = 0.4f;
        
        _backgroundMusicSource.clip = backgroundMusic;
        _backgroundMusicSource.volume = 0.5f;
        _backgroundMusicSource.loop = true;
        _backgroundMusicSource.spatialBlend = 0;
        
        PlayBackgroundMusic();
    }

    public void PlaySound(SoundType type)
    {
        if (!_isSoundEnabled) return;
        var s = sounds.FirstOrDefault(sound => sound.type == type);
        if (s == null) return;

        _globalAudioSource.pitch = s.pitchVariety ? Random.Range(0.98f, 1.02f) : 1;

        _globalAudioSource.PlayOneShot(s.clip);
    }

    public void PlayBackgroundMusic()
    {
        // Assuming that the clip for the background music is already set.
        if (_isSoundEnabled) _backgroundMusicSource.Play();
    }

    public void ToggleSound(bool enable)
    {
        _isSoundEnabled = enable;
        
        if (!_isSoundEnabled)
        {
            _globalAudioSource.Stop();
            _backgroundMusicSource.Stop();
        }
        else
        {
            PlayBackgroundMusic();
        }
    }
}