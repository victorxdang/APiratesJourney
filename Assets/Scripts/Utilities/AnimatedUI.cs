/*****************************************************************************************************************
 - AnimatedUI.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Parent class that contains the common methods shared between menus that have animated menus.
*****************************************************************************************************************/

using UnityEngine;

public class AnimatedUI : UIUtilities
{
    #region Fields

    [SerializeField] protected Animator anim;

    WaitForSecondsRealtime wait = new WaitForSecondsRealtime(0.25f);

    #endregion


    #region Animation Methods

    /// <summary>
    /// Change the value of an anim's parameter.
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="value"></param>
    public void SetAnimBool(string parameter, bool value)
    {
        if (anim)
            anim.SetBool(parameter, value);
    }

    /// <summary>
    /// Helper method to wait until the animation is completed and then execute action.
    /// </summary>
    /// <param name="action"></param>
    public void WaitForAnim(UnityEngine.Events.UnityAction action)
    {
        StartCoroutine(IEnumWaitForAnim(action));
    }

    /// <summary>
    /// Coroutine to wait for 1/4 second (the typical length of the menu animations) and then call
    /// action after waiting. 
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    System.Collections.IEnumerator IEnumWaitForAnim(UnityEngine.Events.UnityAction action)
    {
        yield return wait;
        action();
    }

    #endregion
}
