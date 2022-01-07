/*****************************************************************************************************************
 - Ship.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Parent class to Enemy and Player class. This class holds all of the stats, methods and logic that are 
     shared between the two entities.
*****************************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour, IUpdatableObject
{
    #region Constants

    /// <summary>
    /// The height of the ship starting from the 0 coordinate.
    /// </summary>
    protected const float SHIP_HEIGHT_OFFSET = 3.6f;

    #endregion


    #region Ship Stats

    /// <summary>
    /// Maximum amount of hit points, should not be changed unless increasing stats.
    /// </summary>
    protected float MaxHitPoints;

    /// <summary>
    /// This hit point value is the one that will change and vary based on the damage done to
    /// this ship.
    /// </summary>
    protected float HitPoints;

    /// <summary>
    /// The amount of damage to ignore when taking damage.
    /// 
    /// Range: 0 to less than 1 (IMPORTANT: this value must be LESS THAN 1)
    /// </summary>
    protected float Armor;

    /// <summary>
    /// The amount of HP to regen per second.
    /// 
    /// Min value: 0 HP/sec
    /// </summary>
    protected float RepairSpeed;

    /// <summary>
    /// The amount of cannons that this ship currently has.
    /// 
    /// Range: 0 to 5 cannons
    /// </summary>
    protected int NumberOfCannons;

    /// <summary>
    /// The amount of damage that the cannonball fired from this ship will deal when it hits 
    /// another ship.
    /// </summary>
    protected float CannonDamage;

    /// <summary>
    /// The amount of time it takes to reload the cannon, in seconds.
    /// 
    /// Min value: 0.25 shot/second
    /// </summary>
    protected float ReloadTime;

    /// <summary>
    /// How accurate the cannons are when shooting.
    /// 
    /// Range: 0 to 100%
    /// </summary>
    protected float Accuracy;

    /// <summary>
    /// The damage dealt to the player's ship when the player hits an obstacle or another 
    /// ship from the back.
    /// </summary>
    protected float ImpactDamage;

    #endregion


    #region Fields

    [SerializeField] Cannonball prefabCannonball;

    protected bool isSinking;
    protected bool isReloading;
    protected float timeToNextShot;
    protected float timeToRepairShip;
    protected Animator animShip;

    // cached variables
    Cannonball tempCannonball;

    List<Transform> list_cannons = new List<Transform>(); // the cannons of the ship
    List<Transform> list_cannonBarrels = new List<Transform>(); // the spawn point of the cannonballs
    Queue<Cannonball> queue_cannonballs = new Queue<Cannonball>(); // the actual cannonballs

    #endregion


    #region Update Manager

    // these two are to be overriden in both Player and Enemy classes
    // NOTE: DO NOT CALL base FROM THE OVERRIDDEN METHODS!
    public virtual void UpdateMe() { }
    public virtual bool Active() { return false; }

    #endregion


    #region Built-In Methods

    /// <summary>
    /// Grabs the animator component from the game object.
    /// </summary>
    protected virtual void Start()
    {
        animShip = transform.parent.GetComponent<Animator>();
        ImpactDamage = 30 * SaveManager.PlayerPersistentData.level * 0.5f;

        UpdateManager.obj.Add(this);
    }

    /// <summary>
    /// When a cannonball collides with this ship (and if it is not the ship that fired this 
    /// colliding cannonball) then deal the amount of damage stored in the cannonball.
    /// </summary>
    /// <param name="collider"></param>
    protected virtual void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("CannonBall"))
        {
            tempCannonball = collider.GetComponent<Cannonball>();

            if (tempCannonball && !CompareTag(tempCannonball.FiredFrom))
            {
                TakeDamage(tempCannonball.Damage);
                tempCannonball.RequeueCannonball();

                if (AudioManager.Instance)
                    AudioManager.Instance.PlayCannonballHit();
            }
        }
    }

    #endregion


    #region Ship Methods

    /// <summary>
    /// Ensures that no stats are higher than what is stated below.
    /// </summary>
    protected void ClampStats()
    {
        NumberOfCannons = Mathf.Clamp(NumberOfCannons, 1, 5);
        MaxHitPoints = Mathf.Clamp(MaxHitPoints, 0, 1000000);
        HitPoints = Mathf.Clamp(HitPoints, 0, 1000000);
        Armor = Mathf.Clamp(Armor, 0, 0.99f);
        RepairSpeed = Mathf.Clamp(RepairSpeed, 0, 50000);
        ReloadTime = Mathf.Clamp(ReloadTime, 0.25f, 30);
        CannonDamage = Mathf.Clamp(CannonDamage, 0, 10000);
        Accuracy = Mathf.Clamp(Accuracy, 0, 100);
    }

    /// <summary>
    /// Takes in a positional vector in order to have all cannons point and fire
    /// at this direction.
    /// </summary>
    /// <param name="lookAtPoint"></param>
    protected void Shoot(Vector3 lookAtPoint)
    {
        if (queue_cannonballs.Count >= NumberOfCannons)
        {
            AudioManager.Instance.PlayCannonFiring();

            for (int i = 0; i < NumberOfCannons; i++)
            {
                list_cannons[i].LookAt(lookAtPoint); // make cannon look at position

                // accuracy effect, add or subtract a few degrees from the current rotation so that
                // it makes the cannons inaccurate (unless the accuracy is at 100%)
                list_cannons[i].localRotation = Quaternion.Euler(list_cannons[i].localRotation.eulerAngles.x,
                                                                 list_cannons[i].localRotation.eulerAngles.y + (Random.Range(0, (100 - Accuracy) / 5.0f) * (Random.Range(0, 100) > 50 ? -1 : 1)),
                                                                 list_cannons[i].localRotation.eulerAngles.z);

                // grab a cannonball from the ammo bay, set the appropriate stat and activate it
                tempCannonball = queue_cannonballs.Dequeue();
                tempCannonball.gameObject.SetActive(true);
                tempCannonball.Damage = CannonDamage;

                // set the position and rotation of the cannonballs to the position and rotation of
                // the cannons
                tempCannonball.transform.localPosition = list_cannonBarrels[i].position; 
                tempCannonball.transform.localRotation = list_cannonBarrels[i].rotation;
            }

            // reload the cannon, delay the ability to shoot until the cannon has
            // completely reloaded
            StartCoroutine(IEnumReloadTimer());
        }
    }

    /// <summary>
    /// Deactivates the cannonballs and re-queues it for next round of shots.
    /// </summary>
    /// <param name="cb"></param>
    public void EnqueueCannonball(Cannonball cb)
    {
        cb.gameObject.SetActive(false);
        queue_cannonballs.Enqueue(cb);
    }

    #endregion


    #region Utilities

    /// <summary>
    /// Activate the amount of cannons specified by NumberOfCannons (called by Enemy or Player class). This method
    /// will take into account existing cannons so that those cannons aren't added into the list again.
    /// </summary>
    protected void SetupCannons()
    {
        for (int i = list_cannons.Count; i < NumberOfCannons; i++)
        {
            list_cannons.Add(transform.GetChild(i));
            list_cannonBarrels.Add(list_cannons[i].transform.GetChild(0));
            list_cannons[i].gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Spawn the cannonballs. The amount of cannonballs to spawn is the time it takes to despawn
    /// a cannonball (3 seconds) divided by the ship's reload time. Take the previous quotient and
    /// multiply it by the number of cannons. That together will equal the amount of cannonballs
    /// to pool. This method will take into account existing cannonballs so that there won't be 
    /// duplicate of cannonballs currently active.
    /// </summary>
    protected void SpawnCannonballs()
    {
        int totalCannonballs = (Mathf.CeilToInt(3 / ReloadTime) * NumberOfCannons);

        for (int i = queue_cannonballs.Count; i < totalCannonballs; i++)
        {
            tempCannonball = Instantiate(prefabCannonball, InGameUIManager.Instance.ammoBay);
            tempCannonball.FiredFrom = tag;
            tempCannonball.Ship = this;

            EnqueueCannonball(tempCannonball);
        }
    }

    /// <summary>
    /// Reload timer, will update the Player's UI to show how long it will take until the player
    /// can shoot again.
    /// </summary>
    /// <returns></returns>
    protected System.Collections.IEnumerator IEnumReloadTimer()
    {
        isReloading = true;
        timeToNextShot = 0;

        // the condition is timeToNextShot < 1 rather than timeToNextShot < ReloadTime is because
        // the fill amount of an image is clamped from 0 to 1
        while (timeToNextShot < 1 && !GameManager.GameOver)
        {
            timeToNextShot += Time.deltaTime / ReloadTime;

            // if this ship is the player, then update the Player's UI
            if (CompareTag("Player"))
                InGameUIManager.Instance.UpdateReloadIcon(timeToNextShot);

            yield return null;
        }

        isReloading = false;
    }

    #endregion


    #region Virtual Methods

    /// <summary>
    /// Replensishes the amount of health specified by the RepairSpeed stat per second. Since this
    /// method is called within an update method, the RepairSpeed value is first multiplied by 
    /// Time.deltaTime and then added to the ship's HP value.
    /// </summary>
    protected virtual void RepairShip()
    {
        if (RepairSpeed > 0)
        {
            HitPoints += RepairSpeed * Time.deltaTime;
            HitPoints = Mathf.Clamp(HitPoints, 0, MaxHitPoints);
        }
    }

    /// <summary>
    /// Subtracts the amount of HP specified by parameter. If armor is present, then the amount of 
    /// armor is first multiplied by the damage and that product is subtracted from damage. The final
    /// damage value is then subtracted from the HP of the ship.
    /// </summary>
    /// 
    /// <example>
    /// HP = 100
    /// Armor = 0.4
    /// Incoming Damage = 50
    /// 
    /// Calculate armor mitigation: 
    /// 50 * 0.4 = 20
    /// 
    /// Calculate damage after armor calculation: 
    /// 50 - 20 = 30
    /// 
    /// Subtract from HP:
    /// 100 - 30 = 70
    /// </example>
    /// <param name="damage"></param>
    /// <param name="rammed"></param>
    protected virtual void TakeDamage(float damage, bool rammed = false)
    {
        if (!isSinking)
        {
            timeToRepairShip = Time.time + 5;
            HitPoints -= damage - (damage * Armor);

            if (HitPoints <= 0)
                SinkShip(rammed);
        }
    }

    /// <summary>
    /// Play the ship sinking animation.
    /// </summary>
    protected virtual void SinkShip(bool sunkByRamming = false)
    {
        isSinking = true;
        GetComponent<BoxCollider>().enabled = false;

        if (animShip)
            animShip.SetBool("Sinking", true);

        // unlock Ram Th' Scoundrels achivement
        if (sunkByRamming)
            GooglePlayGamesService.UnlockAchievement(GPGSIds.achievement_ram_th_scoundrels);
    }

    #endregion
}
