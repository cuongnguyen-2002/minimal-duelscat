using DG.Tweening;
using TMPro;
using UnityEngine;

public class ComboSystem : MonoBehaviour
{
    [SerializeField] private TextMeshPro _textCombo;
    [SerializeField] private string[] _comboNames;

    private Sequence _comboTween;
    [Header("Combo Amin Config")]
    [SerializeField] private float _duration = 0.3f;
    [SerializeField] private float _scaleFadeIn = 1.2f;
    [SerializeField] private float _scaleFadeOut = 0f;
    [SerializeField] private float _textFadeIn = 1f;
    [SerializeField] private float _textFadeOut = 0f;
    
    public void PlayCombo()
    {
        
        _textCombo.text = GetRandomCombo();
        
        _comboTween?.Kill();
        _comboTween = DOTween.Sequence();
        ResetCombo();
        _comboTween.Append(FadeInCombo());
        _comboTween.Append(FadeOutCombo());
        _comboTween.Play();
    }
    
    private Tween FadeInCombo()
    {
        Sequence comboTween = DOTween.Sequence();
        comboTween.Append(_textCombo.transform.DOScale(_scaleFadeIn, _duration).SetEase(Ease.Linear));
        comboTween.Join(_textCombo.DOFade(_textFadeIn, _duration).SetEase(Ease.Linear));
        return comboTween;
    }
    
    private Tween FadeOutCombo()
    {
        Sequence comboTween = DOTween.Sequence();
        comboTween.Append(_textCombo.transform.DOScale(_scaleFadeOut, _duration).SetEase(Ease.Linear));
        comboTween.Join(_textCombo.DOFade(_textFadeOut, _duration).SetEase(Ease.Linear));
        return comboTween;
    }

    private void ResetCombo()
    {
        _textCombo.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        _textCombo.color = new Color(1, 1, 1, 0);
    }
    
    private string GetRandomCombo()
    {
        int index = Random.Range(0, _comboNames.Length);
        return _comboNames[index];
    }
}
