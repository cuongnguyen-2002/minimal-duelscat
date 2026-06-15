using System;
using Spine;
using UnityEngine;

public class CatController : MonoBehaviour, ICandyEater
{
    [SerializeField] private CatAnimation _catAnimation;
    [SerializeField] private ComboSystem _comboSystem;
    private float _speed = 5f;
    [SerializeField] private float _maxRange;
    [SerializeField] private float _minRange;
    
    public event Action<int> OnCatEatingCandy;
    private bool _isComplete = false;

    private void Start()
    {
        _catAnimation.PlayIdleAnimation();
    }

    private void OnEnable()
    {
        GameEvents.OnGameComplete += Win;
        GameEvents.OnGameOver += Lose;
        GameEvents.OnGameReset += StartGame;
    }

    private void OnDisable()
    {
        GameEvents.OnGameOver -= Lose;
        GameEvents.OnGameComplete -= Win;
        GameEvents.OnGameReset -= StartGame;
        
    }

    public void Drag(float x)
    {
        float claimedX = Mathf.Clamp(x, _minRange, _maxRange);
        transform.position = 
            Vector3.Lerp(
                transform.position, 
                new Vector3(claimedX,transform.position.y,transform.position.z), 
                Time.deltaTime * _speed);
    }
    
    public void PlayStartAnimation()
    {
        _catAnimation.PlayStartAnimation();
    }
    
    public void PlayEatAnimation()
    {
        _catAnimation.PlayEatAnimation().Complete += CompleteEatAnimation;
    }

    public void Lose()
    {
        _isComplete = true;
        _catAnimation.PlayLoseAnimation();
    }

    public void Win()
    {
        _isComplete = true;
        _catAnimation.PlayWinAnimation();
    }

    public void OnEatCandy(int score)
    {
        if(_isComplete) return;
        PlayEatAnimation();
        OnCatEatingCandy?.Invoke(score);
        _comboSystem.PlayCombo();
    }

    public void StartGame()
    {
        _isComplete = false;
        _catAnimation.PlayIdleAnimation();
    }

    private void CompleteEatAnimation(TrackEntry entry)
    {
        entry.Complete -= CompleteEatAnimation;
        
        if(_isComplete) return;
        PlayStartAnimation();
    }
}
