/*****************************************************************************************************************
 - Loader.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Used to load up the start menu from a cold boot. That's literally it.
*****************************************************************************************************************/

using UnityEngine;

public class Loader : MonoBehaviour
{
    [SerializeField] LoadingUI menuLoading;

    /// <summary>
    /// Loads the start scene (if the names weren't self-explanatory enough).
    /// </summary>
    void Start()
    {
        menuLoading.LoadScene("StartScene");
    }
}
