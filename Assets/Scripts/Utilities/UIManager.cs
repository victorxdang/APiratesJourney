/*****************************************************************************************************************
 - UIManager.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Parent class that contains all of the common methods that will be shared across all UI Managers.
*****************************************************************************************************************/

using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region Fields

    [SerializeField] protected OptionsUI menuOptions;
    [SerializeField] protected LoadingUI menuLoading;

    
    // cached fields
    public bool OptionsMenuOpen;

    #endregion


    #region Switch Menu

    /// <summary>
    /// Activate the target menu and deactivates all menus that were passed to currentMenus. Target and menus passed to
    /// currentMenus can be null and currentMenus can be empty as well.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="currentMenus"></param>
    protected void SwitchMenu(GameObject target, params GameObject[] currentMenus)
    {
        // Make sure there are actually menus within currentMenus to avoid errors.
        if (currentMenus.Length > 0)
        {
            // For each menu in currentMenus, make sure it is not null and deactivate it.
            foreach (GameObject menu in currentMenus)
                if (menu) { menu.SetActive(false); }
        }

        // Activate the target menu if it is not null.
        if (target)
            target.SetActive(true);
    }

    #endregion


    #region Wait

    /// <summary>
    /// Wait for the amount of time specified by duration. After waiting, the action will be called. This is a helper
    /// method to the coroutine IEnumWait.
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="action"></param>
    public void Wait(float duration, UnityEngine.Events.UnityAction action, bool scaledTime = false)
    {
        StartCoroutine(IEnumWait(duration, action, scaledTime));
    }

    /// <summary>
    /// Coroutine to delay execution of a certain action for a time specified by duration. Note that this coroutine
    /// uses WaitForSecondsRealtime instead of just WaitForSeconds so that it can still update even when the time
    /// scale is set to 0. This coroutine will still work, even if the game is paused.
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    System.Collections.IEnumerator IEnumWait(float duration, UnityEngine.Events.UnityAction action, bool scaledTime)
    {
        if (scaledTime)
            yield return new WaitForSeconds(duration);
        else
            yield return new WaitForSecondsRealtime(duration);

        action();
    }

    #endregion


    #region Show/Hide Options Menu

    /// <summary>
    /// Shows the options menu.
    /// </summary>
    public void ShowOptions()
    {
        menuOptions.ShowOptionsMenu();
    }

    /// <summary>
    /// Hides the options menu.
    /// </summary>
    public void HideOptions()
    {
        menuOptions.HideOptionsMenu();
    }

    #endregion


    #region Update Google Play Games Buttons

    /// <summary>
    /// Updates the interactability of the the achivements and leaderboards button. Also updates the
    /// text that is displayed on the sign-in button.
    /// </summary>
    public void UpdateGPGS()
    { 
        menuOptions.UpdateGPGSButtons(GooglePlayGamesService.IsSignedIn);
    }

    #endregion


    #region Built-In Methods (when game ends)

    /// <summary>
    /// When the game is paused (by pressing the home screen, actually the only way to exit the game),
    /// then the data will save to the cloud.
    /// </summary>
    /// <param name="pause"></param>
    void OnApplicationPause(bool pause)
    {
        SaveManager.SaveAll();
        System.GC.Collect();
    }

    /// <summary>
    /// When the game is paused (by pressing the home screen, actually the only way to exit the game),
    /// then the data will save to the cloud.
    /// </summary>
    /// <param name="pause"></param>
    void OnApplicationQuit()
    {
        SaveManager.SaveAll();
        System.GC.Collect();
    }

    #endregion
}
