using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PickSongScreen : UIBase
{
    [SerializeField] private Button _leftCard;
    [SerializeField] private Button _rightCard;

    [SerializeField] private RectTransform _leftCardRect;
    [SerializeField] private RectTransform _rightCardRect;
    private Sequence _pickSongSequence;

    private void Start()
    {
        _leftCard.onClick.AddListener(OnReset);
        _rightCard.onClick.AddListener(OnReset);
    }

    public override void Show()
    {
        base.Show();
        _pickSongSequence?.Kill();
        _pickSongSequence = DOTween.Sequence();
        _pickSongSequence.Append(_leftCardRect.DOScale(1.1f, 0.5f).SetEase(Ease.OutBack));
        _pickSongSequence.Append(_leftCardRect.DOScale(1f, 0.5f).SetEase(Ease.OutBack));

        _pickSongSequence.AppendInterval(1f);
        _pickSongSequence.Append(_rightCardRect.DOScale(1.1f, 0.5f).SetEase(Ease.OutBack));
        _pickSongSequence.Append(_rightCardRect.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
        _pickSongSequence.SetLoops(-1, LoopType.Yoyo);
        _pickSongSequence.Play();
    }

    public override void Hide()
    {
        base.Hide();
        _pickSongSequence?.Kill();
    }
    
    public void OnReset()
    {
        GameEvents.RaiseGameReset();
    }
    
    
}
