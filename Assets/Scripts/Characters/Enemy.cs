/*****************************************************************************************************************
 - Enemy.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     The enemy of the game. These are the ships that the player has to sink.
*****************************************************************************************************************/

using UnityEngine;

public class Enemy : Ship
{
    #region Fields and Constants

    /// <summary>
    /// The distance threshold that an enemy ship must be within in order to fire at the player.
    /// </summary>
    const float DISTANCE_TO_FIRE = 75;

    float timeToNextPlayerCheck;
    Vector3 playerPos;
    Renderer meshRenderer;

    #endregion


    #region Built-In Methods

    /// <summary>
    /// Initializes the stats of the ship.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        meshRenderer = GetComponent<Renderer>();
        SetEnemyStats(SaveManager.PlayerPersistentData.level);
    }

    /// <summary>
    /// Checks if the player attempted to ram this ship.
    /// </summary>
    /// <param name="collider"></param>
    protected override void OnTriggerEnter(Collider collider)
    {
        base.OnTriggerEnter(collider);

        if (collider.CompareTag("Player"))
            TakeDamage(ImpactDamage, true);
    }

    #endregion


    #region Update Manager

    /// <summary>
    /// Check if in range of the player every half-second in order to improve script performance. This method will only check the
    /// player's position if it is currently visible in the camera. Both of these two logic should provide much better script
    /// execution time.
    /// </summary>
    public override void UpdateMe()
    {
        // regen the enemy ship's health if repair speed is greater than 0
        if (HitPoints < MaxHitPoints && Time.time > timeToRepairShip && RepairSpeed > 0)
            RepairShip();

        // if it is time for another check then check the player's current position, if the enemy
        // is in range, then fire at the player
        if (Time.time > timeToNextPlayerCheck)
        {
            playerPos = Player.Instance.transform.parent.position;

            if (InRangeOfPlayer(playerPos) && InGameUIManager.Instance.EnemyShouldFire && !isReloading)
            {
                timeToNextShot = Time.time + ReloadTime;
                playerPos.y += SHIP_HEIGHT_OFFSET;

                Shoot(playerPos);
            }

            // enemy checks the player's position every half-second is becaue the reload time
            // is expected to be 1 second at level 200 (which the game won't get to), therefore, we
            // can check for the player's position every so while to improve script performance.
            timeToNextPlayerCheck = Time.time + 0.5f;
        }
    }

    /// <summary>
    /// Checks to see if this game object is active and is visible to the camera.
    /// </summary>
    /// <returns></returns>
    public override bool Active()
    {
        return !isSinking && meshRenderer.isVisible && GameManager.IsPlaying();
    }

    #endregion


    #region Utilities

    /// <summary>
    /// Primarily used in endless mode, the animation is reset by setting the playback time of the
    /// animation to 0, setting the boolean value to false and setting the transform of the ship
    /// to it's original rotation and position. This method will also set the current HP to full
    /// HP and re-enable the ship's box collider.
    /// </summary>
    public void ResetAnimation()
    {
        if (animShip && isSinking)
        {
            animShip.SetBool("Sinking", false);

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(0, 90, 0);

            isSinking = false;
            GetComponent<BoxCollider>().enabled = true;
        }

        HitPoints = MaxHitPoints;
        SetEnemyStats(SaveManager.PlayerPersistentData.level);
    }

    /// <summary>
    /// Sets the enemy stats based on the current level.
    /// </summary>
    /// <param name="level"></param>
    public void SetEnemyStats(int level)
    {
        // tutorial stats
        if (level == 0)
        {
            MaxHitPoints = 1;
            HitPoints = MaxHitPoints;
            Armor = 0;
            RepairSpeed = 0;

            NumberOfCannons = 1;
            ReloadTime = 3;
            CannonDamage = 1;
            Accuracy = 50;
        }
        // normal stats
        else
        {
            MaxHitPoints = Scaling.EnemyStatInitial[Scaling.max_hp] + (level * Scaling.EnemyStatMultiplier[Scaling.max_hp]);
            HitPoints = MaxHitPoints;
            Armor = Scaling.EnemyStatInitial[Scaling.armor] + (level * Scaling.EnemyStatMultiplier[Scaling.armor]);
            RepairSpeed = Scaling.EnemyStatInitial[Scaling.repair_speed] + (level * Scaling.EnemyStatMultiplier[Scaling.repair_speed]);

            NumberOfCannons = (int) (Scaling.EnemyStatInitial[Scaling.cannons] + (2 * (level / 10)));
            ReloadTime = Scaling.EnemyStatInitial[Scaling.reload_time] + (level * Scaling.EnemyStatMultiplier[Scaling.reload_time]);
            CannonDamage = Scaling.EnemyStatInitial[Scaling.damage] + (level * Scaling.EnemyStatMultiplier[Scaling.damage]);
            Accuracy = Scaling.EnemyStatInitial[Scaling.accuracy] + (level * Scaling.EnemyStatMultiplier[Scaling.accuracy]);

            // makes sure that the values are within acceptable range
            ClampStats();
        }

        // SpawnCannonballs is located in this method because in endless mode, the level can increase, so the number of cannonballs must scale equally
        // with the level of the ship
        SpawnCannonballs();
        SetupCannons();
    }

    /// <summary>
    /// Checks whether this ship is within range of the specified position or not.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    bool InRangeOfPlayer(Vector3 position)
    {
        return DISTANCE_TO_FIRE > Mathf.Sqrt(((transform.position.x - position.x) * (transform.position.x - position.x)) +
                                             ((transform.position.y - position.y) * (transform.position.y - position.y)) +
                                             ((transform.position.z - position.z) * (transform.position.z - position.z)));
    }

    #endregion


    #region Overwritten Inherited Methods

    /// <summary>
    /// Only take damage if the condition is true. This only applies to the tutorial level, every other instance will have
    /// the enemy ship take damage.
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="rammed"></param>
    protected override void TakeDamage(float damage, bool rammed = false)
    {
        if (GameManager.EnemyShipTakeDamage)
            base.TakeDamage(damage, rammed);
    }

    /// <summary>
    /// Calculate the gold value of sinking this ship and then add it to the player CURRENT gold 
    /// counter. GameManager will handle actually saving the current gold amount to the save file.
    /// Also updates the amount of enemy ships destroyed (endless) or the amount of ships left for
    /// the player to destroy (classic).
    /// </summary>
    protected override void SinkShip(bool sunkByRamming = false)
    {
        base.SinkShip(sunkByRamming);
        GameManager.OnEnemyShipSunk();
    }

    #endregion
}
