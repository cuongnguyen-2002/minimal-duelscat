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
    [SerializeField] private float _laneHeight = 8.5f;
    [SerializeField] private float _fallingTimer = 2f;
    [SerializeField] private float _missWindows = 0.2f;
    [SerializeField] private float _bottomHitY = 1.6f;
        
    private List<NoteData> _notes => _candySpawner.Notes;
    private List<Candy> _candiesActive = new();
    
    
    //gameplay
    private bool _isPlaying = false;
    private float _songTimeNow;
    private int _noteIndex = 0;
    private float _fallSpeed;
    private int _score = 0;

    // CandySpawner cần đọc từ controller
    public float BottomHitY => _bottomHitY;
    public float FallSpeed => _fallSpeed;
    public float MissWindows => _missWindows;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _fallSpeed = _laneHeight / _fallingTimer;
        _score = 0;
    }

    private void OnEnable()
    {
        _inputReader.LeftCatMove += _leftCat.Drag;
        _inputReader.RightCatMove += _rightCat.Drag;
        _leftCat.OnCatEatingCandy += AddScore;
        _rightCat.OnCatEatingCandy += AddScore;
        GameEvents.OnGameStart += StartGame;
        GameEvents.OnGameReset += OnReset;
    }

    private void OnDisable()
    {
        _inputReader.LeftCatMove -= _leftCat.Drag;
        _inputReader.RightCatMove -= _rightCat.Drag;
        _leftCat.OnCatEatingCandy -= AddScore;
        _rightCat.OnCatEatingCandy -= AddScore;
        GameEvents.OnGameStart -= StartGame;
        GameEvents.OnGameReset -= OnReset;
        
    }

    private void Update()
    {
        if (!_isPlaying) return;
        HandleSpawnCandy();
        CheckForComplete();
        CheckForLose();
        HandleMoveCandies();
    }

    private void HandleSpawnCandy()
    {
        _songTimeNow = _soundController.SongTime;
        if (_noteIndex < _notes.Count && _notes[_noteIndex].ta <= _songTimeNow)
        {
            var candy = _candySpawner.SpawnCandy(_noteIndex, this);
            if (candy != null)
            {
                _candiesActive.Add(candy);
            }
            _noteIndex++;
        }
        
    }

    private void CheckForLose()
    {
        for (int i = 0; i < _candiesActive.Count; i++)
        {
            if (_candiesActive[i].IsOutRange)
            {
                GameComplete();
                GameEvents.RaiseGameOver();
            }
        }
    }
    
    private void CheckForComplete()
    {
        if (_soundController.IsComplete && _candiesActive.Count == 0)
        {
            GameComplete();
            GameEvents.RaiseGameComplete();
            
        }
    }

    private void GameComplete()
    {
        _isPlaying = false;
        _gameSequenceController.PlayIntro();
        UIManager.Instance.GetUI<GamePlayScreen>().Hide();
    }
    

    private void HandleMoveCandies()
    {
        for (int i = _candiesActive.Count - 1; i >= 0; i--)
        {
            if (!_candiesActive[i].gameObject.activeSelf)
            {
                _candiesActive.RemoveAt(i);
                continue;
            }
            _candiesActive[i].OnMove();
        }
    }

    private void AddScore(int score)
    {
        _score += score;
        GameEvents.RaiseScoreChanged(_score);
    }

    private void ResetScore()
    {
        _score = 0;
        GameEvents.RaiseScoreChanged(_score);
    }

    private void OnReset()
    {
        for (int i = _candiesActive.Count - 1; i >= 0; i--)
        {
            _candiesActive[i].OnReset();
            _candiesActive.RemoveAt(i);
        }

        ResetScore();
        UIManager.Instance.ShowUI<HomeScreen>();
    }

    [ContextMenu("Start Game")]
    public void StartGame()
    {
        _isPlaying = true;
        _soundController.InitSong(_songConfig.GetRandomClip());
        _leftCat.PlayStartAnimation();
        _rightCat.PlayStartAnimation();
        _noteIndex = 0;
    }
}
