using UnityEngine;
using UnityEngine.UI;

public class ColorSelector : MonoBehaviour
{
    public ColorFill_ colorFillScript;  // Reference to the ColorFill script
    public Button[] colorButtons;     // Array of buttons for color selection
    private Color selectedColor;      // The selected color

    void Start()
    {
        // Set default color (first color in array)
        selectedColor = colorButtons[0].GetComponent<Image>().color;

        // Add listeners to all color buttons
        foreach (Button button in colorButtons)
        {
            button.onClick.AddListener(() => OnColorButtonClick(button));
        }
    }

    // Called when a color button is clicked
    void OnColorButtonClick(Button button)
    {
        selectedColor = button.GetComponent<Image>().color;
        colorFillScript.SetSelectedColor(selectedColor); // Update the selected color in ColorFill script
    }
}
