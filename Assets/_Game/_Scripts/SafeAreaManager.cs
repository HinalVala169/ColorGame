using UnityEngine;

public class SafeAreaManager : MonoBehaviour
{
    public RectTransform safeAreaParent; // UI container that should stay inside the safe area
    public Canvas canvas; // Assign your Camera Space Canvas here

    private Vector2 lastScreenSize = Vector2.zero;

    void Start()
    {
        ApplySafeArea();
        InvokeRepeating(nameof(CheckResolutionChange), 0.5f, 0.5f); // Check resolution changes every 0.5s
    }

    void OnDestroy()
    {
        CancelInvoke(nameof(CheckResolutionChange)); // Stop checking when destroyed
    }

    void CheckResolutionChange()
    {
        if (Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y)
        {
            lastScreenSize = new Vector2(Screen.width, Screen.height);
            ApplySafeArea();
        }
    }

    void ApplySafeArea()
    {
        if (canvas == null || safeAreaParent == null)
        {
            Debug.LogWarning("SafeAreaManager: Canvas or SafeAreaParent is not assigned.");
            return;
        }

        Rect safeArea = Screen.safeArea;

        // Convert safe area to normalized anchor positions
        Vector2 minAnchor = new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height);
        Vector2 maxAnchor = new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);

        // Apply the safe area adjustments to the RectTransform
        safeAreaParent.anchorMin = minAnchor;
        safeAreaParent.anchorMax = maxAnchor;
        safeAreaParent.offsetMin = Vector2.zero;
        safeAreaParent.offsetMax = Vector2.zero;

        Debug.Log($"Safe Area Applied: min {minAnchor}, max {maxAnchor}");
    }
}
