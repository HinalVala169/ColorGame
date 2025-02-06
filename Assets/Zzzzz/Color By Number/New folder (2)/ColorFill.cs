using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;
using UnityEngine.SceneManagement;


public class ColorFill : MonoBehaviour
{
    public Texture2D baseTex; // Single texture for both base and mask
    public Texture2D maskNumberTex;
    public Texture2D hightLightTex;

    public Collider2D boxCollider;
    public Color paintColor = Color.white; // The color to fill

    public List<Color> availableColors;
    [SerializeField]
    private Dictionary<Color, bool> colorFillStatus = new Dictionary<Color, bool>();
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

    private int lastUserSelectedIndex = 0;

    private Texture2D duplicateTex, duplicatedMaskTex , duplicateHighlight; // Duplicate texture for painting
   [SerializeField]
    
    



    void Start()
    {

        boxCollider = GetComponent<BoxCollider2D>();

        if (boxCollider == null)
    {
        boxCollider = gameObject.AddComponent<BoxCollider2D>();
    }
     texWidth = baseTex.width;
        texHeight = baseTex.height;
        texPixels = new byte[texWidth * texHeight * 4];

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

        instanceMaterial = new Material(fillMaterial) { name = fillMaterial.name + "InstanceMaterial_" };
        instanceMaterial.mainTexture = duplicateTex;
        instanceMaterial.SetTexture("_Highlight", duplicateHighlight);
        instanceMaterial.SetTexture("_MaskTex", duplicatedMaskTex);
        imageComponent.material = instanceMaterial;
        instanceMaterial.SetFloat("_RevealAndMask", 1f);
        instanceMaterial.SetFloat("_RegionNumber", 0f);

        foreach (Color color in availableColors)
        {
            colorFillStatus[color] = false;
        }

        SaveLoadManager.LoadProgress("ColorByNumberSave", texPixels, texWidth, texHeight, duplicateTex);
        SetPaintColor(0);

        UpdateColliderSize();
    }

   void UpdateColliderSize()
{
    if (imageComponent == null || duplicateTex == null || boxCollider == null)
        return;

    RectTransform rectTransform = imageComponent.rectTransform;
    
    // Ensure boxCollider is a BoxCollider2D
    BoxCollider2D box = boxCollider as BoxCollider2D;
    if (box != null)
    {
        // Set size using RectTransform's width and height (LOCAL UI SPACE)
        box.size = rectTransform.rect.size;
        Debug.Log("Collider Size Updated: " + box.size);
    }
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
       // Debug.Log($"Mouse Clicked at: {pixelUV}");
        RevealClickedRegion(pixelUV); // Handle the clicked region
    }
    else
    {
        Debug.Log("Mouse click did not hit a valid texture.");
    }
}

            Vector2 GetMouseUV()
{
    // Ensure eventSystem and raycaster are properly initialized
    if (eventSystem == null)
    {
        eventSystem = FindObjectOfType<EventSystem>(); // Automatically get the EventSystem if not assigned
    }

    if (raycaster == null)
    {
        raycaster = FindObjectOfType<GraphicRaycaster>(); // Automatically get the GraphicRaycaster if not assigned
    }

    if (imageComponent == null)
    {
        imageComponent = GetComponent<Image>(); // Automatically get the Image component if not assigned
    }

    pointerEventData = new PointerEventData(eventSystem)
    {
        position = Input.mousePosition
    };

    // Perform a UI Raycast to check if the click is on any UI elements (header, footer, etc.)
    List<RaycastResult> results = new List<RaycastResult>();
    raycaster.Raycast(pointerEventData, results);

    // Check if the click is on a UI element in the "UI" layer (header/footer)
    foreach (RaycastResult result in results)
    {
        if (result.gameObject.layer == LayerMask.NameToLayer("UI"))
        {
            // If the click is on a UI element (header/footer), return Vector2.zero (ignore background click)
            return Vector2.zero;
        }
    }

    // Perform a raycast on the background (only if it's not blocked by header/footer)
    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(pointerEventData.position), Vector2.zero);
    if (hit.collider != null && hit.collider.gameObject == imageComponent.gameObject)
    {
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

    return Vector2.zero; // Return zero if click is outside the paint area or on another layer
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
                Debug.Log("paintColor----" + paintColor);
                // if(IsColorFullyFilled(paintColor))
                // {
                    
                //     Debug.Log("SuggestNextPendingColor----");
                // }

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


    void SuggestNextPendingColor()
    {
        if (lastUserSelectedIndex != -1) return; // User manually selected a color, so skip auto selection

        int nextPendingIndex = availableColors.FindIndex(c => !colorFillStatus[c]);
        if (nextPendingIndex != -1)
        {
          
            SetPaintColor(nextPendingIndex);
        }
        else
        {
            // If no pending colors, scale all buttons to 1
            foreach (Button colorButton in colorButtons)
            {
                colorButton.transform.localScale = Vector3.one;
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
    // Calculate the filled percentage for the color (this will give a value between 0 and 1)
    float fillAmount = CalculateFillAmountForColor(color);

    // Find the corresponding button for the paint color
    int colorIndex = FindColorIndex(color);
    if (colorIndex != -1)
    {
        // Get the button and its child image (assumed to be a "Fill Amount" indicator)
        Button button = colorButtons[colorIndex];
        Image childImage = button.transform.GetChild(2).GetComponent<Image>();

        if(fillAmount > 0.95f)
        {
            fillAmount = 1f;
        }
        // Update the fill amount of the button's child image (progress bar)
        childImage.fillAmount = fillAmount;

        // Print the current color index
        Debug.Log($"Current Color Index: {colorIndex}");

        // If the color is completely filled, mark it and suggest the next pending color
        if (fillAmount >= 1f) 
        {
            colorFillStatus[color] = true; // Mark the color as completely filled
            Debug.Log($"Color {color} (Index {colorIndex}) is fully filled.");
            SuggestNextPendingColor(); // Only suggest the next color when the current one is fully filled
        }

        Debug.Log($"Updated fill amount for color (Index {colorIndex}): {color} to {fillAmount * 100}%. Completed: {colorFillStatus[color]}");
    }
    else
    {
        Debug.LogWarning("Color not found in availableColors list.");
    }
}

public bool IsColorFullyFilled(Color color)
{
    return colorFillStatus.ContainsKey(color) && colorFillStatus[color];
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
              lastUserSelectedIndex = -1; 
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
        foreach (Color color in availableColors)
        {
            colorFillStatus[color] = false;
        }
        SetPaintColor(0);
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

      public void OnHomeButtonClicked()
        {
           // SaveImage(ID);
            
            // Start the coroutine to delay scene loading by 1 minute
            StartCoroutine(DelayLoadMainScene());
        }

        private IEnumerator DelayLoadMainScene()
        {
            // Wait for 1 minute (60 seconds)
            yield return new WaitForSeconds(0f);

            // Now load the scene after the delay
            SceneManager.LoadScene("MainScene");

            // Use the sceneLoaded event to wait for the scene to load completely
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainScene")
        {
            UIManager.Instance.ReturnToPreviousScreen();
        }
    }
     void OnApplicationQuit()
    {
       // SaveLoadManager.SaveProgress("ColorByNumberSave", texPixels, texWidth, texHeight);
    }
}
