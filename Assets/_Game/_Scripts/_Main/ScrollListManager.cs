using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

#if UNITY_WEBGL
using System.IO;
#endif

public class ScrollListManager : MonoBehaviour
{
    public string saveIndexString = "ColoringList";

    [Space]
    public bool horizontalList;

    // List element size
    public float cellSizeX = 243;
    public float cellSizeY = 343;
    public float spacing = -50;

    [Space]
    public bool useButtons;
    public GameObject backwardButton;
    public GameObject forwardButton;

    private List<float> snapPositions;
    private float currentCharCheckTemp;
    private Vector3 newLerpPosition;
    private bool lerping;
    private float lerpingSpeed = 0.1f;
    private float focusedElementScale = 0.5f;
    private float unfocusedElementsScale = 0.5f;
    private List<GameObject> listOfCharacters;
    private bool buttonPressed;
    private int currentCharacter;
    private int firstPos = 0;

    private int texWidth = 640;
    private int texHeight = 814;

    public List<Image> applyTexImg = new();
    private static Dictionary<string, Sprite> allTexturesDic;

    private void Awake()
    {
        if (allTexturesDic == null)
        {
            allTexturesDic = new Dictionary<string, Sprite>();
        }

        firstPos = PlayerPrefs.GetInt(saveIndexString, 0);

        lerping = false;
        buttonPressed = false;

        // Ensure the object persists across scenes
        DontDestroyOnLoad(gameObject);

        LoadAllTexture();
    }

    private void SetNewPos(int num)
    {
        if (horizontalList)
        {
            newLerpPosition = new Vector3(snapPositions[num], 0, 0);
        }
        else
        {
            num = snapPositions.Count - 1 - num;
            newLerpPosition = new Vector3(0, snapPositions[num], 0);
        }

        currentCharacter = num;
        transform.localPosition = newLerpPosition;
        lerping = true;
    }

    private void LoadAllTexture()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
           applyTexImg[i].sprite = LoadImage(saveIndexString + i.ToString(), saveIndexString + i.ToString() == ColoringBookManager.ID);
        }
    }

    private Sprite LoadImage(string key, bool update = false)
    {
        // Check if the texture is already in the dictionary and we don't want to update it
        if (allTexturesDic.ContainsKey(key) && !update)
        {
            return allTexturesDic[key];
        }

        byte[] loadPixels = new byte[texWidth * texHeight * 4];

#if UNITY_WEBGL
        string file = Application.persistentDataPath + "/Portrait" + key + ".sav?nocache=" + DateTime.Now.Ticks;

        if (File.Exists(file))
        {
            string fileContents = File.ReadAllText(file);
            loadPixels = System.Convert.FromBase64String(fileContents);
        }
        else
        {
            return null;
        }
#else
        if (PlayerPrefs.HasKey(key))
        {
            string base64Data = PlayerPrefs.GetString(key);
            loadPixels = System.Convert.FromBase64String(base64Data);
        }
        else
        {
            return null;
        }
#endif

        // Validate data size
        int expectedDataSize = texWidth * texHeight * 4;
        if (loadPixels.Length != expectedDataSize)
        {
            return null;
        }

        // Create Texture2D from the byte array
        Texture2D tex = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
        tex.LoadRawTextureData(loadPixels);
        tex.Apply(false);
        Sprite sp = Sprite.Create(tex, new Rect(0, 0, texWidth, texHeight), Vector2.zero, 100);

        // Store sprite in the dictionary and update it
        if (allTexturesDic.ContainsKey(key))
        {
            allTexturesDic[key] = sp;
        }
        else
        {
            allTexturesDic.Add(key, sp);
        }

        return sp;
    }

    private IEnumerator ButtonPressed()
    {
        yield return new WaitForSeconds(0.4f);
        buttonPressed = false;
    }

    public void LoadGame(int index)
    {
        MusicController.USE.PlaySound(MusicController.USE.clickSound);

        PlayerPrefs.SetInt(saveIndexString, index);
        PlayerPrefs.Save();

        if (transform.GetChild(index).childCount > 0)
        {
            ColoringBookManager.maskTexIndex = index;
        }
        else
        {
            ColoringBookManager.maskTexIndex = -1;
        }

        ColoringBookManager.ID = saveIndexString + index.ToString();
        UIManager.Instance.HidePreviousScreen();
        SceneManager.LoadScene("PaintScene");
    }

    public void LoadGlitterScene(int index)
    {
         MusicController.USE.PlaySound(MusicController.USE.clickSound);

        PlayerPrefs.SetInt(saveIndexString, index);
        PlayerPrefs.Save();

        if (transform.GetChild(index).childCount > 0)
        {
            ColoringBookManager.maskTexIndex = index;
        }
        else
        {
            ColoringBookManager.maskTexIndex = -1;
        }

        ColoringBookManager.ID = saveIndexString + index.ToString();
        UIManager.Instance.HidePreviousScreen();
        SceneManager.LoadScene("ColorByGlitter");
    }

    public void LoadColorBNumScene(int index)
    {
     //    MusicController.USE.PlaySound(MusicController.USE.clickSound);

        PlayerPrefs.SetInt(saveIndexString, index);
        PlayerPrefs.Save();

        // if (transform.GetChild(index).childCount > 0)
        // {
        //     ColorFill.currentIndex = index;
        // }
        // else
        // {
        //     ColorFill.currentIndex = -1;
        // }

    //    ColoringBookManager.ID = saveIndexString + index.ToString();
      //  UIManager.Instance.HidePreviousScreen();
        SceneManager.LoadScene("ColorByNumber");
    }

    private void Start()
    {
        // Ensure textures are loaded when starting the scene
        LoadAllTexture();
    }

    private void OnEnable()
    {
        // Clear texture cache to ensure it reloads after returning to the menu screen
        allTexturesDic.Clear();

        // Subscribe to the scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from the scene loaded event to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reload textures when a new scene is loaded
        LoadAllTexture();
    }
}
