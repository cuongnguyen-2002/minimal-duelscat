using System;
using UnityEngine;
using UnityEngine.Events;

public enum ScreenOrientation { Portrait, Landscape } 

public class InputReader : MonoBehaviour
{
    private Camera _cameraMain;
    private Vector3 _worldPoint;
    private float _widthScreen;
    public event Action<float> LeftCatMove;
    public event Action<float> RightCatMove;
    public UnityEvent<ScreenOrientation> DeviceOrientationCallback;

    private void Start()
    {
        _cameraMain = Camera.main;
    }

    public void InpuHandler()
    {
        MouseInput();
    }

    private void MouseInput()
    {
        if(!Input.GetMouseButton(0)) return;
        _worldPoint = _cameraMain.ScreenToWorldPoint(Input.mousePosition);

        if (_worldPoint.x <= 0)
        {
            LeftCatMove?.Invoke(_worldPoint.x);
        }
        else
        {
            RightCatMove?.Invoke(_worldPoint.x);
        }
    }
}
