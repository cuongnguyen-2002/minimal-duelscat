using UnityEngine;

public class SoundController : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;

    public float SongDps => _audioSource.timeSamples;
    public float SongLength => _audioSource.clip.length;
    public bool IsComplete => SongDps - SongLength >= 0;
    public float SongTime => _audioSource.time; 

    public void InitSong(AudioClip song, bool loop = false)
    {
        _audioSource.clip = song;
        _audioSource.loop = loop;
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
