using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action<VFXType, Vector2> SpawnVFX;
    public static event Action OnGameStart; 

    public static void RequestSpawnVFX(VFXType type, Vector2 position)
    {
        SpawnVFX?.Invoke(type, position);
    }
    
    public static void RaiseGameStart()
    {
        OnGameStart?.Invoke();
    }
}
