using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private BaseUICanvas[] canvases; 
    
    [SerializeField] 
    private  CanvasType previousCanvasType = CanvasType.None;

    [SerializeField] 

    int currentCanvasIndex = 0;

    private void Awake()
    {
     
       if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); 
        }
        foreach (var canvas in canvases)
        {
            canvas.Hide();
        }
        if (previousCanvasType != CanvasType.None)
        {
            ShowCanvas(previousCanvasType);
        }
    }

    private void Start()
    {
        ShowMainMenu(); 
    }

    public void ShowCanvas(CanvasType canvasType)
    {
        bool canvasFound = false;

        // Hide all canvases first
        foreach (var canvas in canvases)
        {
            canvas.Hide();
        }

             for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i].CanvasType == canvasType)
            {
                canvases[i].Show();
                previousCanvasType = canvasType;
                currentCanvasIndex = i; // Update the current canvas index
                canvasFound = true;
                break;
            }
        }

        if (!canvasFound)
        {
            Debug.LogWarning($"Canvas of type {canvasType} not found.");
        }

       
    }

    public void GoBackToPreviousCanvas()
    {
        if (currentCanvasIndex > 0)
        {
            currentCanvasIndex--; // Decrement the index to move back
            ShowCanvas(canvases[currentCanvasIndex].CanvasType);
            Debug.Log("Going back to canvas index: " + currentCanvasIndex);
        }
        else
        {
            Debug.LogWarning("No previous canvas to go back to.");
        }
    }

    public void ShowMainMenu()
    {
        ShowCanvas(CanvasType.MainSCR);
    }

    public void ShowLevelScreen()
    {
        ShowCanvas(CanvasType.LevelSCR);
    }

    public void ShowSubMenuScreen()
    {
        ShowCanvas(CanvasType.SubMenuScreen);
    }

     public void ShowColorByNumSUBMenuSCR()
    {
        ShowCanvas(CanvasType.ColorByNumSUBMenuSCR);
    }
    public void LoadScene(string sceneName)
    {
        previousCanvasType = CanvasType.SubMenuScreen;
        SceneManager.LoadScene(sceneName);
    }

    public void HidePreviousScreen()
    {
        foreach (var canvas in canvases)
        {
            canvas.Hide();
        }
    }
    public void ReturnToPreviousScreen()
    {
        ShowCanvas(previousCanvasType);
    }
}

public enum CanvasType
{
    None,
    MainSCR,
    LevelSCR,
    SubMenuScreen,
    ColorByNumSUBMenuSCR
}
