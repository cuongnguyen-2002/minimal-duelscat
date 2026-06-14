using System;
using Spine;
using UnityEngine;

public class CatController : MonoBehaviour, ICandyEater
{
    [SerializeField] private CatAnimation _catAnimation;
    private float _speed = 5f;
    [SerializeField] private float _maxRange;
    [SerializeField] private float _minRange;
    
    public event Action<int> OnCatEatingCandy;

    private void Start()
    {
        _catAnimation.PlayIdleAnimation();
    }

    private void OnEnable()
    {
        GameEvents.OnGameComplete += PlayWinAnimation;
        GameEvents.OnGameOver += PlayLoseAnimation;
        GameEvents.OnGameReset += PlayIdleAnimation;
    }

    private void OnDisable()
    {
        GameEvents.OnGameOver -= PlayLoseAnimation;
        GameEvents.OnGameComplete -= PlayWinAnimation;
        GameEvents.OnGameReset -= PlayIdleAnimation;
        
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

    public void PlayLoseAnimation()
    {
        _catAnimation.PlayLoseAnimation();
    }

    public void PlayWinAnimation()
    {
        _catAnimation.PlayWinAnimation();
    }

    public void OnEatCandy(int score)
    {
        PlayEatAnimation();
        OnCatEatingCandy?.Invoke(score);
    }

    public void PlayIdleAnimation()
    {
        _catAnimation.PlayIdleAnimation();
    }

    private void CompleteEatAnimation(TrackEntry entry)
    {
        entry.Complete -= CompleteEatAnimation;
        PlayStartAnimation();
    }
}
