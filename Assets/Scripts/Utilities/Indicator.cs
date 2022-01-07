/*****************************************************************************************************************
 - Indicator.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     The page indicator on the bottom of the level select menu. Used only for aesthetic reasons and to help the
     player know which page they are on.
*****************************************************************************************************************/

using UnityEngine;

public class Indicator : MonoBehaviour
{
    #region Fields

    /// <summary>
    /// How fast the dot will scale to 0.
    /// </summary>
    public float TransitionSpeed { get; set; }


    // cache fields
    RectTransform dot;

    #endregion


    #region Built-In Methods

    /// <summary>
    /// Grabs the dot game object.
    /// </summary>
    void Awake()
    {
        dot = transform.GetChild(1).GetComponent<RectTransform>();
    }

    #endregion


    #region Show/Hide Dot

    /// <summary>
    /// Updates the dot to either show the dot (1 scale on all axis) or hide it (0 scale on x and y axis).
    /// </summary>
    /// <param name="show"></param>
    public void UpdateDot(bool show)
    {
        dot.localScale = (show) ? new Vector3(1, 1, 1) : new Vector3(0, 0, 1);
    }

    /// <summary>
    /// Plays an animation to show the dot expanding within the circle.
    /// </summary>
    public void ShowDot()
    {
        StartCoroutine(IEnumTransitionDot(true));
    }

    /// <summary>
    /// Plays an animation to show the dot contracting within the circle.
    /// </summary>
    public void HideDot()
    {
        StartCoroutine(IEnumTransitionDot(false));
    }

    /// <summary>
    /// Will either expand or contract the dot based on the provided boolean. This coroutine is updated by
    /// Time.unsclaedDeltaTime instead of just Time.deltaTime in order to be independent from the game's 
    /// time scale.
    /// </summary>
    /// <param name="transitionIn"></param>
    /// <returns></returns>
    System.Collections.IEnumerator IEnumTransitionDot(bool transitionIn)
    {
        float time = 0;
        Vector3 currentScale = dot.localScale,
                newScale = (transitionIn) ? new Vector3(1, 1, 1) : new Vector3(0, 0, 1);

        while (time <= 1)
        {
            dot.localScale = Vector3.Lerp(currentScale, newScale, time);

            time += Time.unscaledDeltaTime * TransitionSpeed;
            yield return null;
        }

        dot.localScale = newScale;
    }

    #endregion
}
