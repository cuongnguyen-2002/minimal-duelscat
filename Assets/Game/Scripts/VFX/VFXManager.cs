using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public enum VFXType
{
    Eater,
}

public class VFXManager : MonoBehaviour
{

    [System.Serializable]
    private class VFXData
    {
        public VFXType Type;
        public ParticleSystem Prefab;
        public int DefaultCapacity = 5;
        public int MaxSize = 20; 
    }
    
    [SerializeField] private List<VFXData> _vfxEntries = new();
    private Dictionary<VFXType, ObjectPool<ParticleSystem>> _vfxPools = new();

    private void Start()
    {
        foreach (VFXData vfxData in _vfxEntries)
        {
            var captureVFXData = vfxData;
            var vfxPool = new ObjectPool<ParticleSystem>(
                createFunc: () => CreateVFX(captureVFXData),
                actionOnRelease: vfx => vfx.gameObject.SetActive(false),
                actionOnGet: vfx => vfx.gameObject.SetActive(true),
                collectionCheck:false,
                defaultCapacity: captureVFXData.DefaultCapacity,
                maxSize: captureVFXData.MaxSize
            );
            _vfxPools[captureVFXData.Type] = vfxPool;
        }
    }

    private void OnEnable()
    {
        GameEvents.SpawnVFX += SpawnVFX;
    }

    private void OnDisable()
    {
        GameEvents.SpawnVFX -= SpawnVFX;
    }

    private ParticleSystem CreateVFX(VFXData vfxData)
    {
        var vfxInstance = Instantiate(vfxData.Prefab);
        var toPoolOnStop = vfxInstance.GetComponent<ReturnToPoolOnStop>();
        if (toPoolOnStop == null) toPoolOnStop = vfxInstance.gameObject.AddComponent<ReturnToPoolOnStop>();
        toPoolOnStop.Init(vfxData.Type);
        toPoolOnStop.OnStopped += ReleaseVFX;
        return vfxInstance;
    }

    public void SpawnVFX(VFXType vfxType, Vector2 position)
    {
        if(!_vfxPools.TryGetValue(vfxType, out var pool)) return;

        var vfx = pool.Get();
        vfx.transform.position = position;
        vfx.transform.SetParent(transform);
        vfx.Play(true);
    }

    private void ReleaseVFX(VFXType vfxType, ParticleSystem vfx)
    {
        if(!_vfxPools.TryGetValue(vfxType, out var pool)) return;
        vfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        vfx.transform.localPosition = Vector3.zero;
        pool.Release(vfx);
    }

    private void OnDestroy()
    {
        foreach (var pool in _vfxPools.Values)
            pool.Clear();
        _vfxPools.Clear();
    }
}
