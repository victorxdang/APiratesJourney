/*****************************************************************************************************************
 - MapCutscene.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Map logic for the cutscene in the start menu.
*****************************************************************************************************************/

using UnityEngine;

public class MapCutscene : MonoBehaviour, IUpdatableObject
{
    Vector3 speed = Vector3.zero;


    /// <summary>
    /// Add this game object to the UpdateManager list.
    /// </summary>
    void Start()
    {
        UpdateManager.obj.Add(this);
    }

    /// <summary>
    /// Moves the map towards the player.
    /// </summary>
    public void UpdateMe()
    {
        if (transform.localPosition.x > -StartSceneMapGenerator.LastSpawnPointX)
        {
            speed.x = -GameManager.MAP_SPEED * Time.deltaTime;
            transform.localPosition += speed;
        }
        else
        {
            transform.localPosition = new Vector3(-25, 0, 0);
        }
    }

    /// <summary>
    /// Always update.
    /// </summary>
    /// <returns></returns>
    public bool Active()
    {
        return true;
    }
}
