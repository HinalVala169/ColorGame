using System.Runtime.Serialization.Formatters;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private CanvasScaler canvasScaler;

    void Start()
    {
        string deviceName = SystemInfo.deviceModel;
        Debug.Log("Detected Device: " + deviceName);

        if (ShouldForceTabletMode(out float matchValue))
        {
            Debug.Log($"✅ Forced Mode: matchWidthOrHeight = {matchValue}");
            canvasScaler.matchWidthOrHeight = matchValue; // Apply the correct value
        }
        else if (IsApplePhone())
        {
            Debug.Log("📱 Detected an iPhone.");
            canvasScaler.matchWidthOrHeight = 0.0f;
        }
        else if (IsAppleTablet())
        {
            Debug.Log("📲 Detected an iPad.");
            canvasScaler.matchWidthOrHeight = 1.0f;
        }
        else if (IsValidNonAppleTablet(out float tabletMatchValue))
        {
            Debug.Log($"📟 Detected a Tablet. Applying matchWidthOrHeight = {tabletMatchValue}");
            canvasScaler.matchWidthOrHeight = tabletMatchValue;
        }
        else
        {
            Debug.Log("📞 Detected a Phone.");
            canvasScaler.matchWidthOrHeight = 0.0f;
        }
    }

    bool ShouldForceTabletMode(out float matchValue)
    {
        float dpi = Screen.dpi;
        float screenWidthInches = Screen.width / dpi;
        float screenHeightInches = Screen.height / dpi;
        float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidthInches, 2) + Mathf.Pow(screenHeightInches, 2));

        Debug.Log($"🖥️ DPI: {dpi}, Width: {screenWidthInches:F2} inches, Height: {screenHeightInches:F2} inches, Diagonal: {diagonalInches:F2} inches");

        // Default to phone mode
        matchValue = 0.0f;

        // ✅ If width is around 5 inches → Set Tablet Mode
        if (screenWidthInches >= 4.8f && screenWidthInches <= 5.9f)
        {
            matchValue = 1.0f;
            return true;
        }

        // ✅ If width is around 3 inches → Set Phone Mode (Including 3.94 inches)
        if (screenWidthInches >= 2.8f && screenWidthInches <=  3.65f)
        {
            matchValue = 0.0f;
            return true;
        }

          if (screenWidthInches >= 3.75f && screenWidthInches <= 3.9f)
        {
            matchValue = 1.0f;
            return true;
        }

        if (screenWidthInches >= 3.75f && screenWidthInches <= 3.9f)
        {
            matchValue = 1.0f;
            return true;
        }

        // ✅ Special Cases: Devices that should NOT be considered tablets
        if (Mathf.Approximately(dpi, 530) && Mathf.Approximately(screenWidthInches, 2.26f) ||
            Mathf.Approximately(dpi, 260) && Mathf.Approximately(screenWidthInches, 4.62f))
        {
            matchValue = 0.0f;  // Treat them as phones
            return true;
        }

        // ✅ If width is 6.14 inches & diagonal is 10.23 inches → Set Tablet Mode
        if (Mathf.Approximately(dpi, 264) &&
            Mathf.Approximately(screenWidthInches, 6.14f) &&
            Mathf.Approximately(diagonalInches, 10.23f) )
        {
            matchValue = 1.0f; // Force Tablet Mode
            return true;
        }

        return false;
    }

    bool IsApplePhone()
    {
        #if UNITY_IOS
        string deviceModel = SystemInfo.deviceModel.ToLower();
        Debug.Log("🍏 iOS Device Model: " + deviceModel);
        return deviceModel.Contains("iphone");
        #endif
        return false;
    }

    bool IsAppleTablet()
    {
        #if UNITY_IOS
        string deviceModel = SystemInfo.deviceModel.ToLower();
        return deviceModel.Contains("ipad");
        #endif
        return false;
    }

    bool IsValidNonAppleTablet(out float matchValue)
    {
        matchValue = 0.0f;

        #if !UNITY_IOS
        float dpi = Screen.dpi;
        if (dpi <= 0 || dpi > 500)
        {
            dpi = 260f; // Approximate DPI for Android tablets
        }

        float screenWidthInches = Screen.width / dpi;
        float screenHeightInches = Screen.height / dpi;
        float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidthInches, 2) + Mathf.Pow(screenHeightInches, 2));

        Debug.Log($"📏 DPI: {dpi}, Width: {screenWidthInches:F2} inches, Height: {screenHeightInches:F2} inches, Diagonal: {diagonalInches:F2} inches");

        // ✅ If width is too small (~2.26 inches) or 4.62 inches, it's NOT a tablet
        if (screenWidthInches <= 4.7f)
        {
            matchValue = 0.0f;
            return true;
        }
        else if(screenWidthInches <= 5.54f)
        {
             matchValue = 0.0f;
            return true;
        }
        else
        {
            matchValue = 1.0f;
            return true;
        }

        return diagonalInches >= 6.5f && diagonalInches <= 14f;
        #endif
        return false;
    }
}
