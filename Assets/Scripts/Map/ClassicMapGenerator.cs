/*****************************************************************************************************************
 - ClassicMapGenerator.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     This class will generate the map based on level. For the map generator that generates a map dynamically,
     see EndlessMapGenerator.
*****************************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

public class ClassicMapGenerator : MapGenerator
{
    #region Fields

    public static ClassicMapGenerator Instance { get; protected set; }

    /// <summary>
    /// This is the amount of ships that actually spawned on the map, not the max amount of 
    /// ships that were to spawn on the map.
    /// </summary>
    public int ShipsSpawned { get; private set; }


    [SerializeField] protected Transform[] prefabIslands;

    // editor fields
    [Range(0, 200)]
    [SerializeField] int level;


    // cached fields
    int terrainBlocks, enemyShips, obstacles;
    float oceanPercentage;

    List<Transform> list_mapObjects;

    #endregion


    #region Built-In Methods

    /// <summary>
    /// Ensures that there is only one instance of this map generator in the scene.
    /// </summary>
    void Awake()
    {
        if (FindObjectsOfType<ClassicMapGenerator>().Length == 1)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion


    #region Public Methods

    /// <summary>
    /// Generates a map by level, instead of manually setting the amount of ships, terrain block,
    /// obstacles, etc.
    /// </summary>
    /// <param name="currentLevel"></param>
    public void GenerateMapByLevel(int currentLevel = -1)
    {
        if (currentLevel == -1)
            currentLevel = level;
        else
            level = currentLevel;

        rand = new System.Random(currentLevel);

        // calculate the number of ships, terrain blocks and obstalces that are to appear on the
        // map.  
        //
        // for ocean percentage, it is a randomly chosen value
        enemyShips = 1 + Mathf.CeilToInt(currentLevel / 10.0f);
        terrainBlocks = (currentLevel == 0) ? 2 : Mathf.CeilToInt(currentLevel / 10.0f);
        obstacles = rand.Next((int) Mathf.Round(terrainBlocks / 2.0f), terrainBlocks) * 2;
        oceanPercentage = (float) rand.NextDouble();

        GenerateMap();
    }

    /// <summary>
    /// Generates the map by first creating the parent object that will hold all of the map objects
    /// together and then calling the spawning methods.
    /// </summary>
    public void GenerateMap()
    {
        // find the map holder object and destroy if there is one
        ClearMap();

        list_mapObjects = new List<Transform>();

        // make a new map holder object to hold all of the spawned terrain blocks, ships and obstacles
        Transform mapObjectHolder = new GameObject(MAP_HOLDER_NAME).transform;
        mapObjectHolder.parent = transform;
        mapObjectHolder.gameObject.AddComponent<Map>();

        // map generating stuff here
        SpawnTerrain(mapObjectHolder);
        SpawnShip(mapObjectHolder);
        SpawnObstacles(mapObjectHolder);
    }

    /// <summary>
    /// Remove all map objects in the scene by removing the parent object that is holding all of 
    /// the map objects together.
    /// </summary>
    /// <param name="completeClear"></param>
    public void ClearMap(bool completeClear = false)
    {
        // delete the object containing all of the map blocks (if exists) and reset all field to 0
        if (transform.Find(MAP_HOLDER_NAME) != null)
        {
            DestroyImmediate(transform.Find(MAP_HOLDER_NAME).gameObject);

            if (completeClear)
            {
                level = 0;
                terrainBlocks = 0;
                enemyShips = 0;
                obstacles = 0;
                oceanPercentage = 0;

                GenerateMap();
            }
        }
    }

    #endregion


    #region Spawn Methods

    /// <summary>
    /// Spawn the terrain blocks. This method will spawn the amount of terrain blocks (either ocean
    /// or river) specified by terrainBlocks variable. More detail in each block of code.
    /// </summary>
    /// <param name="parent"></param>
    void SpawnTerrain(Transform parent)
    {
        Transform terrain;
        Vector3 terrainSpawn = new Vector3(-25, 0, 0);
        int amountOfOceanBlocks = (int) Mathf.Round(oceanPercentage * terrainBlocks);
        int amountOfRiverBlocks = terrainBlocks - amountOfOceanBlocks;

        // spawn ocean and river blocks, based on the percentage of ocean field
        for(int i = 0; i < terrainBlocks; i++)
        {
            if (amountOfOceanBlocks > 0)
            {
                list_mapObjects.Add(Instantiate(prefabOcean, Vector3.zero, Quaternion.identity));
                amountOfOceanBlocks--;
            }
            else if (amountOfRiverBlocks > 0)
            {
                list_mapObjects.Add(Instantiate(prefabRivers[rand.Next(0, prefabRivers.Length)], Vector3.zero, Quaternion.identity));
                amountOfRiverBlocks--;
            }
        }

        // shuffle the list of ocean/river blocks based on the seed field
        ShuffleList(list_mapObjects);

        // spawn the terrain blocks straight from the list in the order that it is in. The list
        // will be randomized from the previous line of code, so all that is needed is to grab
        // the terrain blocks in the order that they are in from the list
        for (int i = 0; i < terrainBlocks; i++)
        {
            terrain = list_mapObjects[i];

            if (terrain.CompareTag("River"))
            {
                terrainSpawn.y = RIVER_Y;
                terrainSpawn.z = RIVER_Z;
                terrain.transform.rotation = riverRotation;
            }
            else
            {
                terrainSpawn.y = OCEAN_Y;
                terrainSpawn.z = OCEAN_Z;
            }

            terrain.position = terrainSpawn;
            terrain.parent = parent;

            terrainSpawn.x += TERRAIN_LENGTH;
        }

        // spawn four extra ocean blocks for the island 
        for (int i = 0; i < 4; i++)
        {
            terrainSpawn.y = OCEAN_Y;
            terrainSpawn.z = OCEAN_Z;

            terrain = Instantiate(prefabOcean, terrainSpawn, Quaternion.identity);
            terrain.parent = parent;
            terrainSpawn.x += TERRAIN_LENGTH;
            list_mapObjects.Add(terrain);
        }

        // spawn island at the second to last ocean block
        terrainSpawn.x -= TERRAIN_LENGTH + 50;
        terrainSpawn.z = 0;
        terrain = Instantiate(prefabIslands[rand.Next(0, prefabIslands.Length)], terrainSpawn, Quaternion.identity);
        terrain.parent = parent;
    }

    /// <summary>
    /// Spawns an enemy ship.
    /// </summary>
    /// <param name="parent"></param>
    void SpawnShip(Transform parent)
    {
        Transform ship;
        Vector3 spawnPoint = Vector3.zero;
        int count = list_mapObjects.Count;
        float enemyShipsLeft = enemyShips;

        // don't spawn an enemy ship at the first terrain block (the player's starting block) and
        // at the last two blocks (reserved for the island)
        for (int i = 1; i < count - 2; i++)
        {
            if (enemyShipsLeft > 0)
            {
                // this is added to have some terrain blocks empty 
                if (i < count - 2 && enemyShipsLeft < count - enemyShipsLeft  - 2)
                {
                    // 10% chance to not spawn a ship at this terrain block
                    if (rand.Next(0, 10) == 0)
                        continue;
                }

                // calculate the position of the ship by taking the current index of the for loop
                // and multiplying it by the length of the terrain block
                if (level == 0)
                    spawnPoint = new Vector3(150, 1.6f, -30);
                else
                    spawnPoint = new Vector3(rand.Next(X_MIN + (i * 100), X_MAX + (i * 100)), 1.6f, rand.Next(Z_MIN, Z_MAX));

                ship = Instantiate(prefabEnemyShip, spawnPoint, Quaternion.identity);
                ship.parent = parent;
                ship.name += " " + i;

                enemyShipsLeft--; // amount of enemy ships that can spawn on the map
                ShipsSpawned++; // the amount of enemy ships that were actually spawned on the map
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// Spawns, at most, two obstacles in a terrain block.
    /// </summary>
    /// <param name="parent"></param>
    void SpawnObstacles(Transform parent)
    {
        Transform obstacle;
        Vector3 spawnPoint = Vector3.zero;
        int count = list_mapObjects.Count;
        int obstaclesLeft = obstacles;

        // don't spawn an obstacle at the first terrain block (the player's starting block) and
        // at the last two blocks (reserved for the island)
        for (int i = (level == 0) ? 3 : 1; i < count - 2; i++)
        {
            // spawn at most two obstacles at each terrain block
            for (int j = 0; j < 2; j++)
            {
                if (obstaclesLeft > 0)
                {
                    // this is added to potentially have some terrain blocks empty 
                    if (i < count - 2 && obstaclesLeft < count - obstaclesLeft - 2)
                    {
                        // 33% chance to not spawn an obstacle at this terrain block
                        if (rand.Next(0, 3) == 0)
                            continue;
                    }

                    // spawn the obstacle
                    obstacle = Instantiate(prefabObstacles[rand.Next(0, prefabObstacles.Length)], Vector3.zero, Quaternion.Euler(0, 90, 0));

                    // check if spawn point is in within a ship in the same terrain block 
                    do
                    {
                        spawnPoint = new Vector3(rand.Next(X_MIN + (i * 100), X_MAX + (i * 100)), 2, rand.Next(Z_MIN, Z_MAX));
                    } while (IsOverlappingSomething(spawnPoint, obstacle.GetComponent<Collider>().bounds.extents));

                    obstacle.transform.position = spawnPoint;
                    obstacle.parent = parent;
                    obstaclesLeft--;
                }
                else
                {
                    break;
                }
            }
        }
    }

    #endregion
}
