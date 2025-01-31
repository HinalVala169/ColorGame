using UnityEngine;
using UnityEngine.UI;

public class DeviceDetectorController : MonoBehaviour
{
    [SerializeField]
    private CanvasScaler canvasScaler;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         if (IsTablet())
        {
            Debug.Log("Running on a Tablet");
            canvasScaler.matchWidthOrHeight = 0.0f; // Tablet setting
        }
        else
        {
            if (IsSamsungGalaxyZFold())
        {
            Debug.Log("Running on Samsung Galaxy Z Fold");
            canvasScaler.matchWidthOrHeight = 0.6f; // Set to 0.6 for Z Fold
        }
     
            // If it's a normal phone
            Debug.Log("Running on a Phone");
            canvasScaler.matchWidthOrHeight = 0.6f; // Default for phones
        }
    }

    // Function to determine if the device is a tablet based on screen size
    bool IsTablet()
    {
        float screenWidthInches = Screen.width / Screen.dpi;
        float screenHeightInches = Screen.height / Screen.dpi;
        float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidthInches, 2) + Mathf.Pow(screenHeightInches, 2));

        Debug.Log("Tablet Diagonal Inches : " + diagonalInches);
      
        return diagonalInches >= 6.5f; // Tablet should have a diagonal size >= 6.5 inches
    }

    // Function to determine if the device is a Samsung Galaxy Z Fold
    bool IsSamsungGalaxyZFold()
    {
        string deviceModel = SystemInfo.deviceModel.ToLower();
        Debug.Log("Device Model: " + deviceModel); // Log the actual device model

        // Check if the device model contains "samsung" and "galaxy z fold"
        if (deviceModel.Contains("samsung") && deviceModel.Contains("galaxy z fold"))
        {
            return true;  // Samsung Galaxy Z Fold detected
        }

        // Fallback to screen size check if model isn't recognized
        float screenWidthInches = Screen.width / Screen.dpi;
        float screenHeightInches = Screen.height / Screen.dpi;
        float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidthInches, 2) + Mathf.Pow(screenHeightInches, 2));

        Debug.Log("Device Screen Diagonal Inches: " + diagonalInches);

        // Adjust the range for Z Fold - Based on typical screen size (around 7.6 inches unfolded)
        return diagonalInches >= 7.0f && diagonalInches <= 8.0f;  // Adjust for the Z Fold range
    }
}
