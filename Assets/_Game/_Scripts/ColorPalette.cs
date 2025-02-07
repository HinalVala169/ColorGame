using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewColorPalette", menuName = "ColorByNumber/ColorPalette")]
public class ColorPalette : ScriptableObject
{

    public Texture2D baseTexture; 
    public Texture2D maskNumTexture;
    public Texture2D maskColTexture; 
    public List<Color> colors;
     public List<Sprite> colNum;
}
