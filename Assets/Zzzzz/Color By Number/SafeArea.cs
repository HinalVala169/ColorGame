using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect lastSafeArea;
    private Vector2 lastScreenSize;
    private ScreenOrientation lastOrientation;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        lastScreenSize = new Vector2(Screen.width, Screen.height);
        lastOrientation = Screen.orientation;

        ApplySafeArea();
    }

    void Update()
    {
        // Check for changes in safe area, screen size, or orientation
        if (lastSafeArea != Screen.safeArea || 
            lastScreenSize.x != Screen.width || 
            lastScreenSize.y != Screen.height || 
            lastOrientation != Screen.orientation)
        {
            ApplySafeArea();
            lastScreenSize = new Vector2(Screen.width, Screen.height);
            lastOrientation = Screen.orientation;
        }
    }

    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        // Store the last safe area
        lastSafeArea = safeArea;

        // Convert safe area rectangle to normalized values
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // Apply the normalized anchors to the RectTransform
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;

        // Reset offsets to zero to prevent unwanted shifts
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
