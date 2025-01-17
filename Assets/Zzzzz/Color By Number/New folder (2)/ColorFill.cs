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
        
        // Copy the initial coloring texture to the color layer texture
        colorLayerTexture.SetPixels(coloringTexture.GetPixels());
        colorLayerTexture.Apply();

        // Set the textures to the material
        regionFillMaterial.SetTexture("_MainTex", lineArtImage.sprite.texture);
        regionFillMaterial.SetTexture("_ColorTex", colorLayerTexture);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 localPoint;
            bool isInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(imageRect, mousePos, Camera.main, out localPoint);

            if (isInside && imageRect.rect.Contains(localPoint))
            {
                float xNormalized = (localPoint.x + imageRect.rect.width / 2) / imageRect.rect.width;
                float yNormalized = (localPoint.y + imageRect.rect.height / 2) / imageRect.rect.height;

                int x = Mathf.FloorToInt(xNormalized * colorLayerTexture.width);
                int y = Mathf.FloorToInt(yNormalized * colorLayerTexture.height);

                ApplyColor(x, y);
            }
        }
    }

    private void ApplyColor(int x, int y)
    {
        if (x < 0 || x >= colorLayerTexture.width || y < 0 || y >= colorLayerTexture.height)
            return;

        colorLayerTexture.SetPixel(x, y, colorToApply);
        colorLayerTexture.Apply();

        // Update the material's _ColorTex property to reflect the updated texture
        regionFillMaterial.SetTexture("_ColorTex", colorLayerTexture);
    }
}
