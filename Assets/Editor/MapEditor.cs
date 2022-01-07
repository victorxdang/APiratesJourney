/*****************************************************************************************************************
 - MapEditor.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Editor script to spawn the map in the inspector rather than having to play the game.
*****************************************************************************************************************/

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ClassicMapGenerator))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Generate the map if the level has been changed or the generate map button is clicked
        if (DrawDefaultInspector() || GUILayout.Button("Generate Map"))
        {
            (target as ClassicMapGenerator).GenerateMapByLevel();
        }

        // clear all objects in the sceen if the clear map button is clicked
        if (GUILayout.Button("Clear Map"))
        {
            (target as ClassicMapGenerator).ClearMap(true);
        }
    }
}
