/*****************************************************************************************************************
 - TutorialUI.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     A tutorial UI for first time players. 
*****************************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    /// <summary>
    /// Enumerator to indicate which tutorial the player is currently in.
    /// </summary>
    enum Tutorial { Start, Tilt, Drag, Tap, TouchHere, WatchObstacles, End}

    [SerializeField] GameObject textBackground;
    [SerializeField] Text text;

    Tutorial currrentTutorialSet;

    /// <summary>
    /// Delays for 2 seconds before starting the tutorial.
    /// </summary>
    void Start()
    {
        textBackground.SetActive(false);
        GameManager.MoveMap = false;
        GameManager.EnemyShipTakeDamage = false;

        InGameUIManager.Instance.Wait(2, delegate
        {
            TutorialStart();
            textBackground.SetActive(true);
        });
    }

    /// <summary>
    /// When the current tutorial is the one where the player taps to shoot anywhere, this Update
    /// function will detect whenever the player does shoot and when they do, the text background 
    /// will disappear and reappear again when the enemy ship is in view.
    /// 
    /// When the current tutorial is the one where the player has to sink the enemy ship, then 
    /// the function will loop until the enemy is sunk. From there the next tutorial will display
    /// a short while after.
    /// </summary>
    void Update()
    {
        if (currrentTutorialSet == Tutorial.Tap)
        {
            #if UNITY_EDITOR
                if (Input.GetMouseButtonDown(1))
                {
            #else
                if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
            #endif
                    currrentTutorialSet = Tutorial.TouchHere;
                    textBackground.SetActive(false);
                    InGameUIManager.Instance.Wait(2, delegate 
                    {
                        TutorialSinkEnemyShip();
                    });
                }
        }
        else if (currrentTutorialSet == Tutorial.TouchHere && GameManager.NumberOfEnemyShips == 0)
        {
            text.text = "That'll teach him a lesson.";
            InGameUIManager.Instance.Wait(3, delegate 
            {
                currrentTutorialSet = Tutorial.WatchObstacles;
                textBackground.SetActive(false);
                InGameUIManager.Instance.Wait(4, TutorialWatchForObstacles);
            });
        }
        else if (currrentTutorialSet == Tutorial.WatchObstacles && !GameManager.IsPlaying())
        {
            TutorialCompleted();
        }
    }

    /// <summary>
    /// The beginning of the tutorials. This will display a message to greet the player.
    /// </summary>
    void TutorialStart()
    {
        text.text = "Ahoy Cap'n! Let's get yer familiar with th' basics!";
        InGameUIManager.Instance.Wait(3, TutorialMoveShip);
    }

    /// <summary>
    /// Displays how to move the ship using tilt. Drag 
    /// </summary>
    void TutorialMoveShip()
    {
        currrentTutorialSet = Tutorial.Tilt;

        text.text = "Tilt yer phone to move th' ship.";
        InGameUIManager.Instance.Wait(5, TutorialDragShip);
    }

    /// <summary>
    /// Not necessarily a tutorial, but does mentions that drag functionality is available.
    /// </summary>
    void TutorialDragShip()
    {
        text.text = "Drag can also be used, find it in th' Options Menu.";
        InGameUIManager.Instance.Wait(5, TutorialTapAnywhereToFire);
    }

    /// <summary>
    /// Tells the player to shoot anywhere. Actual checking for whether the player has shot or
    /// not will be in the Update function.
    /// </summary>
    void TutorialTapAnywhereToFire()
    {
        currrentTutorialSet = Tutorial.Tap;
        text.text = "Tap anywhere to fire yer cannons.";
    }

    /// <summary>
    /// Tells the player to sink the enemy ship.
    /// </summary>
    void TutorialSinkEnemyShip()
    {
        currrentTutorialSet = Tutorial.TouchHere;
        GameManager.MoveMap = true;

        InGameUIManager.Instance.Wait(3, delegate
        {
            textBackground.SetActive(true);
            text.text = "I spotted a scoundrel.\nBlast him out th' waters!";
            GameManager.EnemyShipTakeDamage = true;
        });
    }

    /// <summary>
    /// Informs the player to watch out for rocks in the waters.
    /// </summary>
    void TutorialWatchForObstacles()
    {
        currrentTutorialSet = Tutorial.WatchObstacles;

        textBackground.SetActive(true);
        text.text = "Avoid th' rocks, Cap'n!";

        InGameUIManager.Instance.Wait(3, delegate
        {
            textBackground.SetActive(false);
            InGameUIManager.Instance.Wait(3, TutorialReachIsland);
        });
    }

    /// <summary>
    /// Tells the player they have to reach the island.
    /// </summary>
    void TutorialReachIsland()
    {
        textBackground.SetActive(true);
        text.text = "Thar be land ahead! That be where we're headed!";
        InGameUIManager.Instance.Wait(3, delegate { textBackground.SetActive(false); });
    }

    /// <summary>
    /// The player has finished the tutorial.
    /// </summary>
    void TutorialCompleted()
    {
        currrentTutorialSet = Tutorial.End;

        textBackground.SetActive(true);
        text.text = "That's all it takes, Cap'n!\nNow go and get that booty!";
        InGameUIManager.Instance.Wait(3, delegate { gameObject.SetActive(false); });
    }
}
