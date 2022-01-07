/*****************************************************************************************************************
 - MainMenuUIManager.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Manages all of the UI's within the main menu scene (StartUI, LevelSelectUI, UpgradeUI, LoadingUI and
     OptionsUI). This class will contain the methods that will allow menus to switch from one to another.
*****************************************************************************************************************/


using UnityEngine;

public class MainMenuUIManager : UIManager
{
    #region Debugging

    /// <summary>
    /// Debugging variable, unlocks all of the levels available for play.
    /// </summary>
    public bool UnlockAllLevels;

    /// <summary>
    /// Debugging variable, disregard all gold calculations when upgrading ship,
    /// or anything else involving gold. Gold will be obtained normally during
    /// game play.
    /// </summary>
    public bool UnlimitedMoney;

    /// <summary>
    /// Debugging variable, saves the data to the local device, used if saving is
    /// not needed during testing.
    /// </summary>
    public bool SaveStats;

    /// <summary>
    /// Determines whether or not to enable GPGS for the player to be able to save to the cloud,
    /// access leaderboards and achievements.
    /// </summary>
    public bool EnableGPGS;

    #endregion


    #region Fields

    public static MainMenuUIManager Instance { get; private set; }

    // menus/UI's
    [SerializeField] StartUI menuStart;
    [SerializeField] LevelSelectUI menuLevelSelect;
    [SerializeField] UpgradeUI menuUpgrade;
    [SerializeField] CreditsUI menuCredits;
    [SerializeField] WatchAdUI menuWatchAd;

    public bool WatchAdMenuOpen;

    #endregion


    #region Built-In Methods

    /// <summary>
    /// Initializes the SaveManager and deactivates all of the menus except for the 
    /// start menu.
    /// </summary>
    void Awake()
    {
        Instance = this;
        SaveManager.Initialize();
        AdManager.Initialize();
        AdManager.ShowBanner();

        // attempt to load the file from the cloud if it doesn't already exist
        SaveManager.TryLoadCloudData();

        // every menu besides menuLoading will be active so that it can set up everything it needs before 
        // deactivating itself
        menuStart.gameObject.SetActive(true);
        menuLevelSelect.gameObject.SetActive(true);
        menuUpgrade.gameObject.SetActive(true);
        menuOptions.gameObject.SetActive(true);
        menuCredits.gameObject.SetActive(false);
        menuWatchAd.gameObject.SetActive(false);
        menuLoading.gameObject.SetActive(false);

        // initialize GPGS after appropirate setting action
        if (GooglePlayGamesService.IsSignedIn)
            UpdateGPGS();
        else
            GooglePlayGamesService.Initialize(EnableGPGS);
    }

    #endregion


    #region Public Methods

    /// <summary>
    /// Switches to the start UI and update the amount of gold displayed in the text box. Leave currentMenu null to not
    /// deactivate any menu and instead just have the target menu appear alongside this menu.
    /// </summary>
    /// <param name="currentMenu"></param>
    public void SwitchToStart(GameObject currentMenu = null)
    {
        WatchAdMenuOpen = false;
        OptionsMenuOpen = false;

        menuStart.UpdateGoldText();
        SwitchMenu(menuStart.gameObject, currentMenu, menuOptions.gameObject, menuWatchAd.gameObject);
    }

    /// <summary>
    /// Switches to the level select UI. Leave currentMenu null to not deactivate any menu and instead just have the target 
    /// menu appear alongside this menu.
    /// </summary>
    /// <param name="currentMenu"></param>
    public void SwitchToLevelSelect(GameObject currentMenu = null)
    {
        SwitchMenu(menuLevelSelect.gameObject, currentMenu, menuOptions.gameObject, menuWatchAd.gameObject);
    }

    /// <summary>
    /// Switches to the upgrade ship UI and update the amount of gold displayed in the text box. Leave currentMenu null to 
    /// not deactivate any menu and instead just have the target menu appear alongside this menu.
    /// </summary>
    /// <param name="currentMenu"></param>
    public void SwitchToUpgrade(GameObject currentMenu = null)
    {
        menuUpgrade.UpdateGoldText();
        menuUpgrade.UpdateAllUpgradeBars();
        SwitchMenu(menuUpgrade.gameObject, currentMenu, menuOptions.gameObject, menuWatchAd.gameObject);
    }

    /// <summary>
    /// Opens up the options bar on the right side of the screen. Leave currentMenu null to not
    /// deactivate any menu and instead just have the target menu appear alongside this menu.
    /// </summary>
    /// <param name="currentMenu"></param>
    public void SwitchToOptions(GameObject currentMenu = null)
    {
        UpdateGPGS();
        SwitchMenu(menuOptions.gameObject, currentMenu);
        menuOptions.ShowOptionsMenu();
    }

    /// <summary>
    /// Opens up the prompt that will ask the user whether or not they want to watch an ad to receive 50 gold.
    /// The menu will pop-up on top of the start UI.
    /// </summary>
    /// <param name="currentMenu"></param>
    public void SwitchToWatchAd(GameObject currentMenu = null)
    {
        SwitchMenu(menuWatchAd.gameObject, currentMenu);
        menuWatchAd.ShowWatchAdMenu();
    }

    /// <summary>
    /// Switches to the loading screen. Leave currentMenu null to notdeactivate any menu and instead just have the target menu 
    /// appear alongside this menu.
    /// </summary>
    /// <param name="currentMenu"></param>
    public void SwitchToLoading(GameObject currentMenu = null)
    {
        AdManager.HideBanner();
        SwitchMenu(menuLoading.gameObject, currentMenu, menuOptions.gameObject, menuLevelSelect.gameObject, menuStart.gameObject, menuWatchAd.gameObject);
    }

    /// <summary>
    /// Switches to the credits screen. Leave currentMenu null to notdeactivate any menu and instead just have the target menu 
    /// appear alongside this menu.
    /// </summary>
    /// <param name="currentMenu"></param>
    public void SwitchToCredits(GameObject currentMenu = null)
    {
        SwitchMenu(menuCredits.gameObject, currentMenu, menuOptions.gameObject, menuWatchAd.gameObject);
    }

    /// <summary>
    /// Updates the interactibility of the watch ad button on the start menu.
    /// </summary>
    public void UpdateAdButton()
    {
        menuStart.UpdateWatchAdButton(AdManager.IsRewardedReady());
    }

    /// <summary>
    /// Update gold text and play button in the start UI.
    /// </summary>
    public void UpdateUI()
    {
        menuStart.UpdatePlayButton();
        menuStart.UpdateGoldText();
    }

    /// <summary>
    /// Plays the animation to hide the watch ad menu and deactivates the game object after the animation is compelete.
    /// </summary>
    public void HideWatchAd()
    {
        menuWatchAd.HideWatchAdMenu();
        menuWatchAd.WaitForAnim(delegate
        {
            menuWatchAd.gameObject.SetActive(false);
        });
    }

    /// <summary>
    /// Check if start menu is active. Used for PlayerCutscene class.
    /// </summary>
    /// <returns></returns>
    public bool StartMenuActive()
    {
        return menuStart.Active();
    }

    #endregion


    #region Start Respective Game Modes

    /// <summary>
    /// Sets up the necessary variables for endless game mode and calls the load scene method from the loading UI.
    /// </summary>
    public void StartEndlessMode()
    {
        SaveManager.PlayerPersistentData.game_mode = GameManager.GameMode.Endless;
        SaveManager.PlayerPersistentData.level = 1;
        SwitchToLoading();
        menuLoading.LoadScene(GameManager.ENDLESS_SCENE_NAME);
    }

    /// <summary>
    /// Sets up the necessary variables for classic game mode and calls the load scene method from the loading UI.
    /// </summary>
    /// <param name="level"></param>
    public void StartClassicMode(int level)
    {
        SaveManager.PlayerPersistentData.game_mode = GameManager.GameMode.Classic;
        SaveManager.PlayerPersistentData.level = level;
        SwitchToLoading();
        menuLoading.LoadScene(GameManager.CLASSIC_SCENE_NAME);
    }

    /// <summary>
    /// Loads scene of name.
    /// </summary>
    /// <param name="name"></param>
    public void LoadScene(string name, int level, GameManager.GameMode mode)
    {
        SaveManager.PlayerPersistentData.game_mode = mode;
        SaveManager.PlayerPersistentData.level = level;
        SwitchToLoading();
        menuLoading.LoadScene(name);
    }

    #endregion
}
