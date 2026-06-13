using System;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CandySpawner _candySpawner;
    [SerializeField] private SoundController _soundController;
    [SerializeField] private GameSequenceController _gameSequenceController;
    
    [Header("Input & Cats")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private CatController _leftCat;
    [SerializeField] private CatController _rightCat;
    
    [Header("Config")]
    [SerializeField] private SongConfig _songConfig;
        
    private List<NoteData> _notes => _candySpawner.Notes;
    private List<Candy> _candiesActive = new();
    
    
    // testing
    public bool IsPlaying = false;
    public float Now;
    public int NextIndex = 0;
    public float FallingTimer = 2f;

    // CandySpawner cần đọc từ controller
    public float HitY => 1.6f;
    public float FallSpeed { get; private set; }
    public float MissWindows => 0.2f;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        FallSpeed = (5f - (-3.5f)) / FallingTimer;
        _soundController.InitSong(_songConfig.GetRandomClip());
    }

    private void OnEnable()
    {
        _inputReader.LeftCatMove += _leftCat.Drag;
        _inputReader.RightCatMove += _rightCat.Drag;
    }

    private void OnDisable()
    {
        _inputReader.LeftCatMove -= _leftCat.Drag;
        _inputReader.RightCatMove -= _rightCat.Drag;
    }

    private void Update()
    {
        if (!IsPlaying) return;
        _inputReader.InpuHandler();
        HandleSpawnCandy();
        CheckForComplete();
        CheckForLose();
        HandleMoveCandies();
    }

    private void HandleSpawnCandy()
    {
        Now = _soundController.SongTime;
        if (NextIndex < _notes.Count && _notes[NextIndex].ta - FallingTimer <= Now)
        {
            var candy = _candySpawner.SpawnCandy(NextIndex, this);
            if (candy != null)
            {
                _candiesActive.Add(candy);
            }
            NextIndex++;
        }
    }

    private void CheckForLose()
    {
        for (int i = 0; i < _candiesActive.Count; i++)
        {
            if (_candiesActive[i].IsOutRange)
            {
                IsPlaying = false;
                _leftCat.PlayLoseAnimation();
                _rightCat.PlayLoseAnimation();
                Debug.Log("Failed");
            }
        }
    }
    
    private void CheckForComplete()
    {
        if (_soundController.IsComplete && _candiesActive.Count == 0)
        {
            Debug.Log("Win");
        }
    }
    

    private void HandleMoveCandies()
    {
        for (int i = 0; i < _candiesActive.Count; i++)
        {
            if (!_candiesActive[i].gameObject.activeSelf)
            {
                _candiesActive.RemoveAt(i);
                continue;
            }
            _candiesActive[i].OnMove();
        }
    }

    [ContextMenu("Start Game")]
    public void StartGame()
    {
        IsPlaying = true;
        _soundController.Playing();
        _leftCat.PlayStartAnimation();
        _rightCat.PlayStartAnimation();
        _gameSequenceController.PlayIntro();
    }
}
