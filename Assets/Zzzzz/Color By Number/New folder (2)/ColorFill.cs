using UnityEngine;
using UnityEngine.UI;

public class ColorFill : MonoBehaviour
{
    public Texture2D coloringTexture;   // The texture for coloring
    public Image lineArtImage;          // The Image component showing line art
    public Color colorToApply = Color.red; // Color to apply when a region is clicked
    public RectTransform imageRect;     // Reference to the RectTransform of the Image
    public Material regionFillMaterial; // Reference to the custom shader material

    private Texture2D colorLayerTexture; // Texture for the color layer

    private void Start()
    {
        // Assign the custom shader material to the Image component
        lineArtImage.material = regionFillMaterial;

        // Initialize the color layer texture with the same size as the coloring texture
        colorLayerTexture = new Texture2D(coloringTexture.width, coloringTexture.height, TextureFormat.RGBA32, false);
        
        // Set the initial color layer texture without modifying it
        colorLayerTexture.Apply();

        // Set the textures to the material
        regionFillMaterial.SetTexture("_MainTex", lineArtImage.sprite.texture);
        regionFillMaterial.SetTexture("_ColorTex", coloringTexture);

        // Debugging: Check the initial textures
        Debug.Log("MainTex and ColorTex applied to material.");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 localPoint;

            // Check if the click is inside the image bounds
            bool isInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(imageRect, mousePos, Camera.main, out localPoint);
            Debug.Log($"Mouse Position: {mousePos}, Local Point: {localPoint}, Inside Rect: {isInside}");

            if (isInside && imageRect.rect.Contains(localPoint))
            {
                float xNormalized = (localPoint.x + imageRect.rect.width / 2) / imageRect.rect.width;
                float yNormalized = (localPoint.y + imageRect.rect.height / 2) / imageRect.rect.height;

                int x = Mathf.FloorToInt(xNormalized * colorLayerTexture.width);
                int y = Mathf.FloorToInt(yNormalized * colorLayerTexture.height);

                // Debugging: Log the calculated pixel coordinates
                Debug.Log($"Click at normalized ({xNormalized}, {yNormalized}) -> Texture coords: ({x}, {y})");

                ApplyColor(x, y);
            }
        }
    }

    private void ApplyColor(int x, int y)
    {
        // Ensure the coordinates are within the bounds of the texture
        if (x < 0 || x >= colorLayerTexture.width || y < 0 || y >= colorLayerTexture.height)
        {
            Debug.LogWarning("Clicked outside texture bounds.");
            return;
        }

        // Set the color on the texture at the clicked location
        colorLayerTexture.SetPixel(x, y, colorToApply);
        colorLayerTexture.Apply();

        // Update the material's _ColorTex property to reflect the updated texture
        regionFillMaterial.SetTexture("_ColorTex", colorLayerTexture);

        // Debugging: Check if the texture update is applied
        Debug.Log("Color applied to texture at position: " + new Vector2(x, y));
    }
}
