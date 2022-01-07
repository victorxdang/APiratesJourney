/*****************************************************************************************************************
 - MapGenerator.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Parent class that will contain the common variables, constants and method shared between classic and 
     endless map generators.
*****************************************************************************************************************/


using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    #region Constants

    /// <summary>
    /// The amount of terrain blocks to spawn initially (for endless mode)
    /// </summary>
    protected const int TERRAIN_BLOCKS_TO_SPAWN = 3;

    /// <summary>
    /// The furthest left a ship/obstacle can spawn, with repsect to the player's point of view.
    /// </summary>
    protected const int X_MIN = -12;

    /// <summary>
    /// The furthest right a ship/obstacle can spawn, with repsect to the player's point of view.
    /// </summary>
    protected const int X_MAX = 60;

    /// <summary>
    /// The furthest back a ship/obstacle can spawn, with repsect to the player's point of view.
    /// </summary>
    protected const int Z_MIN = -30;

    /// <summary>
    /// The furthest up a ship/obstacle can spawn, with repsect to the player's point of view.
    /// </summary>
    protected const int Z_MAX = 30;

    /// <summary>
    /// The y-coordinate of the river block's spawn point.
    /// </summary>
    protected const float RIVER_Y = 1.25f;

    /// <summary>
    /// The z-coordinate of the river block's spawn point.
    /// </summary>
    protected const float RIVER_Z = 110;

    /// <summary>
    /// The y-coordinate of the ocean block's spawn point.
    /// </summary>
    protected const float OCEAN_Y = 0;

    /// <summary>
    /// The z-coorindate of the ocean block's spawn point.
    /// </summary>
    protected const float OCEAN_Z = -87;

    /// <summary>
    /// The length of a terrain block.
    /// </summary>
    protected const float TERRAIN_LENGTH = 100;

    /// <summary>
    /// The name of the object that will be the parent object for all spawned map objects 
    /// (terrain blocks, enemy ships and obstacles).
    /// </summary>
    protected const string MAP_HOLDER_NAME = "Map";

    #endregion


    #region Fields

    [SerializeField] protected Transform prefabEnemyShip;
    [SerializeField] protected Transform prefabOcean;
    [SerializeField] protected Transform[] prefabObstacles;
    [SerializeField] protected Transform[] prefabRivers;

    protected System.Random rand; //System.random is used over Random.Range because a seed value is used
    protected Quaternion riverRotation = Quaternion.Euler(0, -90, 0); // The river block's rotation (DO NOT CHANGE!)
    protected Quaternion obstacleRotation = Quaternion.Euler(0, 90, 0); // The obstacle block's rotation (DO NOT CHANGE!)

    #endregion


    #region Utilities

    /// <summary>
    /// Shuffles the array that was passed as a parameter using the Fisher-Yates shuffle.
    /// </summary>
    /// 
    /// <example>
    /// The shuffling is done as follows:
    /// 
    /// - A random number is first chosen from a range of 0 to the length of the list
    /// - i is then incremented by 1 and now a random number is chosen from 1 to the length of the list,
    ///   this will repeat for the length of the list.
    /// 
    /// Okay, this was disgustingly oversimplified, but look up the algorithm if you want to learn more.
    /// </example>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns> The shuffled list. </returns>
    protected void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            int rng = rand.Next(i, list.Count);
            T temp = list[rng];
            list[rng] = list[i];
            list[i] = temp;
        }
    }

    /// <summary>
    /// Determines if there is something overlapping the object at a specified spawn point with a 
    /// certain collider extent. A Physics.OverlapBox method is called to create a box where the obstacle
    /// is suppose to spawn. From there, it determines, based on the extent provided if there is anotehr object
    /// (either ship or obstacle) is within these bounds.
    /// </summary>
    /// <param name="spawnPoint"></param>
    /// <param name="extents"></param>
    /// <returns> True if an object is detected within this extent at this spawn point, false otherwise. </returns>
    protected bool IsOverlappingSomething(Vector3 spawnPoint, Vector3 extents)
    {
        // find any and all colliders within the area of this overlap box
        Collider[] colliders = Physics.OverlapBox(spawnPoint, extents);

        if (colliders.Length == 0)
            return false;

        // iterate through the array obtained above and return true if any objects were found within this overlap box
        foreach (Collider c in colliders)
        {           
            if (c.CompareTag("Enemy") || c.CompareTag("Obstacle"))
                return true;
        }

        return false;
    }

    #endregion
}
