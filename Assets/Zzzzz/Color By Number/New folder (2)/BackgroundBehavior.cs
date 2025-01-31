using UnityEngine;
using UnityEngine.UI;

public class BackgroundBehavior : MonoBehaviour
{
    [SerializeField] private Image image; // Reference to the Image component with a material attached

    public void Awake()
    {
        var camera = Camera.main;

        // Set the background's position to align with the camera
        transform.position = camera.transform.position + camera.transform.forward * (camera.farClipPlane - 0.01f);
        transform.forward = camera.transform.forward;

        // Get the canvas's RectTransform (the canvas size is what we want to match)
        RectTransform canvasRectTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRectTransform.sizeDelta;

        // Get the sprite's texture size and calculate the aspect ratio
        var spriteSize = image.sprite.textureRect.size;
        var spriteAspect = spriteSize.x / spriteSize.y;

        // Get the camera's height and width based on the orthographic size and aspect ratio
        var cameraHeight = camera.orthographicSize * 2;
        var cameraWidth = cameraHeight * camera.aspect;

        // Adjust the background's size based on the camera's aspect ratio, without stretching the image
        if (camera.aspect > spriteAspect)
        {
            // If the camera's aspect ratio is greater, fit by height (image will scale horizontally)
            image.rectTransform.sizeDelta = new Vector2(canvasSize.y * spriteAspect, canvasSize.y);
        }
        else
        {
            // If the sprite's aspect ratio is greater, fit by width (image will scale vertically)
            image.rectTransform.sizeDelta = new Vector2(canvasSize.x, canvasSize.x / spriteAspect);
        }

        // Adjust the position to make sure it is centered in the canvas
        image.rectTransform.anchoredPosition = Vector2.zero;

        // If the image has a material, make sure it gets applied correctly without stretching
        ApplyMaterialToImage();
    }

    // Apply the material to the image, ensuring it's not distorted or stretched
    private void ApplyMaterialToImage()
    {
        // Assuming the material is attached to the image, this will ensure it is applied
        if (image.material != null)
        {
            // Ensure the material is being applied correctly
            image.material.SetFloat("_SomeShaderProperty", 1.0f); // Example of setting shader properties, adjust as needed
        }
    }
}
