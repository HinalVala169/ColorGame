using UnityEngine;

public class GoogleMobileAdsSampleScripts : MonoBehaviour
{
    public void ShowBanner()
    {
        GoogleMobileAdsScript.USE.ShowBanner();
    }

    public void HideBanner()
    {
        GoogleMobileAdsScript.USE.HideBanner();
    }

    public void ShowInterstitial()
    {
        GoogleMobileAdsScript.USE.ShowInterstitial();
    }

    public void ShowRewardAd()
    {
        GoogleMobileAdsScript.USE.ShowRewardAd();
    }
}