using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using System.Security.Cryptography;

public class ColorFill : MonoBehaviour
{
    public Texture2D baseTex; // Single texture for both base and mask
    public Texture2D maskNumberTex;
    public Texture2D hightLightTex;
    public Color paintColor = Color.white; // The color to fill

    public List<Color> availableColors;
    public List<Button> colorButtons;
    public float colorTolerance = 0.1f; // Tolerance for color matching
    public Material fillMaterial;
    public Image imageComponent;

    private GraphicRaycaster raycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;

    private byte[] texPixels; // To store the pixel data of the base texture

    [SerializeField]
    private int texWidth;
    [SerializeField]
    private int texHeight;

    private Material instanceMaterial;

    private Texture2D duplicateTex, duplicatedMaskTex , duplicateHighlight; // Duplicate texture for painting
   [SerializeField]
    
    



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

        duplicateHighlight = new Texture2D(hightLightTex.width, hightLightTex.height);
        duplicateHighlight.SetPixels(hightLightTex.GetPixels());
        duplicateHighlight.Apply();

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
        instanceMaterial.SetTexture("_Highlight", duplicateHighlight);
        instanceMaterial.SetTexture("_MaskTex", duplicatedMaskTex);


        imageComponent.material = instanceMaterial;

        instanceMaterial.SetFloat("_RevealAndMask", 1f);
        instanceMaterial.SetFloat("_RegionNumber", 0f); // Default region number (No region selected)

        SetPaintColor(0);
    }

    void OnMouseDown()
{
    Vector2 pixelUV = GetMouseUV();

    // Only trigger if valid (pixelUV != Vector2.zero) and the pixel is not already filled with the selected paint color
    if (pixelUV != Vector2.zero)
    {
        // Get the pixel position from the UV coordinates
        int x = Mathf.FloorToInt(pixelUV.x);
        int y = Mathf.FloorToInt(pixelUV.y);

        // Get the color from the base texture at the clicked position
        Color currentColor = duplicateTex.GetPixel(x, y);

        // Check if the pixel is already filled with the selected paint color
        if (IsColorMatch(currentColor, paintColor))
        {
            // If the pixel is already filled with the selected color, return without doing anything
            Debug.Log("Pixel already filled with the selected paint color. No further action.");
            return; // Skip if the pixel is already filled
        }

        // If the pixel is valid and not filled, proceed with the region reveal
        Debug.Log($"Mouse Clicked at: {pixelUV}");
        RevealClickedRegion(pixelUV); // Handle the clicked region
    }
    else
    {
        Debug.Log("Mouse click did not hit a valid texture.");
    }
}

   Vector2 GetMouseUV()
    {
        pointerEventData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        // Raycast to detect if the click is on the duplicated base texture (imageComponent's material texture)
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(pointerEventData.position), Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject == imageComponent.gameObject)
        {
            // Ensure the click is only on the duplicated base texture
            if (hit.collider.gameObject != imageComponent.gameObject)
                return Vector2.zero;

            // Now ensure that the click happens within the bounds of the duplicated base texture (not mask or number texture)
            RectTransform rectTransform = imageComponent.rectTransform;

            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, pointerEventData.position, Camera.main, out localPos);

            Vector2 pivotAdjustedPos = localPos + (rectTransform.rect.size * rectTransform.pivot);

            Vector2 uv = new Vector2(
                pivotAdjustedPos.x / rectTransform.rect.width,
                pivotAdjustedPos.y / rectTransform.rect.height
            );

            uv.x *= duplicateTex.width;  // Use duplicateTex width instead of baseTex
            uv.y *= duplicateTex.height; // Use duplicateTex height instead of baseTex

            uv.x = Mathf.Clamp(uv.x, 0, duplicateTex.width - 1);  // Clamp within duplicateTex
            uv.y = Mathf.Clamp(uv.y, 0, duplicateTex.height - 1); // Clamp within duplicateTex

            return uv;
        }

        return Vector2.zero; // Return zero if click is outside the duplicated base texture area
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
                

                // After filling, update the button's fill amount
                UpdateColorButtonFillAmount(paintColor); // Update the corresponding button's fill amount
            }
            else
            {
                Debug.Log("Mask color does not match the selected paint color.");

                // Find the index of the mask color in the available color list
                
                int colorIndex = FindColorIndex(maskColor);
              

                // Highlight the corresponding color button if a matching color is found
                if (colorIndex != -1)
                {
                    HighlightMatchingColorButton(colorIndex);
                }
                else
                {
                    Debug.LogWarning("Mask color not found in the available color list.");
                }


            }

           
        }




    void HighlightMatchingColorButton(int buttonIndex)
{
    if (buttonIndex >= 0 && buttonIndex < colorButtons.Count)
    {
        // Apply scaling animation for bounce effect
        var button = colorButtons[buttonIndex];
        button.transform.DOScale(Vector3.one * 1.2f, 0.2f)  // Scale up to 1.2x
            .OnComplete(() => button.transform.DOScale(Vector3.one, 0.2f)); // Scale back down to 1 (original scale)
    }
}

    void UpdateTexture()
    {

        //Debug.LogWarning("called");
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

            // Check all four directions and fill accordingly
            CheckDirection(ref fillPointX, ref fillPointY, ptsx, ptsy - 1, hitColorR, hitColorG, hitColorB, hitColorA); // down
            CheckDirection(ref fillPointX, ref fillPointY, ptsx + 1, ptsy, hitColorR, hitColorG, hitColorB, hitColorA); // right
            CheckDirection(ref fillPointX, ref fillPointY, ptsx - 1, ptsy, hitColorR, hitColorG, hitColorB, hitColorA); // left
            CheckDirection(ref fillPointX, ref fillPointY, ptsx, ptsy + 1, hitColorR, hitColorG, hitColorB, hitColorA); // up
        }

        // Return the region number based on the clicked location
        return regionNumber;
    }

    private void CheckDirection(ref Queue<int> fillPointX, ref Queue<int> fillPointY, int x, int y, byte hitColorR, byte hitColorG, byte hitColorB, byte hitColorA)
    {
        if (x >= 0 && x < texWidth && y >= 0 && y < texHeight) // Ensure within bounds
        {
            int pixel = (texWidth * y + x) * 4;
            if (CompareThreshold(texPixels[pixel + 0], hitColorR)
                && CompareThreshold(texPixels[pixel + 1], hitColorG)
                && CompareThreshold(texPixels[pixel + 2], hitColorB)
                && CompareThreshold(texPixels[pixel + 3], hitColorA))
            {
                fillPointX.Enqueue(x);
                fillPointY.Enqueue(y);
                DrawPoint(pixel);
            }
        }
    }

    void UpdateColorButtonFillAmount(Color color)
{
    // Calculate the filled percentage for the color (this will give you a value between 0 and 1)
    float fillAmount = CalculateFillAmountForColor(color);

    if (fillAmount > 0.95f) 
    {
        fillAmount = 1f;
    }

    // Find the corresponding button for the paint color
    int colorIndex = FindColorIndex(color);
    if (colorIndex != -1)
    {
        // Get the button and its child image (assumed to be a "Fill Amount" indicator)
        Button button = colorButtons[colorIndex];
        Image childImage = button.transform.GetChild(2).GetComponent<Image>();

        // Update the fill amount of the button's child image (progress bar)
        childImage.fillAmount = fillAmount;

       // Debug.Log($"Updated fill amount for color: {color} to {fillAmount * 100}%");
    }
}

    float CalculateFillAmountForColor(Color color)
{
    int totalParts = 0;  // Total number of regions (parts) for the selected color
    int filledParts = 0;  // Number of regions that have been filled in the duplicate mask

    // Iterate through the mask texture to count the total regions for the specified color
    for (int i = 0; i < texWidth; i++)
    {
        for (int j = 0; j < texHeight; j++)
        {
            // Get the color from the mask image
            Color maskPixelColor = duplicatedMaskTex.GetPixel(i, j);
            
            // If the pixel color matches the selected paint color in the mask
            if (IsColorMatch(maskPixelColor, color))
            {
                totalParts++;

                // Check if this part has been filled in the duplicate texture
                Color duplicateColor = duplicateTex.GetPixel(i, j);
                if (IsColorMatch(duplicateColor, color))  // Already filled with the same color
                {
                    filledParts++;
                }
            }
        }
    }

    // Calculate the fill amount as the ratio of filled parts to total parts
    if (totalParts == 0)
    {
        return 0f;  // No regions of this color, return 0
    }

    // Normalize the fill amount to be between 0 and 1
    float fillAmount = (float)filledParts / totalParts;
    
    // Ensure that fillAmount is always between 0 and 1
    fillAmount = Mathf.Clamp01(fillAmount);

    return fillAmount;
}

    // Method to calculate the filled area percentage for a color
    float CalculateFillPercentage(Color color)
    {
        int filledPixels = 0;
        int totalPixels = texWidth * texHeight;

        // Iterate through the texture to count the filled pixels for the specified color
        for (int i = 0; i < texWidth; i++)
        {
            for (int j = 0; j < texHeight; j++)
            {
                int pixelIndex = (j * texWidth + i) * 4;
                Color pixelColor = new Color(
                    texPixels[pixelIndex] / 255f,
                    texPixels[pixelIndex + 1] / 255f,
                    texPixels[pixelIndex + 2] / 255f,
                    texPixels[pixelIndex + 3] / 255f
                );

                if (IsColorMatch(pixelColor, color))
                {
                    filledPixels++;
                }
            }
        }

        // Return the percentage of the texture that has been filled with the selected color
        return (float)filledPixels / totalPixels;
    }

    // Helper method to find the index of a color in the available colors list
    int FindColorIndex(Color color)
    {
        for (int i = 0; i < availableColors.Count; i++)
        {
            if (IsColorMatch(color, availableColors[i]))
            {
                return i;
            }
        }
        return -1; // Return -1 if no matching color is found
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
             foreach(Button go in colorButtons)
             {
                go.transform.localScale = Vector3.one;
             }
             colorButtons[index].transform.localScale = new Vector3(1.2f,1.2f,1.2f);
             UpdateHighlightTexture(paintColor);
        }
    }

void UpdateHighlightTexture(Color color)
{
    Debug.LogWarning("called: " + color);

     duplicateHighlight = new Texture2D(hightLightTex.width, hightLightTex.height);
        duplicateHighlight.SetPixels(hightLightTex.GetPixels());
        duplicateHighlight.Apply();

    instanceMaterial.SetTexture("_Highlight", duplicateHighlight);

    // Reset the updatedColors array
    Color[] updatedColors = new Color[texWidth * texHeight];
    for (int k = 0; k < updatedColors.Length; k++)
    {
        updatedColors[k] = Color.clear; // Reset all pixels to transparent
    }

    for (int i = 0; i < texWidth; i++)
    {
        for (int j = 0; j < texHeight; j++)
        {
            int pixelIndex = (j * texWidth + i);

            Color maskColor = duplicatedMaskTex.GetPixel(i, j); 

            if (IsColorMatch(maskColor, color))
            {
                updatedColors[pixelIndex] = duplicateHighlight.GetPixel(i, j);  

                 instanceMaterial.SetFloat("_OutlineThreshold", 0f);
                // Debug.Log("called:-----------> " );
            }
        }
    }

    duplicateHighlight.SetPixels(updatedColors); 
    duplicateHighlight.Apply(); 
    instanceMaterial.SetTexture("_Highlight", duplicateHighlight);
}

// Helper method to compare colors with a tolerance
bool IsColorMatch(Color color1, Color color2, float tolerance = 0.1f)
{
    return Mathf.Abs(color1.r - color2.r) <= tolerance &&
           Mathf.Abs(color1.g - color2.g) <= tolerance &&
           Mathf.Abs(color1.b - color2.b) <= tolerance &&
           Mathf.Abs(color1.a - color2.a) <= tolerance;
}

  
    void ResetAllButtonFillAmounts()
    {
        foreach (Button button in colorButtons)
        {
            Image childImage = button.transform.GetChild(2).GetComponent<Image>();
            childImage.fillAmount = 0f;
        }
    }

     public void OnClearButtonClicked()
    {
        duplicateTex.SetPixels(baseTex.GetPixels());
        duplicateTex.Apply();  
        Color[] baseTexColors = baseTex.GetPixels();
        for (int i = 0; i < baseTexColors.Length; i++)
        {
            texPixels[i * 4 + 0] = (byte)(baseTexColors[i].r * 255);
            texPixels[i * 4 + 1] = (byte)(baseTexColors[i].g * 255);
            texPixels[i * 4 + 2] = (byte)(baseTexColors[i].b * 255);
            texPixels[i * 4 + 3] = (byte)(baseTexColors[i].a * 255);
        }
        duplicatedMaskTex.SetPixels(maskNumberTex.GetPixels()); 
        duplicatedMaskTex.Apply(); 
        instanceMaterial.SetTexture("_MainTex", duplicateTex); 
        instanceMaterial.SetTexture("_MaskTex", duplicatedMaskTex); 
        instanceMaterial.SetFloat("_RegionNumber", 0f);
        instanceMaterial.SetFloat("_RevealAndMask", 1f);
        ResetAllButtonFillAmounts();
    }
}
