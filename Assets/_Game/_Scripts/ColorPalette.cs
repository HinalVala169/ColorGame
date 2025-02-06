using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewColorPalette", menuName = "ColorByNumber/ColorPalette")]
public class ColorPalette : ScriptableObject
{
    public List<Color> colors;
     public List<Sprite> colNum;
}
