using UnityEngine;

public class ButtonClickHandler : MonoBehaviour
{
    public Magnifier magnifierScript; // Reference to the Magnifier script

    // Call this method from the button or any event trigger
    public void OnButtonClick()
    {
        // Get the mouse position or any other position you want to zoom in
        Vector2 mousePosition = Input.mousePosition;

        // Call the OnMagnify method to zoom into the clicked position
        magnifierScript.OnMagnify(mousePosition);
    }
}
