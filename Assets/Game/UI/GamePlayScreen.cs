using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class GamePlayScreen : UIBase
{
    private int _lastScore = 0;
    [SerializeField] private TextMeshProUGUI _scoreText;
    private Tween _scoreTween;

    private void OnEnable()
    {
        GameEvents.OnScoreChanged += ScoreChange;
        ScoreChange(0);
    }

    private void OnDisable()
    {
        GameEvents.OnScoreChanged -= ScoreChange;
    }

    private void ScoreChange(int score)
    {
        _lastScore = score;
        _scoreText.text = score.ToString();
        _scoreTween?.Kill();
        _scoreTween = DOTween.To(() => _lastScore, x =>
        {
            _lastScore = x;
        }, score, 0.1f);
    }
}
