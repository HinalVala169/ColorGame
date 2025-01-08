using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private BaseUICanvas[] canvases; // Manually assign canvases 
    
    [SerializeField] 
    private  CanvasType previousCanvasType = CanvasType.None; // To track the last active canvas

    private void Awake()
    {
        // Ensure that there is only one instance of UIManager in each scene
       if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Prevent UIManager from being destroyed on scene load
        }
        else
        {
            Destroy(gameObject); // If another instance exists, destroy this one
        }
        // Hide all canvases initially
        foreach (var canvas in canvases)
        {
            canvas.Hide();
        }

        // Show the last active canvas when returning to the UI
        if (previousCanvasType != CanvasType.None)
        {
            ShowCanvas(previousCanvasType);
        }
    }

    private void Start()
    {
        ShowMainMenu(); // Optionally show the main menu on startup
    }

    public void ShowCanvas(CanvasType canvasType)
    {
        bool canvasFound = false;

        // Hide all canvases first
        foreach (var canvas in canvases)
        {
            canvas.Hide();
        }

        // Show the specified canvas
        foreach (var canvas in canvases)
        {
            if (canvas.CanvasType == canvasType)
            {
                canvas.Show();
                previousCanvasType = canvasType; // Update the last active canvas
                canvasFound = true;
                break;
            }
        }

        if (!canvasFound)
        {
            Debug.LogWarning($"Canvas of type {canvasType} not found.");
        }
    }

    // Wrapper methods to call from UI buttons
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

    // Method to load a new scene
    public void LoadScene(string sceneName)
    {
        // Store the current UI state before loading
        previousCanvasType = CanvasType.SubMenuScreen; // Adjust as per the current active canvas

        // Load the new scene
        SceneManager.LoadScene(sceneName);
    }

    public void HidePreviousScreen()
    {
        foreach (var canvas in canvases)
        {
            canvas.Hide();
        }
    }

    // Method to be called on returning to this scene
    public void ReturnToPreviousScreen()
    {
        ShowCanvas(previousCanvasType); // Show the previous canvas when coming back
    }
}

public enum CanvasType
{
    None,
    MainSCR,
    LevelSCR,
    SubMenuScreen
}
