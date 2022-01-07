/*****************************************************************************************************************
 - AdManager.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Manages all ad networks implemented in this game. Although there will only be AdMob, other networks can
     be added with relative ease (in theory).
*****************************************************************************************************************/

public static class AdManager
{
    /// <summary>
    /// Debugging boolean, set to false to enable live ads.
    /// </summary>
    public const bool AD_TEST_MODE = false;

    static bool initialized = false;


    #region Initialize

    /// <summary>
    /// Initializes the AdManager.
    /// </summary>
    public static void Initialize()
    {
        if (!initialized)
        {
            initialized = true;
            AdMob.Initialize();
            AdMob.OnAdMobRewarded += AdWatched;
            AdMob.OnRewardedAdLoaded += RewardedAdLoaded;
        }
    }

    #endregion


    #region Show/Hide Ads

    /// <summary>
    /// Show banner ad at bottom of screen.
    /// </summary>
    public static void ShowBanner()
    {
        AdMob.ShowBannerAd();
    }

    /// <summary>
    /// Show fullscreen interstital ad.
    /// </summary>
    public static void ShowInterstitial()
    {
        if (AdMob.IsInterstitalLoaded())
            AdMob.ShowInterstitialAd();
    }

    /// <summary>
    /// Show rewarded video ad.
    /// </summary>
    public static void ShowRewardedVideo()
    {
        if (AdMob.IsRewardedLoaded())
            AdMob.ShowRewardedVideoAd();
    }

    /// <summary>
    /// Hides banner, unless completeDestroy is set to true, then the banner
    /// will actually be de-reference.
    /// </summary>
    /// <param name="forceDestroy"></param>
    public static void HideBanner(bool forceDestroy = false)
    {
        AdMob.DestroyBanner(forceDestroy);
    }

    #endregion


    #region Check If Ad Ready

    /// <summary>
    /// Returns whether or not an interstitial ad is ready displayed.
    /// </summary>
    /// <returns></returns>
    public static bool IsInterstitialReady()
    {
        return AdMob.IsInterstitalLoaded();
    }

    /// <summary>
    /// Returns whether or not an rewarded video ad is ready to be played.
    /// </summary>
    /// <returns></returns>
    public static bool IsRewardedReady()
    {
        return AdMob.IsRewardedLoaded();
    }

    #endregion


    #region Utilities

    /// <summary>
    /// Called whenever a video ad is watched in its entirety. Any rewarded video callback method should call
    /// this method to award the reward appropriately and execute the desired action.
    /// </summary>
    /// <param name="reward"></param>
    static void AdWatched(double reward)
    {
        // only add award to player's gold if they watched an ad in the main menu
        if (MainMenuUIManager.Instance)
        {
            SaveManager.PlayerSaveData.lifetime_gold += (int) reward;
            SaveManager.PlayerSaveData.player_gold += (int) reward;
            MainMenuUIManager.Instance.UpdateUI();
            MainMenuUIManager.Instance.HideWatchAd();
        }
        else
        {
            InGameUIManager.Instance.UIRestartGame(true);
        }
    }

    /// <summary>
    /// Called whenever a rewarded video ad is fully loaded. This is to set the watch ad button
    /// on the start menu to be interactable so that the player can watch an ad.
    /// </summary>
    static void RewardedAdLoaded()
    {
        if (MainMenuUIManager.Instance)
            MainMenuUIManager.Instance.UpdateAdButton();
    }

    #endregion
}
