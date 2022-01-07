/*****************************************************************************************************************
 - InGameUIManager.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Manages all of the UI's within the main menu scene (PlayerUI, PauseUI, GameOverUI, LoadingUI and
     OptionsUI). This class will contain the methods that will allow menus to switch from one to another.
*****************************************************************************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameUIManager : UIManager
{
    #region Debugging

    /// <summary>
    /// Debugging variable, determines whether or not the player will take any damage from any source
    /// (enemies or obstacles).
    /// </summary>
    public bool PlayerTakeDamage;

    /// <summary>
    /// Determines whether or not enemy ships should fire at the player.
    /// </summary>
    public bool EnemyShouldFire;

    /// <summary>
    /// Determines whether or not to move the map.
    /// </summary>
    public bool MoveMap;

    #endregion


    #region Fields

    public static InGameUIManager Instance { get; private set; }

    public Transform ammoBay;

    [SerializeField] PlayerUI menuPlayerUI;
    [SerializeField] PauseUI menuPause;
    [SerializeField] GameOverUI menuGameOver;
    [SerializeField] TutorialUI menuTutorial;

    #endregion


    #region Built-In Methods

    /// <summary>
    /// Initializes the GameManager (if not yet initialized from the start menu) and deactivates all UI's except
    /// for the playerUI.
    /// </summary>
    void Awake()
    {
        Instance = this;
        GameManager.Initialize();

        menuPlayerUI.gameObject.SetActive(true);
        menuPause.gameObject.SetActive(false);
        menuGameOver.gameObject.SetActive(false);
        menuOptions.gameObject.SetActive(true);

        menuTutorial.gameObject.SetActive(false);
        menuLoading.gameObject.SetActive(false);

        SaveManager.TryLoadCloudData();

        if (SaveManager.PlayerPersistentData.level == 0)
            SwitchToTutorial();

        if (GooglePlayGamesService.IsSignedIn)
            UpdateGPGS();
    }

    /// <summary>
    /// Starts the game by calling the StartGame method in GameManager.
    /// </summary>
    void Start()
    {
        AdManager.HideBanner();
        GameManager.StartGame();      
    }

    #endregion


    #region Update Player UI

    /// <summary>
    /// Updates the player's current health.
    /// </summary>
    /// <param name="health"></param>
    public void UpdateHealthIcon(float health)
    {
        menuPlayerUI.UpdatePlayerHealth(health);
    }

    /// <summary>
    /// Updates the time remaining until the player is able to fire again.
    /// </summary>
    /// <param name="time"></param>
    public void UpdateReloadIcon(float time)
    {
        menuPlayerUI.UpdatePlayerReload(time);
    }

    /// <summary>
    /// Update how many ships are either remaining on the map (if playing in classic mode) or
    /// how many ships the player has destroyed (if playing in endless mode).
    /// </summary>
    /// <param name="num"></param>
    public void UpdateNumberOfShips(int num)
    {
        menuPlayerUI.UpdateNumberOfShips(num);
    }

    #endregion


    #region Menu Switching

    public void SwitchToTutorial(GameObject currentMenu = null)
    {
        menuPlayerUI.SetPauseButtonActive(false);
        SwitchMenu(menuTutorial.gameObject, currentMenu);
    }

    /// <summary>
    /// Switch to the player UI. Leave currentMenu null to not deactivate any menu and instead just have the 
    /// target menu appear alongside this menu.
    /// </summary>
    /// <param name="currentMenu"></param>
    public void SwitchToInGame(GameObject currentMenu = null)
    {
        OptionsMenuOpen = false;

        menuPause.HidePauseMenu();
        menuPause.WaitForAnim(delegate
        {
            menuPlayerUI.SetPauseButtonActive(true);
            SwitchMenu(menuPlayerUI.gameObject, currentMenu, menuOptions.gameObject);

            AdManager.HideBanner();
            GameManager.ResumeGame();
        });
    }

    /// <summary>
    /// Switch to the pause UI. Leave currentMenu null to not deactivate any menu and instead just have the 
    /// target menu appear alongside this menu.
    /// </summary>
    public void SwitchToPause(GameObject currentMenu = null)
    {
        SwitchMenu(menuPause.gameObject, currentMenu, menuPlayerUI.gameObject);
        menuPause.ShowPauseMenu();

        AdManager.ShowBanner();
        GameManager.PauseGame();
    }

    /// <summary>
    /// Opens up the options bar on the right side of the screen. Leave currentMenu null to not
    /// deactivate any menu and instead just have the target menu appear alongside this menu.
    /// </summary>
    /// <param name="currentMenu"></param>
    public void SwitchToOptions(GameObject currentMenu = null)
    {
        SwitchMenu(menuOptions.gameObject, currentMenu);
        menuOptions.UpdateGPGSButtons(GooglePlayGamesService.IsSignedIn);
        menuOptions.ShowOptionsMenu();
    }

    /// <summary>
    /// Switches to the loading screen. Leave currentMenu null to notdeactivate any menu and instead just have the target menu 
    /// appear alongside this menu.
    /// </summary>
    /// <param name="currentMenu"></param>
    public void SwitchToLoading(GameObject currentMenu = null)
    {
        AdManager.HideBanner();
        SwitchMenu(menuLoading.gameObject, currentMenu, menuOptions.gameObject, menuPlayerUI.gameObject, menuPause.gameObject);
    }

    /// <summary>
    /// Switches to the game over UI. From here, the appropriate text will display depending on the status of completedLevel. If 
    /// completed level is true, then the UI will display a congratulatory message, otherwise, a different message will appear.
    /// </summary>
    /// <param name="completedLevel"></param>
    /// <param name="text"></param>
    /// <param name="subText"></param>
    public void SwitchToGameOver(bool completedLevel, string text, string subTextTop, string subTextBottom)
    {
        AdManager.ShowBanner();

        // display an interstital ad after the player has played through 5 games
        if (--SaveManager.PlayerPersistentData.games_until_ad <= 0)
        {
            AdManager.ShowInterstitial();
            SaveManager.PlayerPersistentData.games_until_ad = 5;
        }

        SwitchMenu(menuGameOver.gameObject, menuPause.gameObject, menuPlayerUI.gameObject);
        menuGameOver.UpdateButtonStatus(completedLevel);
        menuGameOver.UpdateGameOverTextAll(text, subTextTop, subTextBottom);
    }

    #endregion


    #region Button Logic

    /// <summary>
    /// Display a rewarded video ad for the player to watch.
    /// </summary>
    public void UIWatchAd()
    {
        AdManager.ShowRewardedVideo();
    }

    /// <summary>
    /// Moves the player to the next level (in reality, the level from PlayerPersistentData is incremented and then the
    /// game is restarted).
    /// </summary>
    public void UINextLevel()
    {
        SaveManager.PlayerPersistentData.level++;
        UIRestartGame(false);
    }

    /// <summary>
    /// Restarts the game. If this method is called, then the game will reset the current gold and current score in PlayerPersistentData.
    /// </summary>
    /// <param name="watchedAd"></param>
    public void UIRestartGame(bool watchedAd)
    {
        AudioManager.Instance.StopShipSinkingSE();
        GameManager.RestartGame(watchedAd);
        SwitchToLoading(menuGameOver.gameObject);
        menuLoading.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Switches to the main menu scene. This will load immediately into the start menu, instead of having a loading screen pop up.
    /// </summary>
    public void UIMainMenu()
    {
        AudioManager.Instance.StopShipSinkingSE();
        GameManager.RestartGame(false);
        menuLoading.LoadScene(GameManager.MAIN_MENU_SCENE_NAME, false);
    }

    #endregion
}
