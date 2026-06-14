using System;
using UnityEngine;

public class DetectScreen : MonoBehaviour
{
    [SerializeField] private Sprite _portraitBackground;
    [SerializeField] private Sprite _landscapeBackground;
    [SerializeField] private SpriteRenderer _visual;
    private ScreenOrientation _currentOrientation;

    private void Start()
    {
        OnScreenChange(ScreenOrientation.Portrait);
    }

    public void OnScreenChange(ScreenOrientation orientation)
    {
        if (_currentOrientation == orientation) return; 
        _currentOrientation = orientation;
        
        var sprite = orientation == ScreenOrientation.Landscape ? 
            _landscapeBackground : 
            _portraitBackground;
        _visual.sprite = sprite;
    }

    private void Update()
    {
        if (Screen.width >= Screen.height)
        {
            OnScreenChange(ScreenOrientation.Landscape);
        }
        else
        {
            OnScreenChange(ScreenOrientation.Portrait);
        }
    }
}
