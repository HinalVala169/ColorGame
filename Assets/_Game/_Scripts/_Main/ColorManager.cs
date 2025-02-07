using UnityEngine;
using UnityEngine.UI;  // Required for UI Button

public class ColorManager : MonoBehaviour
{

    public static ColorManager Instance;
    public ColorDatabase colorDatabase; 
    public GameObject buttonPrefab;   
    public Transform containerParent; 
    private int currentLevel = 0;       
    public ColorFill colorFill;

    void Awake()
    {
        Instance = this ;
    }
    void Start()
    {
        LoadLevelColors(ColorFill.currentIndex);
    }

    void LoadLevelColors(int levelIndex)
    {
        ColorPalette palette = colorDatabase.GetColorsForLevel(levelIndex);
        if (palette != null)
        {
            foreach (Transform child in containerParent)
            {
                Destroy(child.gameObject);
            }

            // Instantiate buttons for each color in the palette
            for (int i = 0; i < palette.colors.Count; i++)
            {
                InstantiateColorButton(palette.colors[i], palette.colNum[i],i);
            }
        }
        else
        {
            Debug.LogWarning("No color palette found for this level!");
        }
    }

  

   void InstantiateColorButton(Color color, Sprite colNumSprite, int no)
{
    if (colorFill == null)
    {
        Debug.LogError("colorFill is not assigned!");
        return;
    }
    GameObject buttonObject = Instantiate(buttonPrefab, containerParent);
    Button button = buttonObject.GetComponent<Button>();
    button.transform.GetChild(0).GetComponent<Image>().color = color;
    button.transform.GetChild(1).GetComponent<Image>().sprite = colNumSprite;

    colorFill.colorButtons.Add(button);

    button.name = "colorBTN" + " " + (no + 1);
    ButtonColorHelper helper = buttonObject.GetComponent<ButtonColorHelper>();
    
    helper.colorIndex = no;  
    button.onClick.AddListener(helper.OnButtonClicked);

   // Debug.Log("Button instantiated for color: " + color);
    colorFill.availableColors.Add(color);
}
    void OnColorButtonClick(Color selectedColor)
    {
       // Debug.Log("Selected Color: " + selectedColor);
    }
}
