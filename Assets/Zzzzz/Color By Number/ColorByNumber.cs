using UnityEngine;
using UnityEngine.UI;

public class ColorByNumber : MonoBehaviour
{
    public SpriteRenderer lineArtSpriteRenderer; 
    public SpriteRenderer numberedSpriteRenderer; 
    public Color[] colors;
    public Button[] colorButtons;

    private Texture2D coloredTexture;
    private int selectedColorIndex = 0; 

    void Start()
    {
        coloredTexture = new Texture2D(lineArtSpriteRenderer.sprite.texture.width, lineArtSpriteRenderer.sprite.texture.height);
        lineArtSpriteRenderer.material.mainTexture = coloredTexture;

        // Define your colors (make sure they correspond to the numbered regions)
        colors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow }; 

        for (int i = 0; i < colorButtons.Length; i++)
        {
            int buttonIndex = i; // Capture the loop variable
            colorButtons[i].onClick.AddListener(() => OnColorButtonClick(buttonIndex));
        }
    }

    void OnColorButtonClick(int buttonIndex)
    {
        selectedColorIndex = buttonIndex;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    void HandleMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null && hit.collider.gameObject == lineArtSpriteRenderer.gameObject)
        {
            // Get the texture from the lineArtRenderer
            Texture2D texture = lineArtSpriteRenderer.sprite.texture;

            // Calculate pixel coordinates based on texture size and hit point
            float u = hit.collider.bounds.center.x / hit.collider.bounds.size.x; // Get normalized x coordinate
            float v = hit.collider.bounds.center.y / hit.collider.bounds.size.y; // Get normalized y coordinate

            int x = Mathf.RoundToInt(u * texture.width);
            int y = Mathf.RoundToInt(v * texture.height); 

            Color numberedPixelColor = numberedSpriteRenderer.sprite.texture.GetPixel(x, y);
            int regionNumber = GetRegionNumberFromPixelColor(numberedPixelColor);

            if (regionNumber == selectedColorIndex + 1) // Assuming regions are numbered from 1
            {
                FloodFill(x, y, colors[selectedColorIndex]);
            }
        }
    }

    int GetRegionNumberFromPixelColor(Color pixelColor)
    {
        // Example: Assuming each region is a distinct color
        if (pixelColor == Color.red) return 1;
        if (pixelColor == Color.green) return 2;
        if (pixelColor == Color.blue) return 3;
        if (pixelColor == Color.yellow) return 4;
        return 0; // If no match is found
    }

    void FloodFill(int x, int y, Color fillColor)
    {
        // Basic flood fill implementation (you might need to optimize this)
        int width = lineArtSpriteRenderer.sprite.texture.width;
        int height = lineArtSpriteRenderer.sprite.texture.height;

        if (x < 0 || x >= width || y < 0 || y >= height) 
        {
            return;
        }

        Color originalColor = coloredTexture.GetPixel(x, y);

        if (originalColor != fillColor)
        {
            coloredTexture.SetPixel(x, y, fillColor);

            FloodFill(x + 1, y, fillColor); 
            FloodFill(x - 1, y, fillColor);
            FloodFill(x, y + 1, fillColor);
            FloodFill(x, y - 1, fillColor);
        }

        coloredTexture.Apply();
    }
}