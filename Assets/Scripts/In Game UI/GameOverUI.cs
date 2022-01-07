/*****************************************************************************************************************
 - GameOverUI.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Updates the texts that are displayed once the game is over.
*****************************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    #region Fields

    [SerializeField] Button buttonWatchAd,
                            buttonNextLevel;

    [SerializeField] Text textGameOver,
                          subTextTop,
                          subTextBottom;

    #endregion


    #region Update/Buttons Text

    /// <summary>
    /// If developers are lazy to call each text/sub-text method (I wouldn't blame you guys) then just call this method!
    /// </summary>
    /// <param name="text"></param>
    /// <param name="topSubText"></param>
    /// <param name="bottomSubText"></param>
    public void UpdateGameOverTextAll(string text, string topSubText, string bottomSubText)
    {
        UpdateGameOverText(text);
        UpdateGameOverSubTexTop(topSubText);
        UpdateGameOverSubTextBottom(bottomSubText);
    }

    /// <summary>
    /// Updates the primary text that the player sees (the bigger text).
    /// </summary>
    /// <param name="text"></param>
    public void UpdateGameOverText(string text)
    {
        textGameOver.text = text;
    }

    /// <summary>
    /// Updates the secondary text that the player sees (the smaller text below the primary text).
    /// </summary>
    /// <param name="subText"></param>
    public void UpdateGameOverSubTexTop(string subText)
    {
        int gold;

        if (int.TryParse(subText, out gold))
        {
            subTextTop.text = "+" + subText;
            subTextTop.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            subTextTop.text = subText;
        }
    }

    /// <summary>
    /// Updates the secondary text that the player sees (the smaller text below the primary text).
    /// </summary>
    /// <param name="subText"></param>
    public void UpdateGameOverSubTextBottom(string subText)
    {
        int gold;

        if (int.TryParse(subText, out gold))
        {
            subTextBottom.text = "+" + subText;
            subTextBottom.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            subTextBottom.text = subText;
        }
    }

    /// <summary>
    /// Updates the status of the next level button and the watch ad button. Watch ad will not appear in classic game mode,
    /// next level will not appear if the player didn't complete the level or if the game is endless, home and restart should 
    /// appear regardless of game mode or whether the player has completed the level or not.
    /// </summary>
    /// <param name="completedLevel"></param>
    public void UpdateButtonStatus(bool completedLevel)
    {
        buttonNextLevel.gameObject.SetActive(completedLevel && SaveManager.PlayerPersistentData.game_mode == GameManager.GameMode.Classic && SaveManager.PlayerPersistentData.level < Scaling.max_levels);
        buttonWatchAd.gameObject.SetActive(SaveManager.PlayerPersistentData.game_mode == GameManager.GameMode.Endless);

        if (buttonWatchAd.gameObject.activeSelf)
            buttonWatchAd.interactable = !SaveManager.PlayerPersistentData.watched_videoad && AdManager.IsRewardedReady();
    }

    #endregion


    #region Button Logic

    /// <summary>
    /// Shows a rewarded video ad for the player to watch.
    /// </summary>
    public void WatchAd()
    {
        AudioManager.Instance.PlayUIClick();
        InGameUIManager.Instance.UIWatchAd();
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
    /// Returns to the main menu screen.
    /// </summary>
    public void MainMenu()
    {
        AudioManager.Instance.PlayUIClick();
        InGameUIManager.Instance.UIMainMenu();
    }

    /// <summary>
    /// Advances the game to the next level.
    /// </summary>
    public void NextLevel()
    {
        AudioManager.Instance.PlayUIClick();
        InGameUIManager.Instance.UINextLevel();
    }

    #endregion
}