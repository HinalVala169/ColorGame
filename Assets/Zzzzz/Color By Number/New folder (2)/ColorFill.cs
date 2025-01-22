using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ColorFill : MonoBehaviour
{
    public Texture2D baseTex; 
    public Texture2D maskTex; // Mask texture
    public Color paintColor = Color.white; // The color to fill

    public List<Color> availableColors; 
    public float colorTolerance = 0.1f; // Tolerance for color matching
    public Material fillMaterial;
    public Image imageComponent;

    private GraphicRaycaster raycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;

    private byte[] maskPixels; // To store the pixel data of the mask texture
    private byte[] lockMaskPixels; // Used for the flood-fill algorithm (prevents re-filling)

    private int texWidth;
    private int texHeight;

    private Material instanceMaterial;


   private Texture2D duplicatedBaseTex ; // Duplicated base texture
    private Texture2D duplicatedMaskTex; 

    void Start()
{
    // Duplicate the base texture
    duplicatedBaseTex = new Texture2D(baseTex.width, baseTex.height, baseTex.format, baseTex.mipmapCount > 1);
    duplicatedBaseTex.SetPixels(baseTex.GetPixels());
    duplicatedBaseTex.Apply();

    // Duplicate the mask texture
    duplicatedMaskTex = new Texture2D(maskTex.width, maskTex.height, maskTex.format, maskTex.mipmapCount > 1);
    duplicatedMaskTex.SetPixels(maskTex.GetPixels());
    duplicatedMaskTex.Apply();

    // Create a new instance of the material
    instanceMaterial = new Material(fillMaterial) { name = fillMaterial.name + "InstanceMaterial_" };
    instanceMaterial.mainTexture = duplicatedBaseTex;
    instanceMaterial.SetTexture("_MaskTex", duplicatedMaskTex);
    imageComponent.material = instanceMaterial;

    // Initialize the mask texture to be transparent only once
    if (duplicatedMaskTex.GetPixels()[0].a == 0)
    {
        Color[] transparentColors = new Color[duplicatedMaskTex.width * duplicatedMaskTex.height];
        for (int i = 0; i < transparentColors.Length; i++)
        {
            transparentColors[i] = new Color(0, 0, 0, 0);
        }
        duplicatedMaskTex.SetPixels(transparentColors);
        duplicatedMaskTex.Apply();
    }

    // Set up EventSystem and GraphicRaycaster
    raycaster = GetComponentInParent<GraphicRaycaster>();
    eventSystem = EventSystem.current;

    if (raycaster == null || eventSystem == null)
    {
        Debug.LogError("Raycaster or EventSystem not found!");
        return;
    }

    // Initialize mask pixel data
    texWidth = duplicatedMaskTex.width;
    texHeight = duplicatedMaskTex.height;
    maskPixels = new byte[texWidth * texHeight * 4];
    lockMaskPixels = new byte[texWidth * texHeight * 4];

    // Copy pixel data from the duplicated mask texture into maskPixels
    Color[] colors = duplicatedMaskTex.GetPixels();
    for (int i = 0; i < colors.Length; i++)
    {
        maskPixels[i * 4 + 0] = (byte)(colors[i].r * 255);
        maskPixels[i * 4 + 1] = (byte)(colors[i].g * 255);
        maskPixels[i * 4 + 2] = (byte)(colors[i].b * 255);
        maskPixels[i * 4 + 3] = (byte)(colors[i].a * 255);
    }

    instanceMaterial.SetFloat("_Reveal", 1f);
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

        uv.x *= maskTex.width;
        uv.y *= maskTex.height;

        uv.x = Mathf.Clamp(uv.x, 0, maskTex.width - 1);
        uv.y = Mathf.Clamp(uv.y, 0, maskTex.height - 1);

        return uv;
    }
    return Vector2.zero;
}


    void RevealClickedRegion(Vector2 clickedUV)
{
    // Get the pixel position from the UV coordinates
    int x = Mathf.FloorToInt(clickedUV.x);
    int y = Mathf.FloorToInt(clickedUV.y);

    // Flood-fill the region based on the mask texture color at the clicked position
    FloodFillMaskOnlyWithThreshold(x, y);

    // Update the mask texture after filling
    UpdateMaskTexture();
    // Optional: Refresh the texture on the material if needed
    // imageComponent.material.mainTexture = duplicatedMaskTex;

    Debug.Log("Triggering region reveal with flood fill.");

    // Set the "_RevealAndMask" value to control the effect
    // If you want the reveal to happen immediately when clicked, you might set this to a low value like 1
    instanceMaterial.SetFloat("_RevealAndMask", 1f);  // Adjust this based on your desired effect
}


    void UpdateMaskTexture()
    {
        Color[] updatedColors = new Color[duplicatedMaskTex.width * duplicatedMaskTex.height];
        for (int i = 0; i < texWidth; i++)
        {
            for (int j = 0; j < texHeight; j++)
            {
                int pixelIndex = (j * texWidth + i) * 4;
                updatedColors[j * texWidth + i] = new Color(
                    maskPixels[pixelIndex] / 255f,
                    maskPixels[pixelIndex + 1] / 255f,
                    maskPixels[pixelIndex + 2] / 255f,
                    maskPixels[pixelIndex + 3] / 255f
                );
            }
        }

       duplicatedMaskTex.SetPixels(updatedColors);
        duplicatedMaskTex.Apply(); // Apply changes to the texture
    }

    private void FloodFillMaskOnlyWithThreshold(int x, int y)
    {
        // Get the initial color at the clicked position
        byte hitColorR = maskPixels[((texWidth * y) + x) * 4 + 0];
        byte hitColorG = maskPixels[((texWidth * y) + x) * 4 + 1];
        byte hitColorB = maskPixels[((texWidth * y) + x) * 4 + 2];
        byte hitColorA = maskPixels[((texWidth * y) + x) * 4 + 3];

        if (paintColor.r * 255 == hitColorR && paintColor.g * 255 == hitColorG && paintColor.b * 255 == hitColorB && paintColor.a * 255 == hitColorA)
            return; // Skip if the color is already the same

        Queue<int> fillPointX = new Queue<int>();
        Queue<int> fillPointY = new Queue<int>();
        fillPointX.Enqueue(x);
        fillPointY.Enqueue(y);

        int ptsx, ptsy;
        int pixel = 0;

        // Lock array to prevent re-filling
        while (fillPointX.Count > 0)
        {
            ptsx = fillPointX.Dequeue();
            ptsy = fillPointY.Dequeue();

            if (ptsy - 1 >= 0) // down
            {
                pixel = (texWidth * (ptsy - 1) + ptsx) * 4;
                if (lockMaskPixels[pixel] == 0 && CompareThreshold(maskPixels[pixel + 0], hitColorR)
                    && CompareThreshold(maskPixels[pixel + 1], hitColorG)
                    && CompareThreshold(maskPixels[pixel + 2], hitColorB)
                    && CompareThreshold(maskPixels[pixel + 3], hitColorA))
                {
                    fillPointX.Enqueue(ptsx);
                    fillPointY.Enqueue(ptsy - 1);
                    DrawPoint(pixel);
                    lockMaskPixels[pixel] = 1;
                }
            }

            if (ptsx + 1 < texWidth) // right
            {
                pixel = (texWidth * ptsy + ptsx + 1) * 4;
                if (lockMaskPixels[pixel] == 0 && CompareThreshold(maskPixels[pixel + 0], hitColorR)
                    && CompareThreshold(maskPixels[pixel + 1], hitColorG)
                    && CompareThreshold(maskPixels[pixel + 2], hitColorB)
                    && CompareThreshold(maskPixels[pixel + 3], hitColorA))
                {
                    fillPointX.Enqueue(ptsx + 1);
                    fillPointY.Enqueue(ptsy);
                    DrawPoint(pixel);
                    lockMaskPixels[pixel] = 1;
                }
            }

            if (ptsx - 1 >= 0) // left
            {
                pixel = (texWidth * ptsy + ptsx - 1) * 4;
                if (lockMaskPixels[pixel] == 0 && CompareThreshold(maskPixels[pixel + 0], hitColorR)
                    && CompareThreshold(maskPixels[pixel + 1], hitColorG)
                    && CompareThreshold(maskPixels[pixel + 2], hitColorB)
                    && CompareThreshold(maskPixels[pixel + 3], hitColorA))
                {
                    fillPointX.Enqueue(ptsx - 1);
                    fillPointY.Enqueue(ptsy);
                    DrawPoint(pixel);
                    lockMaskPixels[pixel] = 1;
                }
            }

            if (ptsy + 1 < texHeight) // up
            {
                pixel = (texWidth * (ptsy + 1) + ptsx) * 4;
                if (lockMaskPixels[pixel] == 0 && CompareThreshold(maskPixels[pixel + 0], hitColorR)
                    && CompareThreshold(maskPixels[pixel + 1], hitColorG)
                    && CompareThreshold(maskPixels[pixel + 2], hitColorB)
                    && CompareThreshold(maskPixels[pixel + 3], hitColorA))
                {
                    fillPointX.Enqueue(ptsx);
                    fillPointY.Enqueue(ptsy + 1);
                    DrawPoint(pixel);
                    lockMaskPixels[pixel] = 1;
                }
            }
        }
    }

    private bool CompareThreshold(byte a, byte b)
    {
        return Mathf.Abs(a - b) <= colorTolerance * 255; // Compare with tolerance
    }

    private void DrawPoint(int pixel)
    {
        maskPixels[pixel + 0] = (byte)(paintColor.r * 255);
        maskPixels[pixel + 1] = (byte)(paintColor.g * 255);
        maskPixels[pixel + 2] = (byte)(paintColor.b * 255);
        maskPixels[pixel + 3] = (byte)(paintColor.a * 255);
    }
      public void SetPaintColor(int index)
    {
        if (index >= 0 && index < availableColors.Count)
        {
            paintColor = availableColors[index];
        }
    }
}
