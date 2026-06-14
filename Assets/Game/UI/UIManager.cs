using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private List<UIBase> _uiList = new ();
    private UIBase _currentUI;
    void Start()
    {
        foreach (var ui in _uiList)
        {
            ui.Init(this);
        }

        ShowUI<HomeScreen>();
    }

    public void ShowUI<T>() where T : UIBase
    {
        var ui = GetUI<T>();
        if (ui == null)
        {
            Debug.LogError($"UI ${typeof(T).Name} not found");
            return;
        }
        _currentUI?.Hide();
        _currentUI = ui;
        ui.Show();  
    }

    public T GetUI<T>() where T : UIBase
    {
        foreach (var ui in _uiList)
        {
            if(ui is T @base) return @base;
        }
        
        return null;
    }

    public void ShowPickupSong()
    {
        ShowUI<PickSongScreen>();
    }
}
