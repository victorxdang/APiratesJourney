/*****************************************************************************************************************
 - Cannonball.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Handles the movement and despawning of the cannonball.
*****************************************************************************************************************/

using UnityEngine;

public class Cannonball : MonoBehaviour, IUpdatableObject
{
    #region Fields

    /// <summary>
    /// How long the cannonball has been active. Will be resetted to 0 everytime it 
    /// is deactivated.
    /// </summary>
    public float Uptime;

    /// <summary>
    /// The damage that will be dealt to ships upon impact. This stat is dependent on
    /// the Damage stat in each ship.
    /// </summary>
    public float Damage;

    /// <summary>
    /// The tag of the ship that fired this cannonball.
    /// </summary>
    public string FiredFrom;

    /// <summary>
    /// The reference to the ship that fired this cannonball.
    /// </summary>
    public Ship Ship;

    #endregion


    #region Built-In Methods

    /// <summary>
    /// Add this object to the UpdateManager list.
    /// </summary>
    void Start()
    {
        UpdateManager.obj.Add(this);
    }

    #endregion


    #region Update Manager

    /// <summary>
    /// Have the cannonball move in the direction where the cannon that spawned this cannon was
    /// aiming at. After three seconds, if the cannonball has not hit anything, then it will invoke
    /// Action and execute any code that was subscribed to it.
    /// </summary>
    public void UpdateMe()
    {
        if (Uptime > 3)
        {
            RequeueCannonball();
        }
        else
        {
            // use Translate to have the cannonball travel in the direction where the cannon
            // that fired it was aiming at
            transform.Translate(0, 0, 50 * Time.deltaTime);

            // keep track of how long the cannonball has been travelling.
            Uptime += Time.deltaTime;
        }
    }

    /// <summary>
    /// Checks to see if this game object is active.
    /// </summary>
    /// <returns></returns>
    public bool Active()
    {
        return gameObject.activeSelf;
    }

    #endregion


    #region Utilites

    /// <summary>
    /// Enqueues this cannonball to the ship that fired it.
    /// </summary>
    public void RequeueCannonball()
    {
        Uptime = 0;
        Ship.EnqueueCannonball(this);
    }

    #endregion
}
