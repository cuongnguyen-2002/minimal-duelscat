using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HomeScreen : UIBase
{
    [SerializeField] private Button _tabScreen;

    public override void Init(UIManager manager)
    {
        base.Init(manager);
        _tabScreen.onClick.AddListener(OnStartGame);
    }

    public override void Show()
    {
        base.Show();
        _canvasGroup.DOFade(1, 0);
        
    }

    public override void Hide()
    {
        base.Hide();
        _canvasGroup.DOFade(0, 1);
    }

    public void OnStartGame()
    {
        Debug.Log("OnStartGame");
        GameEvents.RaiseGameStart();
    }
}
