using System;
using UnityEngine;

public class Candy :MonoBehaviour
{
    [SerializeField] private SpriteRenderer _visual;
    
    private float _elapsedTime = 0f;
    private float _fallingSpeed;
    private float _missAfter;
    private bool _isDead = false;
    private int _score;
    public bool IsOutRange => _elapsedTime >= _missAfter;

    public event Action<Candy> OnComplete;

    public void Init(Sprite sprite, int score)
    {
        _score = score;
        _visual.sprite = sprite;
    }

    public void Init(float hitTime, float fallSpeed, float missWindows)
    {
        _fallingSpeed = fallSpeed;
        _missAfter = hitTime + missWindows;
        _elapsedTime = 0f;
        _isDead = false;
    }

    public void OnMove()
    {
        if(_isDead) return;
        _elapsedTime += Time.deltaTime;
        transform.position += Vector3.down * _fallingSpeed * Time.deltaTime;
    }

    private void Die()
    {
        _isDead = true;
        transform.localPosition = Vector2.zero;
        OnComplete?.Invoke(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var candyEater = other.GetComponent<ICandyEater>();
        if (candyEater != null)
        {
            GameEvents.RequestSpawnVFX(VFXType.Eater, transform.position);
            candyEater.OnEatCandy(_score);
            Die();
        }
    }

    public void OnReset()
    {
        Die();
    }
}
