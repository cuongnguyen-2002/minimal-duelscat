using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HomeScreen : UIBase
{
    [SerializeField] private Button _tabScreen;
    [SerializeField] private RectTransform _leftHand;
    [SerializeField] private RectTransform _rightHand;

    public override void Init(UIManager manager)
    {
        base.Init(manager);
        _tabScreen.onClick.AddListener(OnStartGame);
    }

    public override void Show()
    {
        base.Show();
        _canvasGroup.DOFade(1, 0);
        _leftHand.DOAnchorPosX(-365, 1f).SetLoops(-1 ,LoopType.Yoyo).SetEase(Ease.Linear);
        _rightHand.DOAnchorPosX(365, 1f).SetLoops(-1 ,LoopType.Yoyo).SetEase(Ease.Linear);
    }

    public override void Hide()
    {
        base.Hide();
        _canvasGroup.DOFade(0, 1);
        _leftHand.DOKill();
        _rightHand.DOKill();
    }

    public void OnStartGame()
    {
        Debug.Log("OnStartGame");
        GameEvents.RaiseGameStart();
    }
}
