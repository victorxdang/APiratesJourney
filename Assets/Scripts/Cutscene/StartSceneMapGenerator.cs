/*****************************************************************************************************************
 - StartSceneMapGenerator.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Spawns terrain blocks in the main menu.
*****************************************************************************************************************/

using UnityEngine;

public class StartSceneMapGenerator : MapGenerator
{
    public static StartSceneMapGenerator Instance { get; private set; }
    public static float LastSpawnPointX { get; private set; }

    [Tooltip("Set to true to spawn an ocean block at the index, otherwise a river block will spawn.")]
    [SerializeField] bool[] OceanBlock;


    /// <summary>
    /// Ensures that there is only one instance of this map generator in the scene.
    /// </summary>
    void Awake()
    {
        Instance = this;

        Transform mapObjectHolder = new GameObject(MAP_HOLDER_NAME).transform;
        mapObjectHolder.parent = transform;
        mapObjectHolder.gameObject.AddComponent<MapCutscene>();

        SpawnTerrain(mapObjectHolder);

        Time.timeScale = 1;
        System.GC.Collect();
    }

    /// <summary>
    /// Spawns the terrain blocks.
    /// </summary>
    void SpawnTerrain(Transform parent)
    {
        Transform block, temp;
        Quaternion rotation;
        Vector3 spawn = new Vector3(-25, 0, 0);

        for(int i = 0; i < OceanBlock.Length; i++)
        {
            block = (OceanBlock[i]) ? prefabOcean : prefabRivers[Random.Range(0, prefabRivers.Length)];

            if (OceanBlock[i])
            {
                spawn.y = OCEAN_Y;
                spawn.z = OCEAN_Z;
                rotation = Quaternion.identity;
            }
            else
            {
                spawn.y = RIVER_Y;
                spawn.z = RIVER_Z;
                rotation = riverRotation;
            }

            temp = Instantiate(block, spawn, rotation);
            temp.parent = parent;

            spawn.x += TERRAIN_LENGTH;
        }

        LastSpawnPointX = spawn.x - TERRAIN_LENGTH - (TERRAIN_LENGTH / 2.0f);
    }
}
