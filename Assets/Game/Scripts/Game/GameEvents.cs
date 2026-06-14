using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action<VFXType, Vector2> SpawnVFX;
    public static event Action OnGameStart; 
    public static event Action OnGameOver;
    public static event Action OnGameComplete;
    public static event Action OnGameReset;
    public static event Action<int> OnScoreChanged;

    public static void RequestSpawnVFX(VFXType type, Vector2 position)
    {
        SpawnVFX?.Invoke(type, position);
    }
    
    public static void RaiseGameStart()
    {
        OnGameStart?.Invoke();
    }

    public static void RaiseGameOver()
    {
        OnGameOver?.Invoke();
    }

    public static void RaiseGameComplete()
    {
        OnGameComplete?.Invoke();
    }

    public static void RaiseScoreChanged(int score)
    {
        OnScoreChanged?.Invoke(score);
    }

    public static void RaiseGameReset()
    {
        OnGameReset?.Invoke();
    }
}
