/*****************************************************************************************************************
 - SaveData.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Contains all of the data that will be saved to the device.
*****************************************************************************************************************/

public class SaveData
{
    /// <summary>
    /// Is this the first time the player has played this game?
    /// 
    /// default value = true
    /// </summary>
    public bool first_time = true;

    /// <summary>
    /// Determines if the player is using tilt or not. If not, then the player is using drag.
    /// </summary>
    public bool use_tilt = true;

    /// <summary>
    /// Determines if the player has upgraded their ship. Used to check whether or not the gold
    /// from the save is correct or not.
    /// </summary>
    public bool upgraded_ship = false;


    /// <summary>
    /// The total amount of gold that the player currently has and can use for ship upgrades and such.
    /// </summary>
    public int player_gold = 0;

    /// <summary>
    /// The total amount of gold the player has obtained throughout their time playing this game.
    /// </summary>
    public int lifetime_gold = 0;

    /// <summary>
    /// The total amount of enemy ships that the player has sunk throughout their time playing
    /// this game.
    /// </summary>
    public int lifetime_ships_sank = 0;

    /// <summary>
    /// The most enemies that the player has ever sunk (used for endless mode).
    /// </summary>
    public int high_score = 0;

    /// <summary>
    /// The highest level that the palyer has completed (used for classic game mode, default value 
    /// is 0).
    /// </summary>
    public int highest_level = 0;


    #region Dictionary Helper Variables/Methods

    public int upgrade_amount_maxHP = 0;
    public int upgrade_amount_armor = 0;
    public int upgrade_amount_repair = 0;

    public int upgrade_amount_cannon = 0;
    public int upgrade_amount_reload = 0;
    public int upgrade_amount_damage = 0;
    public int upgrade_amount_accuracy = 0;


    /// <summary>
    /// The amount of upgrades that the player currently has invested into each upgrade. These values
    /// are in a dictionary so that I don't have to write a whole load of repetitive code in the
    /// UpgradeUI. 
    /// 
    /// In general, every stat will have, at most, 10 upgrades, with the exception of cannons, which
    /// will only have 2 upgrades (see Scaling -> UpgradeAmountMax for details).
    /// </summary>
    public System.Collections.Generic.Dictionary<string, int> UpgradeAmount = new System.Collections.Generic.Dictionary<string, int>()
    {
        { Scaling.max_hp, 0 },
        { Scaling.armor, 0 },
        { Scaling.repair_speed, 0 },
        { Scaling.cannons, 0 },
        { Scaling.reload_time, 0 },
        { Scaling.damage, 0 },
        { Scaling.accuracy, 0 }
    };


    /// <summary>
    /// Called right before the data is saved to the device. This is implemented because Dictionary
    /// does not serialize properly so this is done in order to save the upgrade values.
    /// </summary>
    public void SerializeDictionary()
    {
        upgrade_amount_maxHP = UpgradeAmount[Scaling.max_hp];
        upgrade_amount_armor = UpgradeAmount[Scaling.armor];
        upgrade_amount_repair = UpgradeAmount[Scaling.repair_speed];

        upgrade_amount_cannon = UpgradeAmount[Scaling.cannons];
        upgrade_amount_reload = UpgradeAmount[Scaling.reload_time];
        upgrade_amount_damage = UpgradeAmount[Scaling.damage];
        upgrade_amount_accuracy = UpgradeAmount[Scaling.accuracy];
    }

    /// <summary>
    /// Retrieve the saved upgrade values and set them to the appropriate element in the dictionary.
    /// </summary>
    public void DeserializeDictionary()
    {
        UpgradeAmount[Scaling.max_hp] = upgrade_amount_maxHP;
        UpgradeAmount[Scaling.armor] = upgrade_amount_armor;
        UpgradeAmount[Scaling.repair_speed] = upgrade_amount_repair;

        UpgradeAmount[Scaling.cannons] = upgrade_amount_cannon;
        UpgradeAmount[Scaling.reload_time] = upgrade_amount_reload;
        UpgradeAmount[Scaling.damage] = upgrade_amount_damage;
        UpgradeAmount[Scaling.accuracy] = upgrade_amount_accuracy;
    }

    #endregion


    #region Compare Data

    /// <summary>
    /// Overwritten built-in Equals method. Returns true if one of the fields within this data 
    /// file is less than the corresponding field from the file being passed to this method, 
    /// false otherwise.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns> Ture if one of the fields is less than corresponding field in obj, false otherwise </returns>
    public bool LessThan(object obj)
    {
        if (obj == null || !(obj is SaveData))
        {
            return false;
        }
        else
        {
            SaveData temp = obj as SaveData;

            return (player_gold < temp.player_gold ||
                    high_score < temp.high_score ||
                    highest_level < temp.highest_level ||
                    upgrade_amount_maxHP < temp.upgrade_amount_maxHP ||
                    upgrade_amount_armor < temp.upgrade_amount_armor ||
                    upgrade_amount_repair < temp.upgrade_amount_repair ||
                    upgrade_amount_cannon < temp.upgrade_amount_cannon ||
                    upgrade_amount_reload < temp.upgrade_amount_reload ||
                    upgrade_amount_damage < temp.upgrade_amount_damage ||
                    upgrade_amount_accuracy < temp.upgrade_amount_accuracy);
        }
    }

    #endregion
}
