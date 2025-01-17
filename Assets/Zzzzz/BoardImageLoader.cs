using UnityEngine;

public class BoardImageLoader : MonoBehaviour
{
    public SpriteRenderer boardSpriteRenderer;  // The SpriteRenderer component for displaying the board
    public Sprite paintingSprite;  // The sprite to be applied to the board

    // Start is called before the first frame update
    void Start()
    {
        // Apply the sprite to the board when the game starts
        ApplySpriteToBoard();
    }

    // Method to apply the sprite assigned in the Inspector to the board
    public void ApplySpriteToBoard()
    {
        if (paintingSprite != null)
        {
            // Apply the sprite to the board's SpriteRenderer component
            boardSpriteRenderer.sprite = paintingSprite;
        }
        else
        {
            Debug.LogError("No sprite assigned in the Inspector.");
        }
    }
}
