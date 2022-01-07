/*****************************************************************************************************************
 - Scaling.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Contains all of the scaling/multiplier stats needed in order to upgrade the player's ship or 
     increase the stats of the enemy.
*****************************************************************************************************************/

using System.Collections.Generic;

public static class Scaling
{
    #region Levels

    /// <summary>
    /// The max amount levels available for the player to play. Set in increments of 20 
    /// (or strange things may happen).
    /// </summary>
    public const int max_levels = 120;

    #endregion


    #region Gold Stats

    /// <summary>
    /// The base amount of gold awarded for completing each level. This will be awarded on top of 
    /// what the player will get from destroying enemy ships within the level.
    /// </summary>
    public const int base_gold_per_level = 150;

    /// <summary>
    /// The base amount of gold that will be awarded for sinking a ship. This will added to the product
    /// of current level * ship_value_multiplier (below).
    /// </summary>
    public const int base_gold_per_ship = 50;

    /// <summary>
    /// Ship value multiplier (yes, very descriptive).
    /// </summary>
    public const float ship_value_multiplier = 10.3f;

    #endregion


    #region Dictionary Keys

    public const string max_hp = "maxHP";
    public const string armor = "armor";
    public const string repair_speed = "repair";
    public const string cannons = "cannons";
    public const string reload_time = "reload";
    public const string damage = "damage";
    public const string accuracy = "accuracy";

    #endregion


    #region Enemy Stats

    /// <summary>
    /// Enemy ships starting (base) stats.
    /// </summary>
    public static Dictionary<string, float> EnemyStatInitial = new Dictionary<string, float>()
    {
        { max_hp, 50 },
        { armor, 0 },
        { repair_speed, 0 },
        { cannons, 1 },
        { reload_time, 3 },
        { damage, 15 },
        { accuracy, 50 }
    };

    /// <summary>
    /// The value to increase by per level.
    /// </summary>
    public static Dictionary<string, float> EnemyStatMultiplier = new Dictionary<string, float>()
    {
        { max_hp, 2.12f },
        { armor, 0.0043f },
        { repair_speed, 0.11f },
        { cannons, 1 },
        { reload_time, -0.01f },
        { damage, 1.09f },
        { accuracy, 0.43f }
    };

    #endregion


    #region Player Stats

    /// <summary>
    /// The player's ship starting (base) stats.
    /// </summary>
    public static Dictionary<string, float> PlayerStatInitial = new Dictionary<string, float>()
    {
        { max_hp, 50 },
        { armor, 0 },
        { repair_speed, 1 },
        { cannons, 1 },
        { reload_time, 2 },
        { damage, 25 },
        { accuracy, 60 }
    };

    /// <summary>
    /// The value to increase by per level.
    /// </summary>
    public static Dictionary<string, float> PlayerStatMultiplier = new Dictionary<string, float>()
    {
        { max_hp, 37.41f },
        { armor, 0.09f },
        { repair_speed, 8.1f },
        { cannons, 2 },
        { reload_time, -0.175f },
        { damage, 17.76f },
        { accuracy, 4f }
    };

    #endregion


    #region Upgrade Stats

    /// <summary>
    /// The starting (base) cost to upgrade a particular stat.
    /// </summary>
    public static Dictionary<string, int> UpgradeCost = new Dictionary<string, int>()
    {
        { max_hp, 450 },
        { armor, 750 },
        { repair_speed, 475 },
        { cannons, 5000 },
        { reload_time, 550 },
        { damage, 650 },
        { accuracy, 600 }
    };

    /// <summary>
    /// The amount of gold multiplier.
    /// </summary>
    public static Dictionary<string, float> UpgradeCostMultiplier = new Dictionary<string, float>()
    {
        { max_hp, 0.47f },
        { armor, 0.73f },
        { repair_speed, 0.51f },
        { cannons, 0.5f },
        { reload_time, 0.55f },
        { damage, 0.52f },
        { accuracy, 0.61f }
    };

    /// <summary>
    /// The maximum amount of upgrades allowed for a stat.
    /// </summary>
    public static Dictionary<string, int> UpgradeAmountMax = new Dictionary<string, int>()
    {
        { max_hp, 10 },
        { armor, 10 },
        { repair_speed, 10 },
        { cannons, 2 },
        { reload_time, 10 },
        { damage, 10 },
        { accuracy, 10 }
    };

    #endregion
}
