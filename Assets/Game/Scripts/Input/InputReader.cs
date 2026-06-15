using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ScreenOrientation { Portrait, Landscape } 

public class InputReader : MonoBehaviour , 
    IPointerUpHandler, 
    IPointerDownHandler, 
    IDragHandler
{
    [SerializeField] private Image _touchArea;
    private Camera _cameraMain;
    private Vector3 _worldPoint;
    private float _widthScreen;
    public event Action<float> LeftCatMove;
    public event Action<float> RightCatMove;

    private int _leftPointId;
    private int _rightPointId;
    private const int NO_POINTER = -1;

    private void Start()
    {
        _cameraMain = Camera.main;
    }

    private void OnEnable()
    {
        GameEvents.OnGameStart += OnStart;
        GameEvents.OnGameComplete += OnReset;
        GameEvents.OnGameOver += OnReset;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStart -= OnStart;
        GameEvents.OnGameComplete -= OnReset;
        GameEvents.OnGameOver -= OnReset;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if (eventData.position.x <= Screen.width * 0.5f)
        {
            _leftPointId = eventData.pointerId;
            Debug.Log("left point: " + _leftPointId);
        }
        else
        {
            _rightPointId = eventData.pointerId;
            Debug.Log("Right point: " + _leftPointId);
        }
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        float normalizedDelta = eventData.delta.x / Screen.width;
        float worldX = ScreenToWorldX(eventData.position.x);
        if (eventData.pointerId == _leftPointId)
        {
            LeftCatMove?.Invoke(worldX);
        }
        else if (eventData.pointerId == _rightPointId)
        {
            RightCatMove?.Invoke(worldX);
        }
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if(eventData.pointerId == _leftPointId) _leftPointId = NO_POINTER;
        if (eventData.pointerId == _rightPointId) _rightPointId = NO_POINTER;

    }

    private float ScreenToWorldX(float screenX)
    {
        Vector3 worldPos = _cameraMain.ScreenToWorldPoint(
            new Vector3(screenX, Screen.height * 0.5f, _cameraMain.nearClipPlane)
        );
        return worldPos.x;
    }

    private void OnReset()
    {
        _leftPointId = NO_POINTER;
        _rightPointId = NO_POINTER;
        _touchArea.raycastTarget = false;
    }

    private void OnStart()
    {
        _leftPointId = NO_POINTER;
        _rightPointId = NO_POINTER;
        _touchArea.raycastTarget = true;
    }
}
