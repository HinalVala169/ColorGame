#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;

public class ScreenshotManager : MonoBehaviour
{
    public static event Action ScreenshotFinishedSaving;

#if UNITY_IPHONE
    [DllImport("__Internal")]
    private static extern bool saveToGallery(string path);
#elif UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void ImageDownloader(string str, string fn);

    private static void DownloadScreenshot(byte[] imageData, string imageFilename)
    {
        ImageDownloader(Convert.ToBase64String(imageData), imageFilename);
    }
#endif

    public static IEnumerator SaveForPaint(string fileName, string albumName = "MyScreenshots", bool callback = false)
    {
        string date = DateTime.Now.ToString("dd-MM-yy");
        ScreenShotNumber++;

        string screenshotFilename = $"{fileName}_{ScreenShotNumber}_{date}.jpg";
        Rect rect = new Rect(0, 0, Screen.width, Screen.height);

        Debug.Log("Save screenshot " + screenshotFilename);

        string shareText = "I drew this painting! ^_^\nDo you like drawing too? Then install Coloring Book!";

#if UNITY_IPHONE
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Debug.Log("iOS platform detected");

            yield return CaptureAndSaveScreenshot(rect, screenshotFilename, albumName, shareText, callback);
        }
        else
        {
            yield return CaptureScreenshot(rect, screenshotFilename);
        }
#elif UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android)
        {
            Debug.Log("Android platform detected");

            yield return CaptureAndSaveScreenshot(rect, screenshotFilename, albumName, shareText, callback);
        }
        else
        {
            yield return CaptureScreenshot(rect, screenshotFilename);
        }
#elif UNITY_WEBGL
        yield return CaptureScreenshotWebGL(rect, screenshotFilename);
#else
        Debug.LogWarning("Platform not supported for this operation.");
#endif
    }

    private static IEnumerator CaptureScreenshot(Rect rect, string screenshotFilename)
    {
        yield return new WaitForEndOfFrame();

        Texture2D texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
        texture.ReadPixels(rect, 0, 0);
        texture.Apply();

        byte[] bytes = texture.EncodeToJPG();

        string path = Path.Combine(Application.persistentDataPath, screenshotFilename);
        File.WriteAllBytes(path, bytes);

        Destroy(texture);
    }

    private static IEnumerator CaptureAndSaveScreenshot(Rect rect, string screenshotFilename, string albumName, string shareText, bool callback)
    {
        yield return CaptureScreenshot(rect, screenshotFilename);

        string path = Path.Combine(Application.temporaryCachePath, screenshotFilename);
        byte[] bytes = File.ReadAllBytes(path);

        NativeGallery.SaveImageToGallery(bytes, albumName, screenshotFilename);
        new NativeShare().AddFile(path).SetSubject(albumName).SetText(shareText).Share();

        if (callback)
            ScreenshotFinishedSaving?.Invoke();
    }

#if UNITY_WEBGL
    private static IEnumerator CaptureScreenshotWebGL(Rect rect, string screenshotFilename)
    {
        yield return new WaitForEndOfFrame();

        Texture2D texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
        texture.ReadPixels(rect, 0, 0);
        texture.Apply();

        byte[] bytes = texture.EncodeToJPG();

        DownloadScreenshot(bytes, screenshotFilename);

        Destroy(texture);
    }
#endif

    public static int ScreenShotNumber
    {
        get => PlayerPrefs.GetInt("screenShotNumber", 0);
        set => PlayerPrefs.SetInt("screenShotNumber", value);
    }
}
