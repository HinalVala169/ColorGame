using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorFill_ : MonoBehaviour, IPointerDownHandler
{
    public Texture2D lineArtTexture;   // The line art texture (uncolored image)
    public Texture2D coloringTexture;  // The texture for coloring (initially transparent)
    public Color[] colors;             // Array of colors to use
    public int regionSize = 10;        // Size of the region to color when clicked

    private Image uiImage;             // Reference to the UI Image component
    private Texture2D textureCopy;     // Copy of the coloring texture to apply changes
    private bool isInitialized = false; // Flag to check if initialization is done

    private Color selectedColor;   
    void Start()
    {
        // Get the UI Image component attached to this GameObject
        uiImage = GetComponent<Image>();

        if (lineArtTexture != null && coloringTexture != null)
        {
            // Create a copy of the coloring texture to modify
            textureCopy = new Texture2D(coloringTexture.width, coloringTexture.height);
            textureCopy.SetPixels(coloringTexture.GetPixels());
            textureCopy.Apply();

            // Set the Image component with the base line art texture initially
            uiImage.sprite = Sprite.Create(lineArtTexture, new Rect(0, 0, lineArtTexture.width, lineArtTexture.height), new Vector2(0.5f, 0.5f));

            isInitialized = true;
        }
    }

    public void SetSelectedColor(Color color)
    {
        selectedColor = color;
    }
    

    // This method will be triggered when the user clicks on the Image
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isInitialized) return;

        // Get the mouse position (in the screen space)
        Vector2 localPoint = uiImage.rectTransform.InverseTransformPoint(eventData.position);
        Rect spriteRect = uiImage.sprite.rect;
        Vector2 pivotAdjustedPoint = new Vector2(localPoint.x + spriteRect.width / 2, localPoint.y + spriteRect.height / 2);
        Vector2 normalizedPoint = new Vector2(pivotAdjustedPoint.x / spriteRect.width, pivotAdjustedPoint.y / spriteRect.height);

        // Convert to texture coordinates
        Vector2 textureCoord = new Vector2(normalizedPoint.x * coloringTexture.width, normalizedPoint.y * coloringTexture.height);

        // Select a color (you can modify this to let the user choose)
        Color selectedColor = colors[Random.Range(0, colors.Length)];  // Randomly choose a color for simplicity

        // Fill the clicked region with the selected color
        FillRegion((int)textureCoord.x, (int)textureCoord.y, selectedColor);

        // Apply the coloring texture over the line art texture
        ApplyCombinedTexture();
    }

    void FillRegion(int x, int y, Color color)
    {
        // Determine the bounds of the region to fill (based on region size)
        int startX = Mathf.Clamp(x - regionSize / 2, 0, textureCopy.width - 1);
        int startY = Mathf.Clamp(y - regionSize / 2, 0, textureCopy.height - 1);
        int endX = Mathf.Clamp(x + regionSize / 2, 0, textureCopy.width - 1);
        int endY = Mathf.Clamp(y + regionSize / 2, 0, textureCopy.height - 1);

        // Fill the region with the selected color (only the defined region)
        for (int i = startX; i <= endX; i++)
        {
            for (int j = startY; j <= endY; j++)
            {
                textureCopy.SetPixel(i, j, color);
            }
        }

        textureCopy.Apply();  // Apply the color changes to the texture
    }

    void ApplyCombinedTexture()
    {
        // Create a new texture that combines the line art and the coloring texture
        Texture2D combinedTexture = new Texture2D(lineArtTexture.width, lineArtTexture.height);

        for (int x = 0; x < lineArtTexture.width; x++)
        {
            for (int y = 0; y < lineArtTexture.height; y++)
            {
                // Combine line art with the coloring texture
                Color lineArtColor = lineArtTexture.GetPixel(x, y);
                Color fillColor = textureCopy.GetPixel(x, y);

                // If the fill color is not fully transparent, use the fill color; otherwise, use the line art color
                combinedTexture.SetPixel(x, y, fillColor.a > 0 ? fillColor : lineArtColor);
            }
        }

        combinedTexture.Apply();

        // Set the Image component's sprite with the combined texture
        uiImage.sprite = Sprite.Create(combinedTexture, new Rect(0, 0, combinedTexture.width, combinedTexture.height), new Vector2(0.5f, 0.5f));
    }
}
