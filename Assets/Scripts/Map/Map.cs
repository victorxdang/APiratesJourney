/*****************************************************************************************************************
 - Map.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     This class will just have the map move towards the player. Besides that, there are no other functionality
     within this class.
*****************************************************************************************************************/

using UnityEngine;

public class Map : MonoBehaviour, IUpdatableObject
{
    // the direction and speed of the map
    Vector3 speed = Vector3.zero;


    /// <summary>
    /// Adds this object to UpdateManager.
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
        // if the terrain block has reach -125 in endless mode, then deactivate this terrain block and respawn it,
        // otherwise, keep moving the map
        if (transform.localPosition.x < -125 && SaveManager.PlayerPersistentData.game_mode == GameManager.GameMode.Endless)
        {
            EndlessMapGenerator.Instance.RespawnTerrainBlock(transform);
        }
        else
        {
            speed.x = -GameManager.MAP_SPEED * Time.deltaTime;
            transform.localPosition += speed;
        }
    }

    /// <summary>
    /// Checks to see if this game object is active.
    /// </summary>
    /// <returns></returns>
    public bool Active()
    {
        return GameManager.IsPlaying() && GameManager.MoveMap && InGameUIManager.Instance.MoveMap;
    }
}
