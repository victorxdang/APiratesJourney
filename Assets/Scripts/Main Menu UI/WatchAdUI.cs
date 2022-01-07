/*****************************************************************************************************************
 - WatchAdUI.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Handles all of the logic that occurs within the watch ad UI. 
*****************************************************************************************************************/

public class WatchAdUI : AnimatedUI
{
    #region Overridden Methods

    /// <summary>
    /// Don't update this game object.
    /// </summary>
    /// <returns></returns>
    public override bool Active()
    {
        return false;
    }

    /// <summary>
    /// Overriden method, plays the animation to hide this menu.
    /// </summary>
    public override void Back()
    {
        MainMenuUIManager.Instance.HideWatchAd();
    }

    #endregion


    #region Ad Menu Logic

    /// <summary>
    /// Closes the watch ad pop-up menu.
    /// </summary>
    public void CloseWatchAd()
    {
        AudioManager.Instance.PlayUIClick();
        Back();
    }

    /// <summary>
    /// Shows a rewarded video, called from button on click.
    /// </summary>
    public void ShowRewardedVideo()
    {
        AudioManager.Instance.PlayUIClick();
        AdManager.ShowRewardedVideo();
    }

    // <summary>
    /// Plays the animation where the watch ad menu transitions in from the left side of the screen.
    /// </summary>
    public void ShowWatchAdMenu()
    {
        MainMenuUIManager.Instance.WatchAdMenuOpen = true;
        SetAnimBool("WatchAd", true);
    }

    /// <summary>
    /// Plays the animation where the watch ad menu transitions out of the player's view.
    /// </summary>
    public void HideWatchAdMenu()
    {
        MainMenuUIManager.Instance.WatchAdMenuOpen = false;
        SetAnimBool("WatchAd", false);
    }

    #endregion
}
