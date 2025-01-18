using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ColorFill : MonoBehaviour
{
    public Texture2D maskTex; // Mask texture
    public float colorTolerance = 0.1f;
    public Material fillMaterial;
    public Image imageComponent;

    private GraphicRaycaster raycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;

    void Start()
    {
        // Set up the mask texture and initial material
        imageComponent.material.mainTexture = maskTex;
        Debug.Log("Start: Image material set to mask texture.");

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

        // Use Raycast to check for the Image component
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(pointerEventData.position), Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject == imageComponent.gameObject)
        {
            // Map mouse position to the texture's UV coordinates
            RectTransform rectTransform = imageComponent.rectTransform;
            Vector2 localPos = pointerEventData.position - (Vector2)rectTransform.position;
            Vector2 uv = new Vector2(localPos.x / rectTransform.rect.width, localPos.y / rectTransform.rect.height);

            // Map the UV coordinates to the texture coordinates
            uv.x *= maskTex.width;
            uv.y *= maskTex.height;

            // Ensure UV coordinates are within bounds
            uv.x = Mathf.Clamp(uv.x, 0, maskTex.width - 1);
            uv.y = Mathf.Clamp(uv.y, 0, maskTex.height - 1);

            Debug.Log($"Mouse UV: {uv}");
            return uv;
        }
        return Vector2.zero; // Return invalid if no hit
    }

    void RevealClickedRegion(Vector2 clickedUV)
{
    // Calculate texture coordinates from UV (same as before)
    Vector2 uv = clickedUV;

    // Here, you can update the texture or apply the changes using a shader (no direct SetPixel)
    // If using a fill material with a shader, you could pass the click position to the shader to handle the reveal effect.

    // For example:
    fillMaterial.SetVector("_ClickPosition", uv);
    fillMaterial.SetFloat("_Radius", 10f); // Adjust the reveal radius

    // Apply the material to the image (triggering the shader logic)
    imageComponent.material = fillMaterial;

    Debug.Log("Triggering region reveal with shader.");
}
}
