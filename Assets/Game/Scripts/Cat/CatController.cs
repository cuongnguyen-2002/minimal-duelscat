using System;
using Spine;
using UnityEngine;

public class CatController : MonoBehaviour, ICandyEater
{
    [SerializeField] private CatAnimation _catAnimation;
    private float _speed = 5f;
    [SerializeField] private float _maxRange;
    [SerializeField] private float _minRange;

    private void Start()
    {
        _catAnimation.PlayIdleAnimation();
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

    public void OnEatCandy()
    {
        PlayEatAnimation();
    }

    private void CompleteEatAnimation(TrackEntry entry)
    {
        entry.Complete -= CompleteEatAnimation;
        PlayStartAnimation();
    }
}
