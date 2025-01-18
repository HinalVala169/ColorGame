using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ColorFillScript : MonoBehaviour
{
    public Texture2D maskTex;
    public float colorTolerance = 0.1f;
    public Material fillMaterial;
    public Image imageComponent;

    private Texture2D mainTexture;
    private Texture2D colorTexture;

    private GraphicRaycaster raycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;

    void Start()
    {
        mainTexture = maskTex;
        colorTexture = new Texture2D(mainTexture.width, mainTexture.height);
        colorTexture.SetPixels(new Color[mainTexture.width * mainTexture.height]);
        colorTexture.Apply();

        imageComponent.material.mainTexture = mainTexture;
        Debug.Log("Start: Image material set to base texture.");

        // Set up EventSystem and GraphicRaycaster
        raycaster = GetComponentInParent<GraphicRaycaster>();
        eventSystem = EventSystem.current;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button clicked
        {
            Vector2 pixelUV = GetMouseUV();
            if (pixelUV != Vector2.zero) // Only trigger if valid
            {
                Debug.Log($"Mouse Clicked at: {pixelUV}");
                FillRegionWithColor((int)pixelUV.x, (int)pixelUV.y);
            }
            else
            {
                Debug.Log("Mouse click did not hit a valid texture.");
            }
        }
    }

    Vector2 GetMouseUV()
    {
        pointerEventData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        // Use the Raycast to check for the Image component
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(pointerEventData.position), Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject == imageComponent.gameObject)
        {
            // Map mouse position to the texture's UV coordinates
            RectTransform rectTransform = imageComponent.rectTransform;
            Vector2 localPos = pointerEventData.position - (Vector2)rectTransform.position;
            Vector2 uv = new Vector2(localPos.x / rectTransform.rect.width, localPos.y / rectTransform.rect.height);

            // Map the UV coordinates to the texture coordinates
            uv.x *= mainTexture.width;
            uv.y *= mainTexture.height;

            // Ensure UV coordinates are within bounds
            uv.x = Mathf.Clamp(uv.x, 0, mainTexture.width - 1);
            uv.y = Mathf.Clamp(uv.y, 0, mainTexture.height - 1);

            Debug.Log($"Mouse UV: {uv}");
            return uv;
        }
        return Vector2.zero; // Return invalid if no hit
    }

    void FillRegionWithColor(int x, int y)
    {
        Debug.Log($"Started Fill at: {new Vector2(x, y)}");

        // Ensure the coordinates are within bounds before accessing the texture
        if (x < 0 || x >= mainTexture.width || y < 0 || y >= mainTexture.height)
        {
            Debug.LogError("Click position is outside the texture bounds.");
            return;
        }

        Color[] colors = mainTexture.GetPixels();
        Color startColor = colors[(y * mainTexture.width) + x];

        Queue<Vector2Int> pixelsToFill = new Queue<Vector2Int>();
        pixelsToFill.Enqueue(new Vector2Int(x, y));
        bool[] filledPixels = new bool[mainTexture.width * mainTexture.height];

        int fillCount = 0;
        while (pixelsToFill.Count > 0)
        {
            Vector2Int pixel = pixelsToFill.Dequeue();
            int index = (pixel.y * mainTexture.width) + pixel.x;

            if (filledPixels[index]) continue;

            Color currentColor = colors[index];
            if (IsWithinTolerance(currentColor, startColor))
            {
                colorTexture.SetPixel(pixel.x, pixel.y, Color.white); // Filling with white or another fixed color
                filledPixels[index] = true;
                fillCount++;

                EnqueueIfValid(pixel.x + 1, pixel.y, pixelsToFill); // Right
                EnqueueIfValid(pixel.x - 1, pixel.y, pixelsToFill); // Left
                EnqueueIfValid(pixel.x, pixel.y + 1, pixelsToFill); // Up
                EnqueueIfValid(pixel.x, pixel.y - 1, pixelsToFill); // Down
            }
        }

        colorTexture.Apply();
        Debug.Log($"Filled {fillCount} pixels.");
        UpdateMaterial(); // Ensure material update after the fill
    }

    bool IsWithinTolerance(Color color1, Color color2)
    {
        bool result = Mathf.Abs(color1.r - color2.r) < colorTolerance &&
                      Mathf.Abs(color1.g - color2.g) < colorTolerance &&
                      Mathf.Abs(color1.b - color2.b) < colorTolerance;
        if (result)
        {
            Debug.Log("Color match found within tolerance.");
        }
        return result;
    }

    void EnqueueIfValid(int x, int y, Queue<Vector2Int> queue)
    {
        if (x >= 0 && x < mainTexture.width && y >= 0 && y < mainTexture.height)
        {
            queue.Enqueue(new Vector2Int(x, y));
        }
    }

    void UpdateMaterial()
    {
        fillMaterial.SetTexture("_MainTex", mainTexture);
        fillMaterial.SetTexture("_ColorTex", colorTexture);
        fillMaterial.SetFloat("_Tolerance", colorTolerance);

        imageComponent.material = fillMaterial; // Update the Image component's material
        Debug.Log("Material updated with filled color texture.");
    }
}
