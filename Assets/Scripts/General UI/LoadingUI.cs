/*****************************************************************************************************************
 - LoadingUI.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Handles the loading screen, primarily used to load and display the loading progress of the next level.
*****************************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    #region Constants

    /// <summary>
    /// HP Icon = "Preparing the Ship"
    /// Armor Icon = "Undenting Armor"
    /// Repair Icon = "Finializing Upgrades"
    /// Cannon Icon = "Loading Cannons"
    /// Reload Icon = "Grabbing Cannonballs"
    /// Damage Icon = "Readying Explosions"
    /// Accuracy Icon = "Scouting for Scoundrels"
    /// </summary>
    readonly string[] LOADING_TEXT = { "Preparing the Ship", "Undenting Armor", "Finalizing Upgrades", "Loading Cannons", "Grabbing Cannonballs", "Readying Explosions", "Scouting for Scoundrels" };

    /// <summary>
    /// Parallel array to LOADING_TEXT.
    /// </summary>
    readonly Color[] LOADING_COLORS = { Color.red, Color.white, new Color(0.5754717f, 0.3943024f, 0.2090157f), Color.white, Color.white, Color.white, Color.white };

    #endregion


    #region Fields

    [SerializeField] Sprite[] loadingImages;
    [SerializeField] Image imageFill;
    [SerializeField] Text textLoading;

    #endregion


    #region Load Scene

    /// <summary>
    /// Load the scene named sceneName. Set loading screen to true to display the current progress of the loading, otherwise
    /// the current scene will be in play until the scene to be loaded is finished loading.
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="loadingScreen"></param>
    public void LoadScene(string sceneName, bool loadingScreen = true)
    {
        if (loadingScreen)
        {
            int element = Random.Range(0, LOADING_TEXT.Length);
            StartCoroutine(IEnumLoadScene(sceneName, LOADING_TEXT[element], loadingImages[element], LOADING_COLORS[element]));
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        }
    }

    /// <summary>
    /// Displays a loading progress icon to indicate when the level will finish loading.
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="loadingText"></param>
    /// <param name="loadingImage"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    System.Collections.IEnumerator IEnumLoadScene(string sceneName, string loadingText, Sprite loadingImage, Color color)
    {
        System.GC.Collect();

        // set fill image to the one specified by loadingImage
        imageFill.sprite = loadingImage;
        imageFill.type = Image.Type.Filled;
        imageFill.color = color;
        textLoading.text = loadingText;

        if (loadingImage.name.Equals("Reload_Icon"))
        {
            imageFill.fillMethod = Image.FillMethod.Radial360;
            imageFill.fillClockwise = false;
        }
        else
        {
            imageFill.fillMethod = Image.FillMethod.Vertical;
        }

        // load the level in the background, the current scene will still be in play until this loading is complete
        AsyncOperation newScene = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

        while (!newScene.isDone)
        {
            imageFill.fillAmount = newScene.progress;
            yield return null;
        }
    }

    #endregion
}
