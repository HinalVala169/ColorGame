using UnityEngine;
using UnityEngine.UI;

public class ButtonColorHelper : MonoBehaviour
{
//    public ColorFill colorFill; // Reference to the ColorFill script
    public int colorIndex;      // Store the index for each button

    public void OnButtonClicked()
    {
       if (ColorManager.Instance.colorFill != null)
    {
        ColorManager.Instance.colorFill.SetPaintColor(colorIndex);
    }
    else
    {
        Debug.LogError("ColorFill reference is not assigned!");
    }
    }
}
