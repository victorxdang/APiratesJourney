/*****************************************************************************************************************
 - PersistentData.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     This class will contain all of the data that is required to persist between scene transitions but is not
     to be saved to the device.
*****************************************************************************************************************/

public class PersistentData
{
    /// <summary>
    /// Set to false after loading into a level.
    /// </summary>
    public bool cold_boot = true;

    /// <summary>
    /// Has the player watch an ad in the previous playthrough?
    /// </summary>
    public bool watched_videoad = false;

    /// <summary>
    /// The number of playthroughs left before displaying an interstitial ad. A playthrough is counted
    /// whenever the game ends. It doesn't matter if the player spawned into the map and immediately
    /// sunk by an enemy, that will still count as a playthrough.
    /// </summary>
    public int games_until_ad = 5;


    /// <summary>
    /// The amount of gold obtained in a playthrough (used for endless mode). This is not the total 
    /// amount of gold that the player has (that is in SaveData -> player_gold).
    /// </summary>
    public int current_gold = 0;

    /// <summary>
    /// Current amount of enemies that the player has sunk (used for endless mode). This is not the 
    /// player's highest score (see SaveData -> high_score).
    /// </summary>
    public int current_score = 0; 

    /// <summary>
    /// The current game mode.
    /// </summary>
    public GameManager.GameMode game_mode = GameManager.GameMode.Classic;

    /// <summary>
    /// The current level. This is not the highest level the player has achieved
    /// (see SaveData -> highest_level).
    /// </summary>
    public int level = 0;
}
