using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private List<UIBase> _uiList = new List<UIBase>();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var ui in _uiList)
        {
            ui.Init(this);
        }

        ShowUI<HomeScreen>().Show();
    }

    public T ShowUI<T>() where T : UIBase
    {
        foreach (var ui in _uiList)
        {
            if (ui is T) return (T)ui;
        }
        return null;
    }
}
