/*****************************************************************************************************************
 - PlayerUI.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Updates and displays everything that is to be displayed on the player's UI during gameplay.
*****************************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : UIUtilities
{
    #region Fields

    [SerializeField] Image imageHealthFill,
                           imageReloadFill;

    [SerializeField] Button buttonPause;
    [SerializeField] Text textShipsLeft;

    #endregion


    #region Button Logic

    /// <summary>
    /// When the player presses the back button, have it display the Pause menu.
    /// </summary>
    public override void Back()
    {
        InGameUIManager.Instance.SwitchToPause();
    }

    /// <summary>
    /// This method is used when the player presses the pause button on the UI.
    /// </summary>
    public void Pause()
    {
        AudioManager.Instance.PlayUIClick();
        Back();
    }

    #endregion


    #region Update UI

    /// <summary>
    /// Fills the health icon to the appropriate amount that reflects the player's current hit points. 
    /// </summary>
    /// <param name="health"></param>
    public void UpdatePlayerHealth(float health)
    {
        imageHealthFill.fillAmount = health;
    }

    /// <summary>
    /// Fills the reload icon to the appropriate amount that reflect how long is left before the player
    /// can fire again.
    /// </summary>
    /// <param name="time"></param>
    public void UpdatePlayerReload(float time)
    {
        imageReloadFill.fillAmount = time;
    }

    /// <summary>
    /// Updates how many ships are left (if in classic mode) or how many ships the player has destroyed
    /// (if in endless mode) after the player has sunk an enemy ship.
    /// </summary>
    /// <param name="ships"></param>
    public void UpdateNumberOfShips(int ships)
    {
        if (SaveManager.PlayerPersistentData.game_mode == GameManager.GameMode.Classic)
            textShipsLeft.text = "Ships Left: \n" + ships;
        else
            textShipsLeft.text = "Ships Destroyed: \n" + ships;
    }

    /// <summary>
    /// Updates the interactability of the pause button.
    /// </summary>
    /// <param name="interactable"></param>
    public void UpdatePauseButton(bool interactable)
    {
        buttonPause.interactable = interactable;
    }

    /// <summary>
    /// Updates the button's current status, either on or off.
    /// </summary>
    /// <param name="active"></param>
    public void SetPauseButtonActive(bool active)
    {
        buttonPause.gameObject.SetActive(active);
    }

    #endregion
}
