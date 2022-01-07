/*****************************************************************************************************************
 - OptionsUI.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Handles the options bar on the right side of the screen. This will contain tilt/drag buttons and GPGS
     stuff.
*****************************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : AnimatedUI
{
    #region Fields

    [SerializeField] Button buttonTilt,
                            buttonDrag,
                            buttonLeaderboards,
                            buttonAchievements,
                            buttonSignIn,
                            buttonBack;

    #endregion


    #region Built-In Methods

    /// <summary>
    /// Sets up buttons to the appropriate action.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        UpdateTiltDrag();
        UpdateGPGSButtons(false);
        gameObject.SetActive(false);
    }

    #endregion


    #region Overriden Methods

    /// <summary>
    /// Don't update this game object.
    /// </summary>
    /// <returns></returns>
    public override bool Active()
    {
        return false;
    }

    /// <summary>
    /// Overriden method, hide the options menu and play a click sound.
    /// </summary>
    public override void Back()
    {
        HideOptionsMenu();
    }

    #endregion


    #region Update UI/Settings

    /// <summary>
    /// Update the tilt/drag buttons to display appropriately. If tilt is the current setting, then change the tilt button's 
    /// icon to be orange and the drag icon to be gray (normal). Vice versa if drag is currently selected.
    /// </summary>
    void UpdateTiltDrag()
    {
        buttonTilt.transform.GetChild(0).GetComponent<Image>().color = (SaveManager.PlayerSaveData.use_tilt) ? new Color(1, 0.5f, 0) : Color.gray;
        buttonDrag.transform.GetChild(0).GetComponent<Image>().color = (SaveManager.PlayerSaveData.use_tilt) ? Color.gray : new Color(1, 0.5f, 0);
    }

    /// <summary>
    /// Only display the leaderboards and achievements buttons if the player is signed into Google Play Games. If the player
    /// is signed in, then the sign-in button will display "Sign Out". Vice versa if the player has not yet signed in.
    /// </summary>
    /// <param name="signedIn"></param>
    public void UpdateGPGSButtons(bool signedIn)
    {
        buttonLeaderboards.interactable = signedIn;
        buttonAchievements.interactable = signedIn;
        buttonSignIn.transform.parent.GetChild(1).GetComponent<Text>().text = (signedIn) ? "Sign Out" : "Sign In";

        // unlock Ahoy, Cap'n achivement if signed in successfully for the first time
        if (signedIn)
            GooglePlayGamesService.UnlockAchievement(GPGSIds.achievement_ahoy_capn);
    }

    #endregion


    #region Display/Hide Menu

    /// <summary>
    /// Play the animation to bring the options bar into view.
    /// </summary>
    public void ShowOptionsMenu()
    {
        if (MainMenuUIManager.Instance)
            MainMenuUIManager.Instance.OptionsMenuOpen = true;
        else
            InGameUIManager.Instance.OptionsMenuOpen = true;

        buttonBack.gameObject.SetActive(true);
        buttonBack.interactable = true;
        SetAnimBool("Options", true);
    }

    /// <summary>
    /// Play the animation to hide the options bar.
    /// </summary>
    public void HideOptionsMenu()
    {
        if (MainMenuUIManager.Instance)
            MainMenuUIManager.Instance.OptionsMenuOpen = false;
        else
            InGameUIManager.Instance.OptionsMenuOpen = false;

        buttonBack.interactable = false;
        SetAnimBool("Options", false);

        WaitForAnim(delegate
        {
            buttonBack.gameObject.SetActive(false);
            gameObject.SetActive(false);
        });
    }

    #endregion


    #region Button Logic

    /// <summary>
    /// Change the settings to tilt.
    /// </summary>
    public void SetTilt()
    {
        AudioManager.Instance.PlayUIClick();
        SaveManager.PlayerSaveData.use_tilt = true;
        GameManager.SettingsUpdated();
        GameManager.Calibrate();

        UpdateTiltDrag();
    }

    /// <summary>
    /// Change the settings to drag.
    /// </summary>
    public void SetDrag()
    {
        AudioManager.Instance.PlayUIClick();
        SaveManager.PlayerSaveData.use_tilt = false;
        GameManager.SettingsUpdated();

        UpdateTiltDrag();
    }

    /// <summary>
    /// Displays the Google Play leaderboards.
    /// </summary>
    public void OpenGPGSLeaderboards()
    {
        AudioManager.Instance.PlayUIClick();
        GooglePlayGamesService.ShowLeaderboardsUI();
    }

    /// <summary>
    /// Displays the Google Play achivements.
    /// </summary>
    public void OpenGPGSAchievements()
    {
        AudioManager.Instance.PlayUIClick();
        GooglePlayGamesService.ShowAchievementsUI();
    }

    /// <summary>
    /// Sign into Google Play Games if not signed in. If already signed in, then sign out of 
    /// Google Play Games.
    /// </summary>
    public void OpenGPGSSignIn()
    {
        AudioManager.Instance.PlayUIClick();
        GooglePlayGamesService.ShowSignInUI();
    }

    /// <summary>
    /// Opens up the web page that contains the privacy policy.
    /// </summary>
    public void OpenPrivacyPolicy()
    {
        AudioManager.Instance.PlayUIClick();
        Application.OpenURL(Hyperlinks.PRIVACY_POLICY);
    }

    /// <summary>
    /// Closes the options menu.
    /// </summary>
    public void CloseOptions()
    {
        AudioManager.Instance.PlayUIClick();
        Back();
    }

    #endregion
}
