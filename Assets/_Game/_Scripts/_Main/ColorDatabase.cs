using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewColorDatabase", menuName = "ColorByNumber/ColorDatabase")]
public class ColorDatabase : ScriptableObject
{
    public List<ColorPalette> levelColorPalettes;

    public ColorPalette GetColorsForLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelColorPalettes.Count)
        {
            return levelColorPalettes[levelIndex];
        }
        return null;
    }
}
