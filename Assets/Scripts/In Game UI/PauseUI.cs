/*****************************************************************************************************************
 - PauseUI.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Moves the pause menu to the appropriate position when the player presses on the pause button on the 
     player's UI.
*****************************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class PauseUI : AnimatedUI
{
    #region Fields

    [SerializeField] Button buttonOptions;

    #endregion


    #region Update Manager

    /// <summary>
    /// Overridden method, have it where this object isn't added to the UpdateManager. The PauseUI
    /// will use the built-in Update() method since there is a bug when using UpdateMe() where if 
    /// the player presses the back button on the PlayerUI, then the PauseUI will immediately close
    /// since the back press was registered in the same frame as activating the PauseUI so the PauseIU
    /// will immediately hide as it is trying to show. Every other menu seems to work just normally
    /// with the UpdateManager.
    /// 
    /// I hope that wasn't too confusing.
    /// </summary>
    protected override void Start()
    {
    }

    /// <summary>
    /// Closes the options menu if it currently open or returns to the player UI and resumes the game.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (InGameUIManager.Instance.OptionsMenuOpen)
                InGameUIManager.Instance.HideOptions();
            else
                Back();
        }
    }

    #endregion


    #region Utilities

    /// <summary>
    /// Updates the button's current status, either on or off.
    /// </summary>
    /// <param name="active"></param>
    public void SetOptionsButtonActive(bool active)
    {
        buttonOptions.gameObject.SetActive(active);
    }

    /// <summary>
    /// Plays the animation where the pause menu transitions in from the left side of the screen.
    /// </summary>
    public void ShowPauseMenu()
    {
        SetAnimBool("Pause", true);
    }

    /// <summary>
    /// Plays the animation where the pause menu transitions out of the player's view.
    /// </summary>
    public void HidePauseMenu()
    {
        SetAnimBool("Pause", false);
    }

    #endregion


    #region Button Logic

    /// <summary>
    /// Resumes the game.
    /// </summary>
    public void Resume()
    {
        AudioManager.Instance.PlayUIClick();
        Back();
    }

    /// <summary>
    /// Restarts the game.
    /// </summary>
    public void Restart()
    {
        AudioManager.Instance.PlayUIClick();
        InGameUIManager.Instance.UIRestartGame(false);
    }

    /// <summary>
    /// Exits game to main menu screen.
    /// </summary>
    public void MainMenu()
    {
        AudioManager.Instance.PlayUIClick();
        InGameUIManager.Instance.UIMainMenu();
    }

    /// <summary>
    /// Displays the options menu.
    /// </summary>
    public void Options()
    {
        AudioManager.Instance.PlayUIClick();
        InGameUIManager.Instance.SwitchToOptions();
    }

    #endregion
}
