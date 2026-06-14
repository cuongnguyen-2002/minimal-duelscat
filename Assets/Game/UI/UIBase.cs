using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIBase : MonoBehaviour
{
    protected CanvasGroup _canvasGroup;
    protected UIManager _uiManager;
    private float _fadeDuration = 0.5f;
    private Tween _canvasGroupTween;
    
    public virtual void Init(UIManager manager)
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _uiManager = manager;
        Hide();
    }
    
    public virtual void Show()
    {
        _canvasGroupTween?.Kill();
        _canvasGroupTween = _canvasGroup.DOFade(1, _fadeDuration)
            .OnComplete(() =>_canvasGroup.interactable = true);
        this.gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        _canvasGroupTween?.Kill();
        _canvasGroup.interactable = false;
        _canvasGroupTween = _canvasGroup.DOFade(0, _fadeDuration)
            .OnComplete(() => this.gameObject.SetActive(false));
    }
}
