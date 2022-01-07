/*****************************************************************************************************************
 - UIUtilities.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     General utility methods for UI's. Contains the code needed return to previous menu when pressing the back
     button on Android devices.
*****************************************************************************************************************/

using UnityEngine;

public class UIUtilities : MonoBehaviour, IUpdatableObject
{
    /// <summary>
    /// Add this game object to the UpdateManager.
    /// </summary>
    protected virtual void Start()
    {
        UpdateManager.obj.Add(this);
    }
    
    /// <summary>
    /// Returns to the start UI if the back button on the phone was pressed.
    /// </summary>
    public virtual void UpdateMe()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Back();
    }

    /// <summary>
    /// Only update if the game object is currently active.
    /// </summary>
    public virtual bool Active()
    {
        return gameObject.activeSelf;
    }

    /// <summary>
    /// Goes back to the start page, if in the main menu or to the in-game UI if currently
    /// in-game (or whichever page for that matter since this method can be overridden).
    /// </summary>
    public virtual void Back()
    {
        if (MainMenuUIManager.Instance)
            MainMenuUIManager.Instance.SwitchToStart(gameObject);
        else if (InGameUIManager.Instance)
            InGameUIManager.Instance.SwitchToInGame(gameObject);
    }
}