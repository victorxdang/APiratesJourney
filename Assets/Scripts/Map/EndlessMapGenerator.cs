/*****************************************************************************************************************
 - ClassicMapGenerator.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     This class will generate the map dynamically. There will only be 5 sets of blocks that will spawn and
     these blocks will despawn and respawn again when needed. A set includes one terrain block, one ship and
     two obstacles.
*****************************************************************************************************************/

using UnityEngine;

public class EndlessMapGenerator : MapGenerator
{
    #region Fields

    public static EndlessMapGenerator Instance { get; protected set; }

    Transform mapObjectHolder, block, previousBlock, ship, obstacle;

    Vector3 terrainSpawn = new Vector3(-25, 0, 0), 
            spawnPoint = Vector3.zero;

    #endregion


    #region Built-In Methods

    /// <summary>
    /// Ensures that there is only one instance of this map generator in the scene.
    /// </summary>
    void Awake()
    {
        if (FindObjectsOfType<EndlessMapGenerator>().Length == 1)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Create a new parent object that will hold all of the map objects and then initially spawn
    /// 4 sets of blocks.
    /// </summary>
    void Start()
    {
        mapObjectHolder = new GameObject(MAP_HOLDER_NAME).transform;
        mapObjectHolder.parent = transform;

        // spawn a set
        for (int i = 0; i < TERRAIN_BLOCKS_TO_SPAWN; i++)
        {
            SpawnTerrainBlock(null, true);
            SpawnEnemyShip(previousBlock, i);
            SpawnObstacle(previousBlock, i);
        }
    }

    #endregion


    #region Spawn Objects

    /// <summary>
    /// Enqueues the block set and then respawns another block set in the queue.
    /// </summary>
    /// <param name="terrainBlock"></param>
    public void RespawnTerrainBlock(Transform terrainBlock)
    {
        SpawnTerrainBlock(terrainBlock);
        SpawnEnemyShip(terrainBlock, TERRAIN_BLOCKS_TO_SPAWN - 1);
        SpawnObstacle(terrainBlock, TERRAIN_BLOCKS_TO_SPAWN - 1);
    }

    /// <summary>
    /// Spawn the terrain block.
    /// </summary>
    /// <param name="block"> Set to null to spawn new terrain blocks </param>
    /// <param name="initialize"></param>
    void SpawnTerrainBlock(Transform block, bool initialize = false)
    {
        // spawn terrain block
        if (!block)
        {
            block = Instantiate(prefabOcean);
            block.gameObject.AddComponent<Map>();
            block.parent = mapObjectHolder;
        }


        if (block.CompareTag("Ocean")) // ocean block
        {
            terrainSpawn.y = OCEAN_Y;
            terrainSpawn.z = OCEAN_Z;
            block.rotation = Quaternion.identity;
        }
        else // river block
        {
            terrainSpawn.y = RIVER_Y;
            terrainSpawn.z = RIVER_Z;
            block.rotation = riverRotation;
        }

        // if initialize, spawn the blocks in the specified starting positions, otherwise, spawn
        // the blocks right after the previous block that spawned before this one.
        if (initialize)
        {
            block.position = terrainSpawn;
            terrainSpawn.x += TERRAIN_LENGTH;
        }
        else
        {
            terrainSpawn.x = previousBlock.position.x + TERRAIN_LENGTH - 0.3f;
            block.position = terrainSpawn;
        }

        block.gameObject.SetActive(true);
        previousBlock = block;
    }

    /// <summary>
    /// Spawn enemy ship. Also resets the animation as needed.
    /// </summary>
    /// <param name="block"> Set to null to spawn new terrain blocks </param>
    /// <param name="blockIndex"></param>
    void SpawnEnemyShip(Transform block, int blockIndex)
    {
        if (block.childCount > 0)
            ship = block.GetChild(0);

        // spawn ship
        if (!ship)
        {
            ship = Instantiate(prefabEnemyShip);
            ship.parent = block;
        }


        ship.GetChild(0).GetComponent<Enemy>().ResetAnimation();
        ship.position = new Vector3(Random.Range(X_MIN + (blockIndex * 100), X_MAX + (blockIndex * 100)), 1.6f, Random.Range(Z_MIN, Z_MAX));
        ship.gameObject.SetActive(blockIndex > 0);
        ship = null;
    }

    /// <summary>
    /// Spawns the obstacles. Ensures that both obstacles doesn't spawn inside one another or inside
    /// an enemy ship.
    /// </summary>
    /// <param name="block"> Set to null to spawn new terrain blocks </param>
    /// <param name="blockIndex"></param>
    void SpawnObstacle(Transform block, int blockIndex)
    {
        for (int j = 0; j < 2; j++)
        {
            if (block.childCount > j + 1)
                obstacle = block.GetChild(j + 1);

            // spawn obstacles
            if (!obstacle)
            {
                obstacle = Instantiate(prefabObstacles[Random.Range(0, prefabObstacles.Length)]);
                obstacle.parent = block;
            }


            // check if spawn point is in within a ship or obstacle in the same terrain block 
            do
            {
                spawnPoint = new Vector3(Random.Range(X_MIN + (blockIndex * 100), X_MAX + (blockIndex * 100)), 2, Random.Range(Z_MIN, Z_MAX));
            } while (IsOverlappingSomething(spawnPoint, obstacle.GetComponent<Collider>().bounds.extents));

            obstacle.transform.position = spawnPoint;
            obstacle.gameObject.SetActive(Random.Range(0, 5) != 0 && blockIndex > 0); // 20% chance to not set an obstacle active at this terrain block
            obstacle = null;
        }
    }

    #endregion
}
