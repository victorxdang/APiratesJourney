/*****************************************************************************************************************
 - StartUI.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Handles all of the logic that occurs within the start UI. 
*****************************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class StartUI : UIUtilities
{
    #region Fields

    [SerializeField] Button buttonPlay,
                            buttonWatchAd;

    [SerializeField] Text textGold;

    // cache fields
    bool pressedOnce;
    float timeOfClick;
    AndroidJavaObject currentActivity;

    AndroidJavaClass toastClass,
                     unityPlayer;

    #endregion


    #region Built-In Methods

    /// <summary>
    /// Sets up all of the buttons within the start UI. Updates the gold text so that it reflects the amount of gold
    /// that the player currently has.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        SetupToast();
        UpdateGoldText();
        UpdatePlayButton();
        UpdateWatchAdButton(AdManager.IsRewardedReady());
    }

    #endregion


    #region Update Manager

    /// <summary>
    /// Overriden method, used to exit the game if the player hits the android back button twice, or
    /// close the options menu if it is open.
    /// </summary>
    public override void UpdateMe()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (MainMenuUIManager.Instance.WatchAdMenuOpen)
            {
                MainMenuUIManager.Instance.HideWatchAd();
            }
            else if (MainMenuUIManager.Instance.OptionsMenuOpen)
            {
                MainMenuUIManager.Instance.HideOptions();
            }
            else
            {
                if (pressedOnce && Time.time > timeOfClick)
                    pressedOnce = false;

                if (!pressedOnce)
                {
                    pressedOnce = true;
                    timeOfClick = Time.time + 2; // reset exiting if player has not pressed back button for more than two second
                    ShowToast("Press back again to exit");
                }
                else
                {
                    Back();
                }
            }
        }
    }

    #endregion


    #region Overridden Methods

    /// <summary>
    /// Overriden method, used to exit the game if the player hits the android back button.
    /// </summary>
    public override void Back()
    {
        SaveManager.SaveAll();
        System.GC.Collect();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    #endregion


    #region Update UI

    /// <summary>
    /// Display the amount of gold the player current has.
    /// </summary>
    public void UpdateGoldText()
    {
        textGold.text = (MainMenuUIManager.Instance.UnlimitedMoney) ? "Unlimited" : SaveManager.PlayerSaveData.player_gold.ToString("N0");
    }

    /// <summary>
    /// Displays "Tutorial" if this is the first time the player has played this game or "Classic Sail" otherwise.
    /// </summary>
    public void UpdatePlayButton()
    {
        buttonPlay.transform.GetChild(1).GetComponent<Text>().text = (SaveManager.PlayerSaveData.first_time) ? "Tutorial" : "Classic Sail";
    }

    /// <summary>
    /// Activates or deactivates the watch ad button.
    /// </summary>
    /// <param name="active"></param>
    public void UpdateWatchAdButton(bool active)
    {
        buttonWatchAd.interactable = active;
    }

    #endregion


    #region Button Logic

    /// <summary>
    /// Starts the tutorial if this is the first time the player has played this game, otherwise
    /// displays the level select menu.
    /// </summary>
    public void StartClassicMode()
    {
        AudioManager.Instance.PlayUIClick();

        if (SaveManager.PlayerSaveData.first_time)
            MainMenuUIManager.Instance.StartClassicMode(0);
        else
            MainMenuUIManager.Instance.SwitchToLevelSelect(gameObject);
    }

    /// <summary>
    /// Starts endless mode.
    /// </summary>
    public void StartEndlessMode()
    {
        AudioManager.Instance.PlayUIClick();
        MainMenuUIManager.Instance.StartEndlessMode();
    }

    /// <summary>
    /// Displays the upgrade menu.
    /// </summary>
    public void ShowUpgradeMenu()
    {
        AudioManager.Instance.PlayUIClick();
        MainMenuUIManager.Instance.SwitchToUpgrade(gameObject);
    }

    /// <summary>
    /// Displays the credits menu.
    /// </summary>
    public void ShowCreditsMenu()
    {
        AudioManager.Instance.PlayUIClick();
        MainMenuUIManager.Instance.SwitchToCredits(gameObject);
    }

    /// <summary>
    /// Displays or hides the options menu.
    /// </summary>
    public void ShowOptionsMenu()
    {
        AudioManager.Instance.PlayUIClick();
        MainMenuUIManager.Instance.SwitchToOptions();
    }

    /// <summary>
    /// Plays a video ad for the player to receive gold.
    /// </summary>
    public void ShowWatchAdMenu()
    {
        AudioManager.Instance.PlayUIClick();

        if (MainMenuUIManager.Instance.WatchAdMenuOpen)
            MainMenuUIManager.Instance.HideWatchAd();
        else
            MainMenuUIManager.Instance.SwitchToWatchAd();
    }

    #endregion


    #region Toasts

    /// <summary>
    /// Sets up the variables that will display a toast message on the bottom of the screen.
    /// </summary>
    void SetupToast()
    {
        toastClass = new AndroidJavaClass("android.widget.Toast");
        unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    }

    /// <summary>
    /// Displays a toast notification on the bottom of the screen.
    /// Reference Material: https://medium.com/@agrawalsuneet/native-android-in-unity-8ebfb42edfe8
    /// </summary>
    /// <param name="message"></param>
    void ShowToast(string message)
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
            toastClass.CallStatic<AndroidJavaObject>("makeText", currentActivity, message, toastClass.GetStatic<int>("LENGTH_SHORT")).Call("show");
        #else
            Debug.Log(message);
        #endif
    }

    #endregion
}
