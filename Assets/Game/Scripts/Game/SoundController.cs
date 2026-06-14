using System;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;

    public float SongDps => _audioSource.timeSamples;
    public float SongLength => _audioSource.clip.length;
    public bool IsComplete => SongTime >= SongLength;
    public float SongTime => _audioSource.time; 

    public void InitSong(AudioClip song, bool loop = false)
    {
        _audioSource.clip = song;
        _audioSource.loop = loop;
    }

    private void OnEnable()
    {
        GameEvents.OnGameStart += Playing;
        GameEvents.OnGameOver += Stop;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStart -= Playing;
        GameEvents.OnGameOver -= Stop;
    }

    public void Playing()
    {
        _audioSource.Play();
    }
    
    public void Stop()
    {
        _audioSource.Stop();
    }

    public void Pause()
    {
        _audioSource.Pause();
    }
}
