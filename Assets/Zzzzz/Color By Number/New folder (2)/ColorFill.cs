using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // For Queue<T>

public class ColorFillScript : MonoBehaviour
{
    public Texture2D maskTex; // Texture used for locking the region
    public Color targetColor = Color.yellow; // The color of the region to fill
    public float colorTolerance = 0.1f; // Color matching tolerance
    public Material fillMaterial; // Material with RegionFillShader already applied
    public Image imageComponent; // Reference to the Image component displaying the texture

    private Texture2D mainTexture;
    private Texture2D colorTexture;

    void Start()
    {
        mainTexture = maskTex; // Set the base texture
        colorTexture = new Texture2D(mainTexture.width, mainTexture.height);
        colorTexture.SetPixels(new Color[mainTexture.width * mainTexture.height]); // Initialize the color texture
        colorTexture.Apply();
        
        // Ensure the Image component initially displays the base texture
        imageComponent.material.mainTexture = mainTexture;
        Debug.Log("Start: Image material set to base texture.");
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

    // Get the mouse UV coordinates on the texture
    Vector2 GetMouseUV()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector2 uv = hit.textureCoord;
            uv.x *= mainTexture.width;
            uv.y *= mainTexture.height;
            Debug.Log($"Mouse UV: {uv}");
            return uv;
        }
        return Vector2.zero; // Return invalid if no hit
    }

    // Fill the region with the target color
    void FillRegionWithColor(int x, int y)
    {
        Debug.Log($"Started Fill at: {new Vector2(x, y)}");

        Color[] colors = mainTexture.GetPixels();
        Color startColor = colors[(y * mainTexture.width) + x];

        Queue<Vector2Int> pixelsToFill = new Queue<Vector2Int>();
        pixelsToFill.Enqueue(new Vector2Int(x, y));
        bool[] filledPixels = new bool[mainTexture.width * mainTexture.height]; // To avoid processing the same pixel twice

        int fillCount = 0;
        while (pixelsToFill.Count > 0)
        {
            Vector2Int pixel = pixelsToFill.Dequeue();
            int index = (pixel.y * mainTexture.width) + pixel.x;

            if (filledPixels[index]) continue; // Skip already filled pixels

            Color currentColor = colors[index];
            if (IsWithinTolerance(currentColor, startColor))
            {
                colorTexture.SetPixel(pixel.x, pixel.y, targetColor); // Fill with color
                filledPixels[index] = true;
                fillCount++;

                // Add adjacent pixels to the queue
                EnqueueIfValid(pixel.x + 1, pixel.y, pixelsToFill); // Right
                EnqueueIfValid(pixel.x - 1, pixel.y, pixelsToFill); // Left
                EnqueueIfValid(pixel.x, pixel.y + 1, pixelsToFill); // Up
                EnqueueIfValid(pixel.x, pixel.y - 1, pixelsToFill); // Down
            }
        }

        colorTexture.Apply(); // Apply the color changes immediately
        Debug.Log($"Filled {fillCount} pixels.");
        UpdateMaterial(); // Update the material with the filled color texture
    }

    // Check if the color is within the tolerance range
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

    // Enqueue the valid pixel position if it's within bounds
    void EnqueueIfValid(int x, int y, Queue<Vector2Int> queue)
    {
        if (x >= 0 && x < mainTexture.width && y >= 0 && y < mainTexture.height)
        {
            queue.Enqueue(new Vector2Int(x, y));
        }
    }

    // Update the shader material with the filled color texture
    void UpdateMaterial()
    {
        fillMaterial.SetTexture("_MainTex", mainTexture); // Set the base texture
        fillMaterial.SetTexture("_ColorTex", colorTexture); // Set the filled color texture
        fillMaterial.SetColor("_TargetColor", targetColor); // Set the target color
        fillMaterial.SetFloat("_Tolerance", colorTolerance); // Set the tolerance for color matching

        // Apply the updated material to the Image component
        imageComponent.material = fillMaterial;
        Debug.Log("Material updated with filled color texture.");
    }
}
