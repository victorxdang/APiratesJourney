/*****************************************************************************************************************
 - AdMob.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Handles setting up all AdMob related ads and callbacks.

     NOTE: To enable live ads, set the AD_TEST_MODE variable in AdManager to true. 
     Set it to false to use test ads.
*****************************************************************************************************************/

using GoogleMobileAds.Api;

public static class AdMob
{
    #region Editable Region

    //---------------------------------------------------------------------------
    // Begin Editable variables
    //---------------------------------------------------------------------------

    #if UNITY_ANDROID
        // the id of the app (AdMob)
        const string ADMOB_APPID_ANDROID = "ca-app-pub-8168057146331582~6943389868";

        // the id of each live ad unit (AdMob)
        const string BANNER_ANDROID = "ca-app-pub-8168057146331582/9186409827";
        const string INTERSTITIAL_ANDROID = "ca-app-pub-8168057146331582/9800298125";
        const string REWARDED_ANDROID = "ca-app-pub-8168057146331582/9855783222";

        // the id of each test ad unit (AdMob)
        const string TEST_BANNER_ANDROID = "ca-app-pub-3940256099942544/6300978111";
        const string TEST_INTERSTITIAL_ANDROID = "ca-app-pub-3940256099942544/1033173712";
        const string TEST_REWARDED_ANDROID = "ca-app-pub-3940256099942544/5224354917";
    #elif UNITY_IOS
        // the id of the app (AdMob)
        const string ADMOB_APPID_IPHONE = "";

        // the id of each test ad unit (AdMob)
        const string BANNER_IPHONE = "";
        const string INTERSTITIAL_IPHONE = "";
        const string REWARDED_IPHONE = "";

        // the id of each live ad unit (AdMob)
        const string TEST_BANNER_IPHONE = "ca-app-pub-3940256099942544/2934735716";
        const string TEST_INTERSTITIAL_IPHONE = "ca-app-pub-3940256099942544/4411468910";
        const string TEST_REWARDED_IPHONE = "ca-app-pub-3940256099942544/1712485313";
    #endif

    //---------------------------------------------------------------------------
    // End Editable variables
    //---------------------------------------------------------------------------

    #endregion


    #region Fields

    public static UnityEngine.Events.UnityAction<double> OnAdMobRewarded;
    public static UnityEngine.Events.UnityAction OnRewardedAdLoaded;

    // cached variables
    static string adMobAppID,
                  adMobBannerID,
                  adMobInterstitalID,
                  adMobRewardedID;

    static BannerView adMobBanner;
    static InterstitialAd adMobInterstitial;
    static RewardBasedVideoAd adMobRewardVideo;

    #endregion


    #region Initialize

    public static void Initialize()
    {
        #if UNITY_ANDROID
            adMobAppID = ADMOB_APPID_ANDROID;
            adMobBannerID = (AdManager.AD_TEST_MODE) ? TEST_BANNER_ANDROID : BANNER_ANDROID;
            adMobInterstitalID = (AdManager.AD_TEST_MODE) ? TEST_INTERSTITIAL_ANDROID : INTERSTITIAL_ANDROID;
            adMobRewardedID = (AdManager.AD_TEST_MODE) ? TEST_REWARDED_ANDROID : REWARDED_ANDROID;
        #elif UNITY_IOS
            adMobAppID = ADMOB_APPID_IPHONE;
            adMobBannerID = (AdManager.AD_TEST_MODE) ? TEST_BANNER_IPHONE : BANNER_IPHONE;
            adMobInterstitalID = (AdManager.AD_TEST_MODE) ? TEST_INTERSTITIAL_IPHONE : INTERSTITIAL_IPHONE;
            adMobRewardedID = (AdManager.AD_TEST_MODE) ? TEST_REWARDED_IPHONE : REWARDED_IPHONE;
        #else
            adMobAppID = "unexpected_platform";
            adMobBannerID = "unexpected_platform";
            adMobInterstitalID = "unexpected_platform";
            adMobRewardedID = "unexpected_platform";
        #endif

        // initialize AdMob ads
        MobileAds.Initialize(adMobAppID);

        // initialize and load the various ad types, banners show up immediately after
        // being called so it is defaulted to hide the ad when loaded. Use ShowBannerAd()
        // method to display the ad when needed
        RequestAdMobBannerAd();

        // request interstitial and rewarded video is outside of initializedIDs flag is because
        // these two will be requested each time the game starts or restarts a level.
        RequestAdMobInterstitialAd();
        RequestAdMobRewardedVideoAd();
    }

    #endregion


    #region Show Ads

    /// <summary>
    /// Displays a banner ad at the bottom of the screen.
    /// </summary>
    public static void ShowBannerAd()
    {
        if (adMobBanner == null)
            RequestAdMobBannerAd();

        adMobBanner.Show();
    }

    /// <summary>
    /// Shows a full screen ad, either interactable or static, or a skippable video.
    /// </summary>
    public static void ShowInterstitialAd()
    {
        adMobInterstitial.Show();
    }

    /// <summary>
    /// Call to play a rewarded video ad (unskippable). Prioritizes AdMob over Unity ads.
    /// </summary>
    public static void ShowRewardedVideoAd()
    {
        adMobRewardVideo.Show();
    }

    #endregion


    #region Check if Ad Ready

    /// <summary>
    /// Returns whether or not an interstital ad has been loaded.
    /// </summary>
    /// <returns></returns>
    public static bool IsInterstitalLoaded()
    {
        return adMobInterstitial.IsLoaded();
    }

    /// <summary>
    /// Returns whether or not a rewarded video ad has been loaded.
    /// </summary>
    /// <returns></returns>
    public static bool IsRewardedLoaded()
    {
        return adMobRewardVideo.IsLoaded();
    }

    #endregion


    #region Destroy Ads

    /// <summary>
    /// Should be called whenever the game exits.
    /// </summary>
    public static void CleanUp()
    {
        DestroyBanner(true); // actually destroy the banner
        DestroyInterstitial();
    }

    /// <summary>
    /// Destroys any banner ads on the screen. However, under normal circumstances, the banner will only be
    /// hidden and not actually destroyed. If froceDestroy is true, then the banner will then be de-referenced.
    /// </summary>
    /// <param name="forceDestroy"> Set to true to de-reference banner, false to hide it. </param>
    public static void DestroyBanner(bool forceDestroy = false)
    {
        // destroy AdMob banner ad (if specified), else hide it
        if (adMobBanner != null)
        {
            if (forceDestroy)
                adMobBanner.Destroy();
            else
                adMobBanner.Hide();
        }
    }

    /// <summary>
    /// De-references the interstitial ad on the screen.
    /// </summary>
    public static void DestroyInterstitial()
    {
        if (adMobInterstitial != null)
            adMobInterstitial.Destroy();
    }

    #endregion


    #region Request Ads

    /// <summary>
    /// Creates a new banner ad that will be displayed on the bottom of screen.
    /// There is no Show() method call for banner so after it is finished creating, then
    /// the banner ad will be displayed on the screen.
    /// 
    /// This method should only be called once.
    /// </summary>
    static void RequestAdMobBannerAd()
    {
        // create the banner ad
        adMobBanner = new BannerView(adMobBannerID, AdSize.Banner, AdPosition.Bottom);

        adMobBanner.OnAdLoaded += HandleAdMobBannerAdLoaded;
        adMobBanner.OnAdFailedToLoad += HandleAdMobBannerAdFailedToLoad;

        // load the banner ad to display
        adMobBanner.LoadAd(new AdRequest.Builder().Build());
    }

    /// <summary>
    /// Creates a new full screen interstitial ad. To show an interstital, the
    /// ShowInterstitialAd() method needs to be called, this method only loads the
    /// interstital ad.
    /// </summary>
    static void RequestAdMobInterstitialAd()
    {
        // destroy the previous interstital (if one exists) before creating a new one
        DestroyInterstitial();

        // create a new interstitial ad
        adMobInterstitial = new InterstitialAd(adMobInterstitalID);

        adMobInterstitial.OnAdLoaded += HandleAdMobInterstitialAdLoaded;
        adMobInterstitial.OnAdFailedToLoad += HandleAdMobInterstitialAdFailedToLoad;

        // load the interstital ad
        adMobInterstitial.LoadAd(new AdRequest.Builder().Build());
    }

    /// <summary>
    /// Loads a rewarded video ad. To show the rewarded video ad, the 
    /// ShowRewardedVideoAd() needs to be called, like the interstitial
    /// method, this method will only load the rewarded video ad.
    /// </summary>
    static void RequestAdMobRewardedVideoAd()
    {
        // get the singleton of rewarded video ads
        adMobRewardVideo = RewardBasedVideoAd.Instance;

        adMobRewardVideo.OnAdLoaded += HandleAdMobRewardedVideoLoaded;
        adMobRewardVideo.OnAdRewarded += HandleAdMobRewardedAd;
        adMobRewardVideo.OnAdFailedToLoad += HandleAdMobRewardedVideoFailedToLoad;

        // load the rewarded video ad
        adMobRewardVideo.LoadAd(new AdRequest.Builder().Build(), adMobRewardedID);
    }

    #endregion


    #region Callbacks

    /// <summary>
    /// This method handles the events that should occur after a banner ad is loaded.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    static void HandleAdMobBannerAdLoaded(object sender, System.EventArgs args)
    {
        // nothing happens here, yet
    }

    /// <summary>
    /// Handle for whenever a banner ad fails to load (i.e. no internet connection).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    static void HandleAdMobBannerAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        // nothing happens here, yet
    }

    /// <summary>
    /// This method handles the events that should occur after an interstitial ad is loaded.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    static void HandleAdMobInterstitialAdLoaded(object sender, System.EventArgs args)
    {
        // nothing happens here, yet
    }

    /// <summary>
    /// Handle for whenever an interstitial ad fails to load (i.e. no internet connection).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    static void HandleAdMobInterstitialAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        // nothing happens here, yet
    }

    /// <summary>
    /// This handles the events that should occur after the rewarded video ad is completed and 
    /// closed by the user.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="reward"></param>
    static void HandleAdMobRewardedAd(object sender, Reward reward)
    {
        RequestAdMobRewardedVideoAd();
        OnAdMobRewarded?.Invoke(reward.Amount);
    }

    /// <summary>
    /// Handle for when the video ad successfully loads.
    /// </summary>
    static void HandleAdMobRewardedVideoLoaded(object sender, System.EventArgs args)
    {
        OnRewardedAdLoaded?.Invoke();
    }

    /// <summary>
    /// Handle for whenever an rewarded video fails to load (i.e. no internet connection).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    static void HandleAdMobRewardedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        // nothing happens here, yet
    }

    #endregion
}
