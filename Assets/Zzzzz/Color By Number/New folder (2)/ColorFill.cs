using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ColorFill : MonoBehaviour
{
    public Texture2D baseTex; // Single texture for both base and mask
    public Texture2D maskNumberTex;
    public Color paintColor = Color.white; // The color to fill

    public List<Color> availableColors;
    public float colorTolerance = 0.1f; // Tolerance for color matching
    public Material fillMaterial;
    public Image imageComponent;

    private GraphicRaycaster raycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;

    private byte[] texPixels; // To store the pixel data of the base texture

    private int texWidth;
    private int texHeight;

    private Material instanceMaterial;

    private Texture2D duplicateTex, duplicatedMaskTex; // Duplicate texture for painting

    void Start()
    {
        // Duplicate the base texture to work with
        texWidth = baseTex.width;
        texHeight = baseTex.height;

        texPixels = new byte[texWidth * texHeight * 4];

        // Clone the base texture into the duplicate texture
        duplicateTex = new Texture2D(baseTex.width, baseTex.height);
        duplicateTex.SetPixels(baseTex.GetPixels());
        duplicateTex.Apply();

        duplicatedMaskTex = new Texture2D(maskNumberTex.width, maskNumberTex.height);
        duplicatedMaskTex.SetPixels(maskNumberTex.GetPixels());
        duplicatedMaskTex.Apply();

        Color[] baseTexColors = baseTex.GetPixels();
        for (int i = 0; i < baseTexColors.Length; i++)
        {
            texPixels[i * 4 + 0] = (byte)(baseTexColors[i].r * 255);
            texPixels[i * 4 + 1] = (byte)(baseTexColors[i].g * 255);
            texPixels[i * 4 + 2] = (byte)(baseTexColors[i].b * 255);
            texPixels[i * 4 + 3] = (byte)(baseTexColors[i].a * 255);
        }

        // Create a new instance of the material
        instanceMaterial = new Material(fillMaterial) { name = fillMaterial.name + "InstanceMaterial_" };
        instanceMaterial.mainTexture = duplicateTex; // Use the duplicate texture
        instanceMaterial.SetTexture("_MaskTex", duplicatedMaskTex);
        imageComponent.material = instanceMaterial;

        instanceMaterial.SetFloat("_RevealAndMask", 1f);
        instanceMaterial.SetFloat("_RegionNumber", 0f); // Default region number (No region selected)
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button clicked
        {
            Vector2 pixelUV = GetMouseUV();
            if (pixelUV != Vector2.zero) // Only trigger if valid
            {
                Debug.Log($"Mouse Clicked at: {pixelUV}");
                RevealClickedRegion(pixelUV); // Handle the clicked region
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

        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(pointerEventData.position), Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject == imageComponent.gameObject)
        {
            RectTransform rectTransform = imageComponent.rectTransform;

            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, pointerEventData.position, Camera.main, out localPos);

            Vector2 pivotAdjustedPos = localPos + (rectTransform.rect.size * rectTransform.pivot);

            Vector2 uv = new Vector2(
                pivotAdjustedPos.x / rectTransform.rect.width,
                pivotAdjustedPos.y / rectTransform.rect.height
            );

            uv.x *= baseTex.width;
            uv.y *= baseTex.height;

            uv.x = Mathf.Clamp(uv.x, 0, baseTex.width - 1);
            uv.y = Mathf.Clamp(uv.y, 0, baseTex.height - 1);

            return uv;
        }
        return Vector2.zero;
    }

   void RevealClickedRegion(Vector2 clickedUV)
{
    // Get the pixel position from the UV coordinates
    int x = Mathf.FloorToInt(clickedUV.x);
    int y = Mathf.FloorToInt(clickedUV.y);

    // Get the color from the mask texture at the clicked position
    Color maskColor = duplicatedMaskTex.GetPixel(x, y);

    // Check if the color in the mask matches the selected paint color
    if (IsColorMatch(maskColor, paintColor))
    {
        // Flood-fill the region based on the color at the clicked position
        int regionNumber = FloodFill(x, y);

        // Set the region number in the material
        instanceMaterial.SetFloat("_RegionNumber", regionNumber);

        // Update the duplicate texture after filling
        UpdateTexture();
    }
    else
    {
        Debug.Log("Mask color does not match the selected paint color.");
    }
}

    void UpdateTexture()
    {
        Color[] updatedColors = new Color[texWidth * texHeight];
        for (int i = 0; i < texWidth; i++)
        {
            for (int j = 0; j < texHeight; j++)
            {
                int pixelIndex = (j * texWidth + i) * 4;
                updatedColors[j * texWidth + i] = new Color(
                    texPixels[pixelIndex] / 255f,
                    texPixels[pixelIndex + 1] / 255f,
                    texPixels[pixelIndex + 2] / 255f,
                    texPixels[pixelIndex + 3] / 255f
                );
            }
        }

        duplicateTex.SetPixels(updatedColors); // Apply changes to the duplicate texture
        duplicateTex.Apply(); // Apply the changes to the duplicate texture
    }

   private int FloodFill(int x, int y)
{
    // Get the color from the mask texture at the clicked position
    Color maskColor = duplicatedMaskTex.GetPixel(x, y);

    // Compare the color from the mask texture with the selected paint color
    if (!IsColorMatch(maskColor, paintColor))
        return 0; // Skip if colors do not match

    // Get the initial color at the clicked position on the base texture
    byte hitColorR = texPixels[((texWidth * y) + x) * 4 + 0];
    byte hitColorG = texPixels[((texWidth * y) + x) * 4 + 1];
    byte hitColorB = texPixels[((texWidth * y) + x) * 4 + 2];
    byte hitColorA = texPixels[((texWidth * y) + x) * 4 + 3];

    if (paintColor.r * 255 == hitColorR && paintColor.g * 255 == hitColorG && paintColor.b * 255 == hitColorB && paintColor.a * 255 == hitColorA)
        return 0; // Skip if the color is already the same

    Queue<int> fillPointX = new Queue<int>();
    Queue<int> fillPointY = new Queue<int>();
    fillPointX.Enqueue(x);
    fillPointY.Enqueue(y);

    int ptsx, ptsy;
    int pixel = 0;

    int regionNumber = 0;

    while (fillPointX.Count > 0)
    {
        ptsx = fillPointX.Dequeue();
        ptsy = fillPointY.Dequeue();

        if (ptsy - 1 >= 0) // down
        {
            pixel = (texWidth * (ptsy - 1) + ptsx) * 4;
            if (CompareThreshold(texPixels[pixel + 0], hitColorR)
                && CompareThreshold(texPixels[pixel + 1], hitColorG)
                && CompareThreshold(texPixels[pixel + 2], hitColorB)
                && CompareThreshold(texPixels[pixel + 3], hitColorA))
            {
                fillPointX.Enqueue(ptsx);
                fillPointY.Enqueue(ptsy - 1);
                DrawPoint(pixel);
            }
        }

        if (ptsx + 1 < texWidth) // right
        {
            pixel = (texWidth * ptsy + ptsx + 1) * 4;
            if (CompareThreshold(texPixels[pixel + 0], hitColorR)
                && CompareThreshold(texPixels[pixel + 1], hitColorG)
                && CompareThreshold(texPixels[pixel + 2], hitColorB)
                && CompareThreshold(texPixels[pixel + 3], hitColorA))
            {
                fillPointX.Enqueue(ptsx + 1);
                fillPointY.Enqueue(ptsy);
                DrawPoint(pixel);
            }
        }

        if (ptsx - 1 >= 0) // left
        {
            pixel = (texWidth * ptsy + ptsx - 1) * 4;
            if (CompareThreshold(texPixels[pixel + 0], hitColorR)
                && CompareThreshold(texPixels[pixel + 1], hitColorG)
                && CompareThreshold(texPixels[pixel + 2], hitColorB)
                && CompareThreshold(texPixels[pixel + 3], hitColorA))
            {
                fillPointX.Enqueue(ptsx - 1);
                fillPointY.Enqueue(ptsy);
                DrawPoint(pixel);
            }
        }

        if (ptsy + 1 < texHeight) // up
        {
            pixel = (texWidth * (ptsy + 1) + ptsx) * 4;
            if (CompareThreshold(texPixels[pixel + 0], hitColorR)
                && CompareThreshold(texPixels[pixel + 1], hitColorG)
                && CompareThreshold(texPixels[pixel + 2], hitColorB)
                && CompareThreshold(texPixels[pixel + 3], hitColorA))
            {
                fillPointX.Enqueue(ptsx);
                fillPointY.Enqueue(ptsy + 1);
                DrawPoint(pixel);
            }
        }
    }

    // Return the region number based on the clicked location
    return regionNumber; // You can set regionNumber based on where the click occurs, e.g., region 1, 2, 3, etc.
}

private bool IsColorMatch(Color maskColor, Color paintColor)
{
    return Mathf.Abs(maskColor.r - paintColor.r) <= colorTolerance
        && Mathf.Abs(maskColor.g - paintColor.g) <= colorTolerance
        && Mathf.Abs(maskColor.b - paintColor.b) <= colorTolerance
        && Mathf.Abs(maskColor.a - paintColor.a) <= colorTolerance;
}

    private bool CompareThreshold(byte a, byte b)
    {
        return Mathf.Abs(a - b) <= colorTolerance * 255; // Compare with tolerance
    }

    private void DrawPoint(int pixel)
    {
        texPixels[pixel + 0] = (byte)(paintColor.r * 255);
        texPixels[pixel + 1] = (byte)(paintColor.g * 255);
        texPixels[pixel + 2] = (byte)(paintColor.b * 255);
        texPixels[pixel + 3] = (byte)(paintColor.a * 255);
    }

    public void SetPaintColor(int index)
    {
        if (index >= 0 && index < availableColors.Count)
        {
            paintColor = availableColors[index];
        }
    }
}
