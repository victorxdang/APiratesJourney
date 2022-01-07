/*****************************************************************************************************************
 - UpdateManager.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     This class is a global update manager that will update any game object that extends from IUpdateManager.
     Using this implementation, there is only one update method (only in gameplay; in the start menu each UI
     still gets its' own Update method), meaning there will be less individual update methods to call and execute 
     and performance should be greatly improved.

     Classes that implement IUpdatableObject:
        - Ship (parent class for Player and Enemy classes)
        - Cannonball
        - Map
        - UIUtilities (parent class for all menu classes in InGameUIManager and MainMenuUIManager)
*****************************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class UpdateManager : MonoBehaviour
{
    // list to hold all of the updatable objectss
    public static List<IUpdatableObject> obj = new List<IUpdatableObject>();

    // cached variables
    int i, // for loop index 
        count; // number of elements in list


    /// <summary>
    /// Clears the list.
    /// </summary>
    void Awake()
    {
        obj.Clear();
    }

    /// <summary>
    /// Updates each game object that needs to update each frame and only updates if the game object is 
    /// active.
    /// </summary>
    void Update()
    {
        count = obj.Count;

        for (i = 0; i < count; i++)
        {
            try
            {
                // only update if the object is active, see each object that
                // implements IUpdatableObject to see it's definition of active
                if (obj[i].Active())
                    obj[i].UpdateMe();
            }
            catch (System.Exception e) // ignore any and all exceptions that are raised by this method, continue with execution
            {
                continue;
            }
        }
    }
}
