/*****************************************************************************************************************
 - GameManager.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     This class will handle background tasks involving game logic. Primary game logic that this class handles
     is game over. Game over will calculate the score and gold that will be awarded to the player when the game
     ends. Ads will be displayed through methods from GameManager. Other logic, such as start and restart, only
     deal with the resetting or handling the player's data and nothing directly with the flow of execution of
     the game. Pause will set the time to 0 and effectively make the game stop until the time scale is set back
     to 1.
*****************************************************************************************************************/

using UnityEngine;

public static class GameManager
{
    #region Enumerator

    /// <summary>
    /// The game's primary game modes.
    /// </summary>
    public enum GameMode { Classic, Endless }

    #endregion


    #region Constants

    /// <summary>
    /// The speed of the map, how fast the map will move towards the player.
    /// </summary>
    public const float MAP_SPEED = 15;

    /// <summary>
    /// The distance to when the enemy ship's level will increase. This only
    /// takes place in the endless game mode.
    /// </summary>
    public const float DISTANCE_TO_INCREASE_LEVEL = 150;

    /// <summary>
    /// The name of the main menu scene.
    /// </summary>
    public const string MAIN_MENU_SCENE_NAME = "StartScene";

    /// <summary>
    /// The name of the scene to show the tutorial.
    /// </summary>
    public const string TUTORIAL_SCENE_NAME = "TutorialScene";

    /// <summary>
    /// The normal, level-based scene.
    /// </summary>
    public const string CLASSIC_SCENE_NAME = "ClassicScene";

    /// <summary>
    /// The endless scene, player will play until they are sunk.
    /// </summary>
    public const string ENDLESS_SCENE_NAME = "EndlessScene";

    #endregion


    #region Fields

    /// <summary>
    /// Determines whether the game has started or not.
    /// </summary>
    public static bool GameStart;

    /// <summary>
    /// Determines whether the game is currently paused or not.
    /// </summary>
    public static bool GamePaused;

    /// <summary>
    /// Determines whether the game is over (player has been sunk).
    /// </summary>
    public static bool GameOver;

    /// <summary>
    /// Determines whether the map should move.
    /// </summary>
    public static bool MoveMap;

    /// <summary>
    /// Shows that the player either has taken damage or has not, used for
    /// Not a Scratch to be Seen achievement.
    /// </summary>
    public static bool PlayerTakenDamage;

    /// <summary>
    /// Determines whether the enemy ships should take damage or not.
    /// </summary>
    public static bool EnemyShipTakeDamage;

    /// <summary>
    /// The total number of enemy ships that have spawned on the map by
    /// the map generator.
    /// </summary>
    public static int NumberOfEnemyShips;


    // the number of ships in the beginning of the game (classic mode)
    static int startingAmountofShip = 0;

    #endregion


    #region Initialize

    /// <summary>
    /// Since this is a static class, initialize will be in place of Awake. This method will be called
    /// in the Awake method within another script that is attached to an object and extends from Monobehaviour.
    /// This method will set all of the properties and fields to its default value.
    /// </summary>
    public static void Initialize()
    {
        GameStart = false;
        GamePaused = false;
        GameOver = false;
        MoveMap = true;
        PlayerTakenDamage = false;
        EnemyShipTakeDamage = true;
        NumberOfEnemyShips = 0;
        startingAmountofShip = 0;

        // this is necessary to ensure the game is not permanently paused, if the game is restarted from the pause menu
        Time.timeScale = 1;
    }

    #endregion


    #region Game Logic

    /// <summary>
    /// Begins gameplay, sets the player's UI text to either 0 or the amount of ships that were spawned
    /// based on the current game mode. If player is in endless game mode, the number of ships will 
    /// actually start at 0 and then count up everytime the player destroys an enemy ship. If in classic
    /// mode, the text will be the number of ships that spawned and will decrement every time the player
    /// destroys an enemy ship.
    /// </summary>
    public static void StartGame()
    {
        // if the game mode is classic, then generate a map based on the level selected and update the player's
        // UI apporpriately, otherwise, set the text on the player's UI to 0
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Equals(CLASSIC_SCENE_NAME))
        {
            ClassicMapGenerator.Instance.GenerateMapByLevel(SaveManager.PlayerPersistentData.level);
            NumberOfEnemyShips = ClassicMapGenerator.Instance.ShipsSpawned;
            startingAmountofShip = NumberOfEnemyShips;
            InGameUIManager.Instance.UpdateNumberOfShips(NumberOfEnemyShips);
        }
        else
        {
            NumberOfEnemyShips = SaveManager.PlayerPersistentData.current_score;
            InGameUIManager.Instance.UpdateNumberOfShips(NumberOfEnemyShips);
        }

        SaveManager.PlayerPersistentData.cold_boot = false;

        GameStart = true;
        Player.Instance.Calibrate();
        System.GC.Collect();
    }

    /// <summary>
    /// Displays a banner ad and effectively pauses the game. The game will pause by setting the time scale to 0.
    /// </summary>
    public static void PauseGame()
    {
        GamePaused = true;
        Time.timeScale = 0;
    }

    /// <summary>
    /// Hides the banner ad and resumes the game by setting time scale back to 1. Will also garbage collect any
    /// unreferenced variables in memory, if any.
    /// </summary>
    public static void ResumeGame()
    {
        GamePaused = false;
        Time.timeScale = 1;
        System.GC.Collect();
    }

    /// <summary>
    /// Resets the current score and gold of the player and determines if the restart is by watching an ad or clicking
    /// the restart button. If the player did watch an ad, the player's score will be retained. However, watching an ad
    /// can only occur in endless mode (or in the main menu, but this is independent of gameplay) and as such, score is 
    /// only retained in endless mode. Gold will always be resetted to 0 since the gold that the player has achieved in
    /// the last playthrough will be added into their gold funds.
    /// </summary>
    /// <param name="watchedAd"></param>
    public static void RestartGame(bool watchedAd)
    {
        SaveManager.PlayerPersistentData.watched_videoad = watchedAd;
        SaveManager.PlayerPersistentData.current_gold = 0;

        if (!watchedAd)
            SaveManager.PlayerPersistentData.current_score = 0;
        else
            SaveManager.PlayerSaveData.lifetime_ships_sank -= SaveManager.PlayerPersistentData.current_score;

        if (SaveManager.PlayerPersistentData.game_mode == GameMode.Endless && !watchedAd)
            SaveManager.PlayerPersistentData.level = 1;
    }

    #endregion


    #region Game Over

    /// <summary>
    /// Helper method for end game, will set the game over variable to true and wait for 3 seconds before calling
    /// the actual end game method.
    /// </summary>
    /// <param name="completedLevel"></param>
    public static void InvokeEndGame(bool completedLevel)
    {
        GameOver = true;
        InGameUIManager.Instance.Wait(3, delegate { EndGame(completedLevel); });
    }

    /// <summary>
    /// End game logic, this method will determine which mode the player is in and output the appropriate text on the 
    /// game over UI. 
    /// 
    /// If the player is in endless mode, the game over UI will display the player's score achieved in this 
    /// playthrough, the highest score the player has ever achieved and the amount of gold obtained. Score is based on the 
    /// amount of ships destroyed and gold is calculated within the SinkShip() method within the Enemy class. If a new record
    /// has been achieved, then the text will make sure you know that you have done so with the words: "NEW RECORD".
    /// 
    /// If the player is in classic mode, then the text will either display a congratulatory message and display the gold obtained
    /// or tell you to get good. 
    /// 
    /// After making you feel either good or bad about yourself, the method will then save the data to the local device.
    /// </summary>
    /// <param name="completedLevel"></param>
    static void EndGame(bool completedLevel)
    {
        string text = "", subTextTop = "", subTextBottom = "";
        SaveManager.PlayerSaveData.first_time = false;
        SaveManager.PlayerSaveData.lifetime_ships_sank += SaveManager.PlayerPersistentData.current_score;

        if (SaveManager.PlayerPersistentData.game_mode == GameMode.Endless)
        {
            bool newRecord = SaveManager.PlayerPersistentData.current_score > SaveManager.PlayerSaveData.high_score;

            // display new record if the current score is higher than the high score that is recorded to the device
            if (newRecord)
                SaveManager.PlayerSaveData.high_score = SaveManager.PlayerPersistentData.current_score;

            text = (newRecord) ? "NEW RECORD!" : "Ships Destroyed: " + SaveManager.PlayerPersistentData.current_score.ToString("N0");
            subTextTop = ((newRecord) ? "Ships Destroyed: " : "Best: ") + SaveManager.PlayerSaveData.high_score.ToString("N0");
            subTextBottom = SaveManager.PlayerPersistentData.current_gold.ToString();

            // add the player's gold to the save file
            SaveManager.PlayerSaveData.player_gold += SaveManager.PlayerPersistentData.current_gold;
            SaveManager.PlayerSaveData.lifetime_gold += SaveManager.PlayerPersistentData.current_gold;
        }
        else
        {
            if (completedLevel)
            {
                // add the player's gold to the save file
                int gold = (SaveManager.PlayerPersistentData.level <= SaveManager.PlayerSaveData.highest_level) ? 50 : Scaling.base_gold_per_level + SaveManager.PlayerPersistentData.current_gold;

                // if this level is the highest level the player just achieved, then record to the level to the file
                if (SaveManager.PlayerPersistentData.level > SaveManager.PlayerSaveData.highest_level)
                    SaveManager.PlayerSaveData.highest_level = SaveManager.PlayerPersistentData.level;

                // output gold and congratulatory text
                text = "Obtained the Booty!";
                subTextTop = gold.ToString();
                SaveManager.PlayerSaveData.player_gold += gold;
                SaveManager.PlayerSaveData.lifetime_gold += gold;
            }
            else
            {
                // display get good text
                text = "Lost to the Deep";
                subTextTop = "better luck next time";
            }
        }

        // update UI and do garbage collect
        InGameUIManager.Instance.SwitchToGameOver(completedLevel, text, subTextTop, subTextBottom);

        // save all files to device
        SaveManager.SaveAll();


        #region Achivements/Leaderboards

        if (SaveManager.PlayerPersistentData.level > 0)
        {
            if (completedLevel)
            {
                // unlock Give Me Th' Booty achievement if not tutorial level and is first time 
                // completing a level
                GooglePlayGamesService.UnlockAchievement(GPGSIds.achievement_give_me_th_booty);

                if (!PlayerTakenDamage)
                    GooglePlayGamesService.UnlockAchievement(GPGSIds.achievement_not_a_scratch_to_be_seen);
            }
            else
            {
                // unlock Scuttle Th' Ship! achievement if the player sinks for the first time
                GooglePlayGamesService.UnlockAchievement(GPGSIds.achievement_scuttle_th_ship);
            }
        }

        // unlock No Prey, No Pay achievement if the player did not take down any enemy ships
        if (SaveManager.PlayerPersistentData.level > 0 && NumberOfEnemyShips == startingAmountofShip && SaveManager.PlayerPersistentData.game_mode == GameMode.Classic)
            GooglePlayGamesService.UnlockAchievement(GPGSIds.achievement_no_prey_no_pay);

        // unlock Let's Yeet This Wheat achivement if the player has completed all of the levels
        // available for play
        if (SaveManager.PlayerPersistentData.level == Scaling.max_levels)
            GooglePlayGamesService.UnlockAchievement(GPGSIds.achievement_lets_yeet_this_wheat);

        // unlock A Boatload of Gold achievement if the player has achieved a lifetime gold of 
        // 100,000 gold or more
        if (SaveManager.PlayerSaveData.lifetime_gold > 100000)
            GooglePlayGamesService.UnlockAchievement(GPGSIds.achievement_a_boatload_of_gold);

        // update Richest Pirate leaderboard
        GooglePlayGamesService.AddScoreToLeaderboard(GPGSIds.leaderboard_richest_pirate, SaveManager.PlayerSaveData.lifetime_gold);

        // update Most Ships Sunk leaderboard
        GooglePlayGamesService.AddScoreToLeaderboard(GPGSIds.leaderboard_most_enemy_ships_sunk, SaveManager.PlayerSaveData.lifetime_ships_sank);

        #endregion
    }

    #endregion


    #region Utilities

    /// <summary>
    /// Is the game currently in play? 
    /// </summary>
    /// <returns> True if the game has started and the game is not yet over, false otherwise. </returns>
    public static bool IsPlaying()
    {
        return GameStart && !GameOver && !GamePaused;
    }

    /// <summary>
    /// Calls all methods that are subscribed to this event. The main method that should
    /// be subscribed to this action is the Calibrate method in the Player class.
    /// </summary>
    public static void Calibrate()
    {
        if (Player.Instance)
            Player.Instance.Calibrate();
        else if (PlayerCutscene.Instance)
            PlayerCutscene.Instance.Calibrate();

    }

    /// <summary>
    /// Calls all methods that are subscribed to this event. The main method that should
    /// be subscribed to this action is the SettingsChanged method in the Player class.
    /// </summary>
    public static void SettingsUpdated()
    {
        if (Player.Instance)
            Player.Instance.OnSettingsChanged();
        else if (PlayerCutscene.Instance)
            PlayerCutscene.Instance.OnSettingsChanged();
    }

    /// <summary>
    /// Called whenever an enemy ship is sunk by the player.
    /// </summary>
    public static void OnEnemyShipSunk()
    {
        SaveManager.PlayerPersistentData.current_gold += (int) (Scaling.base_gold_per_ship + (SaveManager.PlayerPersistentData.level * Scaling.ship_value_multiplier));
        SaveManager.PlayerPersistentData.current_score++;

        NumberOfEnemyShips += (SaveManager.PlayerPersistentData.game_mode == GameMode.Classic) ? -1 : 1;
        InGameUIManager.Instance.UpdateNumberOfShips(NumberOfEnemyShips);


        // unlock/increment achievements
        if (SaveManager.PlayerPersistentData.level > 0)
        {
            // increment Scourge of the Seven Seas achievement by 1 if not in tutorial level
            GooglePlayGamesService.IncrementAchivement(GPGSIds.achievement_scourge_of_the_seven_seas, 1);

            // unlock No Quarters Given achievement if the player sinks a ship for the first time 
            // and not in the tutorial level
            GooglePlayGamesService.UnlockAchievement(GPGSIds.achievement_no_quarters_given);
        }
    }

    #endregion
}
