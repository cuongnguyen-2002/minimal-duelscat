using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SongConfig", menuName = "Data/SongConfig")]
public class SongConfig : ScriptableObject
{
    [SerializeField] private List<AudioClip> _audioClips = new();
    
    public AudioClip GetRandomClip()
    {
        return _audioClips[Random.Range(0, _audioClips.Count)];
    }
}