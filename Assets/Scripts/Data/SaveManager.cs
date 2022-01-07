/*****************************************************************************************************************
 - SaveManager.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     This class will handles saving and loading the player's data. The data that will persist throughout the 
     game will be handled in this class and maintained in the PlayerPersistentData field. Data that is going
     to be saved to the device is maintained in the PlayerSaveData field. Both of these fields are classes that
     hold the data and instaniated when the game starts. The file type that will be saved to the device is a 
     .json file. Although not secure by any means, it does offer the flexibility of adding more variables to 
     the save file if desired. For "more secure" means of saving data, add a save/load method using 
     System.BinaryFormatter.
*****************************************************************************************************************/

public static class SaveManager
{
    #region Data

    /// <summary>
    /// The name of the file and its type.
    /// </summary>
    const string LOCAL_SAVE_NAME = "Save_PiratesJourney.json";

    /// <summary>
    /// The path to save the player's data to.
    /// </summary>
    public static string SavePath { get { return UnityEngine.Application.persistentDataPath + "/" + LOCAL_SAVE_NAME; } }


    /// <summary>
    /// The data that was saved onto the cloud. The whole save file will be saved instead of just
    /// a variable or two.
    /// </summary>
    public static SaveData CloudSaveData { get; private set; }

    /// <summary>
    /// Holds all of the data that are going to be saved. This will be saved to the device and the 
    /// cloud.
    /// </summary>
    public static SaveData PlayerSaveData { get; private set; }

    /// <summary>
    /// Holds all of the data that will persist through scene transitions, will not be saved and will be instaniated
    /// everytime the game starts up again.
    /// </summary>
    public static PersistentData PlayerPersistentData { get; private set; }


    // Boolean to make sure this class doesn't initialize more than once.
    static bool initialized = false;

    #endregion


    #region Initialize

    /// <summary>
    /// Initialized the PlayerSaveData and PlayerPersistentData. PlayerPersisentData will be instaniated while 
    /// PlayerSaveData will be loaded from the device (if a save file exist). If a save file doesn't exist, then
    /// a new save file will be instanitated.
    /// </summary>
    /// <param name="forceInit"> Have SaveManager re-initialize </param>
    public static void Initialize(bool forceInit = false)
    {
        if (!initialized || forceInit)
        {
            initialized = true;

            // load new persistent data class
            PlayerPersistentData = new PersistentData();

            // load local data
            SaveData temp;
            PlayerSaveData = (Load(out temp, SavePath)) ? temp : new SaveData();
            PlayerSaveData.DeserializeDictionary();

            // load cloud data, will call OnCloudDataLoaded method when completed
            #if UNITY_ANDROID && !UNITY_EDITOR
                TryLoadCloudData();
            #endif
        }
    }

    /// <summary>
    /// Attempts to load the cloud data if it is not already loaded.
    /// </summary>
    public static void TryLoadCloudData()
    {
        // no further action needed since the callback after successfully loading from the cloud
        // will automatically call OnCloudDataLoaded() method.
        if (CloudSaveData == null && !GooglePlayGamesService.CloudLoad(GooglePlayGamesService.CLOUD_SAVE_FILENAME))
            CloudSaveData = new SaveData();
    }

    #endregion


    #region Check Data

    /// <summary>
    /// Converts the string data from the cloud into a SaveData class. Creates a new class of SaveData
    /// for CloudSaveData is the data was unsuccessfully loaded from the cloud.
    /// </summary>
    /// <param name="data"></param>
    public static void OnCloudDataLoaded(bool sucess, string data)
    {
        if (sucess)
        {
            CloudSaveData = UnityEngine.JsonUtility.FromJson<SaveData>(data);
            VerifyDataIntegrity();

            if (MainMenuUIManager.Instance)
                MainMenuUIManager.Instance.UpdateUI();
        }
        else
        {
            CloudSaveData = new SaveData();
        }
    }

    /// <summary>
    /// Checks to see if the file saved to the device is the same as the file saved onto the cloud.
    /// If not, this method will take appropriate actions to ensure that the data of both files 
    /// matches up.
    /// </summary>
    public static void VerifyDataIntegrity()
    {
        // check first time
        if (PlayerSaveData.first_time && !CloudSaveData.first_time)
            PlayerSaveData.first_time = CloudSaveData.first_time;
        else if (CloudSaveData.first_time && !PlayerSaveData.first_time)
            CloudSaveData.first_time = PlayerSaveData.first_time;

        /*// check use tilt
        if (!PlayerSaveData.use_tilt && CloudSaveData.use_tilt)
            PlayerSaveData.use_tilt = CloudSaveData.use_tilt;
        else if (!CloudSaveData.use_tilt && PlayerSaveData.use_tilt)
            CloudSaveData.use_tilt = PlayerSaveData.use_tilt;*/

        // check current player gold
        if ((PlayerSaveData.player_gold < CloudSaveData.player_gold && !CloudSaveData.upgraded_ship) ||
            (PlayerSaveData.player_gold > CloudSaveData.player_gold && CloudSaveData.upgraded_ship))
        {
            PlayerSaveData.player_gold = CloudSaveData.player_gold;
        }
        else if ((CloudSaveData.player_gold < PlayerSaveData.player_gold && !PlayerSaveData.upgraded_ship) ||
                 (CloudSaveData.player_gold > PlayerSaveData.player_gold && PlayerSaveData.upgraded_ship))
        {
            CloudSaveData.player_gold = PlayerSaveData.player_gold;
        }

        // check lifetime gold
        if (PlayerSaveData.lifetime_gold < CloudSaveData.lifetime_gold)
            PlayerSaveData.lifetime_gold = CloudSaveData.lifetime_gold;
        else if (CloudSaveData.lifetime_gold < PlayerSaveData.lifetime_gold)
            CloudSaveData.lifetime_gold = PlayerSaveData.lifetime_gold;

        // check lifetime ships sank
        if (PlayerSaveData.lifetime_ships_sank < CloudSaveData.lifetime_ships_sank)
            PlayerSaveData.lifetime_ships_sank = CloudSaveData.lifetime_ships_sank;
        else if (CloudSaveData.lifetime_ships_sank < PlayerSaveData.lifetime_ships_sank)
            CloudSaveData.lifetime_ships_sank = PlayerSaveData.lifetime_ships_sank;

        // check high score
        if (PlayerSaveData.high_score < CloudSaveData.high_score)
            PlayerSaveData.high_score = CloudSaveData.high_score;
        else if (CloudSaveData.high_score < PlayerSaveData.high_score)
            CloudSaveData.high_score = PlayerSaveData.high_score;

        // check highest level
        if (PlayerSaveData.highest_level < CloudSaveData.highest_level)
            PlayerSaveData.highest_level = CloudSaveData.highest_level;
        else if (CloudSaveData.highest_level < PlayerSaveData.highest_level)
            CloudSaveData.highest_level = PlayerSaveData.highest_level;


        // check upgrade HP amount
        if (PlayerSaveData.upgrade_amount_maxHP < CloudSaveData.upgrade_amount_maxHP)
            PlayerSaveData.upgrade_amount_maxHP = CloudSaveData.upgrade_amount_maxHP;
        else if (CloudSaveData.upgrade_amount_maxHP < PlayerSaveData.upgrade_amount_maxHP)
            CloudSaveData.upgrade_amount_maxHP = PlayerSaveData.upgrade_amount_maxHP;

        // check upgrade armor amount 
        if (PlayerSaveData.upgrade_amount_armor < CloudSaveData.upgrade_amount_armor)
            PlayerSaveData.upgrade_amount_armor = CloudSaveData.upgrade_amount_armor;
        else if (CloudSaveData.upgrade_amount_armor < PlayerSaveData.upgrade_amount_armor)
            CloudSaveData.upgrade_amount_armor = PlayerSaveData.upgrade_amount_armor;

        // check upgrade repair speed amount 
        if (PlayerSaveData.upgrade_amount_repair < CloudSaveData.upgrade_amount_repair)
            PlayerSaveData.upgrade_amount_repair = CloudSaveData.upgrade_amount_repair;
        else if (CloudSaveData.upgrade_amount_repair < PlayerSaveData.upgrade_amount_repair)
            CloudSaveData.upgrade_amount_repair = PlayerSaveData.upgrade_amount_repair;

        // check upgrade cannon amount 
        if (PlayerSaveData.upgrade_amount_cannon < CloudSaveData.upgrade_amount_cannon)
            PlayerSaveData.upgrade_amount_cannon = CloudSaveData.upgrade_amount_cannon;
        else if (CloudSaveData.upgrade_amount_cannon < PlayerSaveData.upgrade_amount_cannon)
            CloudSaveData.upgrade_amount_cannon = PlayerSaveData.upgrade_amount_cannon;

        // check upgrade reload time amount 
        if (PlayerSaveData.upgrade_amount_reload < CloudSaveData.upgrade_amount_reload)
            PlayerSaveData.upgrade_amount_reload = CloudSaveData.upgrade_amount_reload;
        else if (CloudSaveData.upgrade_amount_reload < PlayerSaveData.upgrade_amount_reload)
            CloudSaveData.upgrade_amount_reload = PlayerSaveData.upgrade_amount_reload;

        // check upgrade damage amount 
        if (PlayerSaveData.upgrade_amount_damage < CloudSaveData.upgrade_amount_damage)
            PlayerSaveData.upgrade_amount_damage = CloudSaveData.upgrade_amount_damage;
        else if (CloudSaveData.upgrade_amount_damage < PlayerSaveData.upgrade_amount_damage)
            CloudSaveData.upgrade_amount_damage = PlayerSaveData.upgrade_amount_damage;

        // check upgrade accuracy amount 
        if (PlayerSaveData.upgrade_amount_accuracy < CloudSaveData.upgrade_amount_accuracy)
            PlayerSaveData.upgrade_amount_accuracy = CloudSaveData.upgrade_amount_accuracy;
        else if (CloudSaveData.upgrade_amount_accuracy < PlayerSaveData.upgrade_amount_accuracy)
            CloudSaveData.upgrade_amount_accuracy = PlayerSaveData.upgrade_amount_accuracy;
    }

    #endregion


    #region Local Save/Load

    /// <summary>
    /// This method will save both the cloud data and the local data. Usually called when the 
    /// player loads from one level to another or when the player presses the home button to exit 
    /// the game.
    /// </summary>
    /// <returns> True if the save was a success, false if an error occured. </returns>
    public static void SaveAll()
    {
        PlayerSaveData.SerializeDictionary();
        Save(PlayerSaveData, SavePath);

        #if UNITY_ANDROID && !UNITY_EDITOR
            VerifyDataIntegrity();
            GooglePlayGamesService.CloudSave(UnityEngine.JsonUtility.ToJson(CloudSaveData));
        #endif
    }

    /// <summary>
    /// Saves the specifed file to the specified directory path. The file will be saved to the 
    /// device as a JSON file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="path"></param>
    /// <returns> True if the save was a success, false if an error occured. </returns>
    public static bool Save<T>(T data, string path)
    {
        try
        {
            System.IO.File.WriteAllText(path, UnityEngine.JsonUtility.ToJson(data));
            return true;
        }
        catch (System.Exception e)
        {
            return false;
        }
    }

    /// <summary>
    /// Loads the data from the device if one exists.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="path"></param>
    /// <returns> True if the file was found and returned, otherwise false if the file does not exists or an error was encountered. </returns>
    public static bool Load<T>(out T data, string path)
    {
        try
        {
            if (System.IO.File.Exists(path))
            {
                data = UnityEngine.JsonUtility.FromJson<T>(System.IO.File.ReadAllText(path));
                return true;
            }

            data = default(T);
            return false;
        }
        catch (System.Exception e)
        {
            data = default(T);
            return false;
        }
    }

    #endregion
}
