using UnityEngine;

public abstract class BaseUICanvas : MonoBehaviour
{
    [SerializeField] private CanvasType canvasType;

    public CanvasType CanvasType => canvasType;

    private Canvas _canvas;

    private void Awake()
    {
        // Cache the Canvas component
        _canvas = GetComponent<Canvas>();
        if (_canvas == null)
        {
            Debug.LogError("No Canvas component found on this GameObject.");
        }
    }

    public virtual void Show()
    {
        if (_canvas != null)
        {
            _canvas.enabled = true;
        }
    }

    public virtual void Hide()
    {
        if (_canvas != null)
        {
            _canvas.enabled = false;
        }
    }
}
