using System;
using UnityEngine;

public class ReturnToPoolOnStop : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;
    public Action<VFXType, ParticleSystem> OnStopped;
    private VFXType _vfxType;

    public void Init(VFXType vfxType)
    {
        var main = _particleSystem.main;
        main.stopAction = ParticleSystemStopAction.Callback;
        _vfxType = vfxType;
    }

    private void OnParticleSystemStopped()
    {
        Debug.Log($"[VFX] {gameObject.name} stopped"); // debug check
        OnStopped?.Invoke(_vfxType, _particleSystem);
    }
}
