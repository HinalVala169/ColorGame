using UnityEngine;
using UnityEngine.UI;

public class Magnifier : MonoBehaviour
{
    public Image magnifierImage; // The magnifier UI element (normal Image)
    public float zoomFactor = 2.0f; // Zoom level
    public Vector2 magnifierSize = new Vector2(100, 100); // Size of the magnifier
    private RectTransform magnifierRect;
    private bool isMagnifying = false;

    private float currentZoom = 1.0f; // To keep track of the current zoom level

    void Start()
    {
        magnifierRect = magnifierImage.GetComponent<RectTransform>();
        // magnifierImage.gameObject.SetActive(false); // Initially hide the magnifier
    }

    // Method to toggle magnification (called by the button)
    public void ToggleMagnify()
    {
        isMagnifying = !isMagnifying;
        magnifierImage.gameObject.SetActive(isMagnifying); // Show or hide the magnifier
    }

    // This method will be called to apply the zoom effect when clicked
    public void OnMagnify(Vector2 screenPosition)
    {
        if (isMagnifying)
        {
            // Convert screen position to local position in the parent rect
            Vector2 localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(magnifierRect.parent.GetComponent<RectTransform>(), screenPosition, null, out localPosition);

            // Update magnifier position
            magnifierRect.localPosition = localPosition;

            // Apply zoom scale
            magnifierRect.sizeDelta = magnifierSize * currentZoom;

            // Scale the Image to zoom in on the area
            magnifierImage.rectTransform.localScale = new Vector3(currentZoom, currentZoom, 1);

            // Optional: Adjust material properties if you have custom shaders
            if (magnifierImage.material != null)
            {
                magnifierImage.material.SetFloat("_Zoom", currentZoom); // Pass zoom to a custom shader here if needed
            }
        }
    }

    void Update()
    {
        // Debugging for mouse scroll input
        if (Input.mousePresent && !Input.touchSupported)
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            Debug.Log("Mouse Scroll Input: " + scrollInput); // Debug log for scroll input

            if (scrollInput != 0)
            {
                Debug.Log("Mouse Scroll Detected"); // Debug if scroll is detected
                Zoom(scrollInput);
            }
        }

        // Pinch to Zoom (For Mobile)
        if (Input.touchSupported)
        {
            if (Input.touchCount == 2)
            {
                // Get the distance between two touches
                float touchDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);

                // Debugging for pinch zoom
                Debug.Log("Pinch Distance: " + touchDistance);

                // If there's a difference in distance, zoom in or out
                if (Mathf.Abs(touchDistance - currentZoom) > 1.0f)
                {
                    float zoomDelta = touchDistance / currentZoom;
                    Debug.Log("Pinch Zoom Detected, zoomDelta: " + zoomDelta); // Debug log for pinch zoom
                    Zoom(zoomDelta);
                }
            }
        }
    }

    private void Zoom(float zoomDelta)
    {
        // Adjust zoom level based on mouse scroll or pinch
        Debug.Log("Zooming with delta: " + zoomDelta); // Debug the zoom delta
        currentZoom += zoomDelta * 0.1f; // Adjust the sensitivity if needed
        currentZoom = Mathf.Clamp(currentZoom, 0.5f, 5.0f); // Clamp the zoom to a range (optional)

        Debug.Log("Current Zoom: " + currentZoom); // Debug the current zoom level
    }
}
