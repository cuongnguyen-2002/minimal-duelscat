using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using DG.Tweening.Core;

public class GameSequenceController : MonoBehaviour
{
    [SerializeField] private Transform pawTransform;
    [SerializeField] private Renderer vignetteRenderer;
    [SerializeField] private Ease vignetteEase = Ease.InElastic;

    [SerializeField] private float startScale = 0.3f;
    [SerializeField] private float endScale = 1.2f;
    [SerializeField] private float scaleDuration = 0.8f;
    [SerializeField] private float vignetteDuration = 0.5f;

    public UnityEvent onIntroReady;
    public UnityEvent onOutroReady;

    private MaterialPropertyBlock mpb;
    private static readonly int ProgressPID = Shader.PropertyToID("_Progress");
    private float vignetteProgress;

    private Sequence activeSequence;
    private DOGetter<float> vignetteGetter;
    private DOSetter<float> vignetteSetter;
    private TweenCallback cbIntroReady;
    private TweenCallback cbOutroReady;

    private void Awake()
    {
        mpb = new MaterialPropertyBlock();
        vignetteGetter = GetVignette;
        vignetteSetter = SetVignette;
        cbIntroReady = OnIntroReady;
        cbOutroReady = OnOutroReady;
        ResetState();
    }

    private void OnDestroy()
    {
        activeSequence?.Kill();
    }

    private float GetVignette() => vignetteProgress;

    private void SetVignette(float value)
    {
        vignetteProgress = value;
        if (vignetteRenderer == null) return;
        vignetteRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat(ProgressPID, value);
        vignetteRenderer.SetPropertyBlock(mpb);
    }

    public void PlayIntro()
    {
        activeSequence?.Kill();

        if (pawTransform)
        {
            pawTransform.gameObject.SetActive(true);
            pawTransform.localScale = Vector3.one * startScale;
        }
        SetVignette(0f);

        var seq = DOTween.Sequence();
        activeSequence = seq;

        if (pawTransform)
        {
            seq.Append(pawTransform.DOScale(endScale, scaleDuration * 0.5f)
                .SetEase(vignetteEase));
            seq.Append(pawTransform.DOScale(0f, scaleDuration * 0.5f)
                .SetEase(vignetteEase));
        }

        seq.Append(DOTween.To(vignetteGetter, vignetteSetter, 1f, vignetteDuration));
        seq.OnComplete(cbIntroReady);
    }

    private void OnIntroReady()
    {
        if (pawTransform) pawTransform.gameObject.SetActive(false);
        onIntroReady.Invoke();
    }

    public void PlayOutro()
    {
        activeSequence?.Kill();

        if (pawTransform)
        {
            pawTransform.gameObject.SetActive(true);
            pawTransform.localScale = Vector3.zero;
        }
        SetVignette(1f);

        var seq = DOTween.Sequence();
        activeSequence = seq;

        seq.Append(DOTween.To(vignetteGetter, vignetteSetter, 0f, vignetteDuration));

        if (pawTransform)
        {
            seq.Append(pawTransform.DOScale(endScale, scaleDuration * 0.5f)
                .SetEase(vignetteEase));
        }

        seq.OnComplete(cbOutroReady);
    }

    private void OnOutroReady()
    {
        onOutroReady.Invoke();
    }

    public void ResetState()
    {
        activeSequence?.Kill();
        if (pawTransform)
        {
            pawTransform.localScale = Vector3.zero;
            pawTransform.gameObject.SetActive(false);
        }
        SetVignette(0f);
    }
}
