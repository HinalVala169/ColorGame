using UnityEngine;
using System;
using System.IO;

public static class SaveLoadManager
{
    public static void SaveProgress(string key, byte[] pixels, int texWidth, int texHeight)
    {
        if (pixels == null || pixels.Length != texWidth * texHeight * 4)
        {
            Debug.LogError("Invalid pixel data.");
            return;
        }

#if UNITY_WEBGL
        string file = Application.persistentDataPath + "/Portrait" + key + ".sav";
        string fileData = Convert.ToBase64String(pixels);
        File.WriteAllText(file, fileData);
#else
        PlayerPrefs.SetString(key, Convert.ToBase64String(pixels));
        PlayerPrefs.Save();
#endif

        Debug.Log("-----> Save ID : " + key);
    }

    public static void LoadProgress(string key, byte[] pixels, int texWidth, int texHeight, Texture2D duplicateTex)
    {
        if (PlayerPrefs.HasKey(key))
        {
            string data = PlayerPrefs.GetString(key);
            pixels = Convert.FromBase64String(data);

            Color[] updatedColors = new Color[texWidth * texHeight];
            for (int i = 0; i < texWidth; i++)
            {
                for (int j = 0; j < texHeight; j++)
                {
                    int pixelIndex = (j * texWidth + i) * 4;
                    updatedColors[j * texWidth + i] = new Color(
                        pixels[pixelIndex] / 255f,
                        pixels[pixelIndex + 1] / 255f,
                        pixels[pixelIndex + 2] / 255f,
                        pixels[pixelIndex + 3] / 255f
                    );
                }
            }

            duplicateTex.SetPixels(updatedColors);
            duplicateTex.Apply();
            Debug.Log("Loaded progress from key: " + key);
        }
        else
        {
            Debug.Log("No saved progress found for key: " + key);
        }
    }
}
