using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIBase : MonoBehaviour
{
    protected CanvasGroup _canvasGroup;
    protected UIManager _uiManager;
    
    public virtual void Init(UIManager manager)
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _uiManager = manager;
    }
    
    public virtual void Show()
    {
        this.gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
