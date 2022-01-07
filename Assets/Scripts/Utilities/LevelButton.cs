/*****************************************************************************************************************
 - LevelButton.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     A utility class for the level select buttons. Each button will hold a particular level and once clicked,
     the button will call the StartClassicMode method in the MainMenuUIManager in order to start the game
     with the level it displayed.
*****************************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    /// <summary>
    /// Sets the button's text to the level specified. It will also display the sword icon in the background of
    /// the button if the player has already completed the specified level. For any levels above the highest level
    /// the player has completed, then the button will not be interactable.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="highestLevel"></param>
    public void SetupButton(int level, int highestLevel)
    {
        Button button = GetComponent<Button>();
        button.interactable = (level <= highestLevel); // set to not interactable if level is greater than highest level
        transform.GetChild(0).gameObject.SetActive(level < highestLevel); // background sword icon
        transform.GetChild(1).GetComponent<Text>().text = level.ToString(); // text component

        if (level == highestLevel)
            transform.GetChild(1).GetComponent<Text>().color = new Color(1, 1, 1);

        if (button.interactable)
        {
            button.onClick.AddListener(delegate
            {
                AudioManager.Instance.PlayUIClick();
                MainMenuUIManager.Instance.StartClassicMode(level);
            });
        }
    }
}
