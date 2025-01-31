using UnityEngine;
using UnityEngine.UI;

public class UIAutoScaler : MonoBehaviour
{
    private Canvas canvas; // Canvas reference
    private RectTransform canvasRect; // Canvas RectTransform

    void Start()
    {
        canvas = FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("Canvas not found! Make sure your UI has a Canvas.");
            return;
        }

        canvasRect = canvas.GetComponent<RectTransform>();
        AdjustAllImages();
    }

    void AdjustAllImages()
    {
        // Find all UI Images in the scene
        Image[] images = FindObjectsOfType<Image>();

        foreach (Image img in images)
        {
            RectTransform imgRect = img.GetComponent<RectTransform>();

            if (imgRect == null) continue;

            // Adjust size to match canvas dimensions
            imgRect.sizeDelta = new Vector2(canvasRect.rect.width, canvasRect.rect.height);
        }
    }
}
