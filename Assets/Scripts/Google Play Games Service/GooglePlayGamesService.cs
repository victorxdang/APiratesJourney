/*****************************************************************************************************************
 - GooglePlayGameServices.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.2.10f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     This class handles all work that will be required to have a Google Play Games Service (GPGS) leaderboard, 
     achievements and save games to the cloud.
*****************************************************************************************************************/

using System;
using System.Text;

using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;

public static class GooglePlayGamesService
{
    #region Constants

    // come up with and add game name
    public const string CLOUD_SAVE_FILENAME = "CloudSaveFile_PiratesJourney";

    #endregion


    #region Fields

    public static bool IsSignedIn { get; private set; }

    static bool enableGPGS = true;
    static bool initialized = false;
    static ISavedGameMetadata CurrentGameMetaData;

    #endregion


    #region Initialize

    /// <summary>
    /// Attempts a silent sign-in when starting up the game, otherwise it will disable
    /// the achivement and leaderboard buttons.
    /// </summary>
    /// <param name="enable"></param>
    public static void Initialize(bool enable)
    {
        enableGPGS = enable;

        if (enable)
        {
            if (!initialized)
            {
                initialized = true;
                GPGSConfiguration(true);
            }
        }
        else
        {
            IsSignedIn = false;
            UpdateAfterSignIn();
        }
    }

    #endregion


    #region Sign-In/Setup For Google Play Games Service

    /// <summary>
    /// Called whenever the users presses the sign-in button from either the start menu or the settings menu.
    /// </summary>
    public static void ShowSignInUI()
    {
        if (enableGPGS)
        {
            if (!PlayGamesPlatform.Instance.localUser.authenticated)
            {
                PlayGamesPlatform.Instance.Authenticate(SignInCallback, false);
            }
            else
            {
                PlayGamesPlatform.Instance.SignOut();
                IsSignedIn = false;
                UpdateAfterSignIn();
            }
        }
    }

    /// <summary>
    /// Activate Google Play Games Service to allow signing into Google Play to access achievements, leaderboards
    /// and to save the player's progress.
    /// </summary>
    static void GPGSConfiguration(bool silent)
    {
        // GPGS configurations
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().EnableSavedGames().Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.Activate();

        // sign-in
        PlayGamesPlatform.Instance.Authenticate(SignInCallback, silent);
    }

    /// <summary>
    /// Callback function for when the sign-in button is clicked, or when the game automatically
    /// signs in the player.
    /// </summary>
    /// <param name="success"></param>
    static void SignInCallback(bool success)
    {
        IsSignedIn = success;
        CloudLoad(CLOUD_SAVE_FILENAME);
        UpdateAfterSignIn();
    }

    /// <summary>
    /// Updates the text, buttons, etc. of whichever menu is currently active.
    /// </summary>
    static void UpdateAfterSignIn()
    {
        if (MainMenuUIManager.Instance)
            MainMenuUIManager.Instance.UpdateGPGS();
        else
            InGameUIManager.Instance.UpdateGPGS();
    }

    #endregion


    #region Achievement

    /// <summary>
    /// Displays Google Play Platform achievement UI.
    /// </summary>
    public static void ShowAchievementsUI()
    {
        if (PlayGamesPlatform.Instance.localUser.authenticated)
            PlayGamesPlatform.Instance.ShowAchievementsUI();
    }

    /// <summary>
    /// Unlocks an achievement with the given id. Do not use this method for incremental achievements.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool UnlockAchievement(string id)
    {
        if (PlayGamesPlatform.Instance.localUser.authenticated && enableGPGS)
        {
            bool unlocked = false;
            PlayGamesPlatform.Instance.ReportProgress(id, 100, success => { unlocked = success; });
            return unlocked;
        }

        return false;
    }

    /// <summary>
    /// Increments an achievement with the given id by a certain number of steps. Exclusive to GPGS.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="steps"></param>
    /// <returns></returns>
    public static bool IncrementAchivement(string id, int steps)
    {
        if (PlayGamesPlatform.Instance.localUser.authenticated && enableGPGS)
        {
            bool incremented = false;
            PlayGamesPlatform.Instance.IncrementAchievement(id, steps, success => { incremented = success; });
            return incremented;
        }

        return false;
    }

    #endregion


    #region Leaderboard

    /// <summary>
    /// Displays Google Play Platform leaderboard UI.
    /// </summary>
    public static void ShowLeaderboardsUI()
    {
        if (PlayGamesPlatform.Instance.localUser.authenticated)
            PlayGamesPlatform.Instance.ShowLeaderboardUI();
    }

    /// <summary>
    /// Reports a score to the Google Play Platform leaderboard with the given id leaderboardID.
    /// </summary>
    /// <param name="leaderboardID"></param>
    /// <param name="score"></param>
    /// <returns></returns>
    public static bool AddScoreToLeaderboard(string leaderboardID, long score)
    {
        if (PlayGamesPlatform.Instance.localUser.authenticated && enableGPGS)
        {
            bool added = false;
            PlayGamesPlatform.Instance.ReportScore(score, leaderboardID, success => { added = success; });
            return added;
        }

        return false;
    }

    #endregion


    #region Cloud Save

    /// <summary>
    /// Saves the reported score to the cloud. This is a helper function for CloudSave() below which will actually
    /// save the data. This just takes a JSON, converted into a string and then into an array of bytes.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static bool CloudSave(string data)
    {
        try
        {
            if (PlayGamesPlatform.Instance.localUser.authenticated)
                return CloudSave(CurrentGameMetaData, Encoding.UTF8.GetBytes(data));

            return false;
        }
        catch(Exception e)
        {
            return false;
        }
    }

    /// <summary>
    /// Saves the data to the Google Play cloud. Keeps record of the time and date of when the data was saved.
    /// </summary>
    /// <param name="game"></param>
    /// <param name="savedData"></param>
    /// <returns></returns>
    public static bool CloudSave(ISavedGameMetadata game, byte[] savedData)
    {
        try
        {
            // update the file to include the date and time that it was saved
            SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder()
                .WithUpdatedPlayedTime(TimeSpan.FromMinutes(game.TotalTimePlayed.Minutes + 1))
                .WithUpdatedDescription("Timestamp: " + DateTime.Now);

            SavedGameMetadataUpdate updatedMetadata = builder.Build();

            // commit the save to the cloud
            PlayGamesPlatform.Instance.SavedGame.CommitUpdate(game, updatedMetadata, savedData, HandleCloudSave);

            return true;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("Excetpion occured\n" + e.Message);
            return false;
        }
    }

    /// <summary>
    /// Call back function for whenever the game has saved the data. Nothing happens here but it is here for
    /// future implementations.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="game"></param>
    static void HandleCloudSave(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        // empty for now, here for future implementations
    }

    #endregion


    #region Cloud Load

    /// <summary>
    /// Saves the game to the Google Play cloud.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static bool CloudLoad(string filename)
    {
        try
        {
            if (PlayGamesPlatform.Instance.localUser.authenticated)
            {
                // if there are conflicts with saving the game (i.e. two devices trying to save at the same time), then 
                // the save file with the longest playing time will be the one that is saved to the cloud.
                PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(filename, DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseLongestPlaytime, HandleCloudLoad);
                return true;
            }

            return false;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("Excetpion occured\n" + e.Message);
            return false;
        }
    }

    /// <summary>
    /// Callback function for when the data has been loaded from the cloud.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="game"></param>
    static void HandleCloudLoad(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            CurrentGameMetaData = game;
            PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(game, HandleCloudBinaryLoad);
        }
        else
        {
            SaveManager.OnCloudDataLoaded(false, System.String.Empty);
        }
    }

    /// <summary>
    /// This function will take the data from the cloud and convert it into the correct data type suported
    /// by the game. Which in this case will be an integer becaue the only data saved onto the cloud is
    /// the player's highest score obtained.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="data"></param>
    static void HandleCloudBinaryLoad(SavedGameRequestStatus status, byte[] data)
    {
        SaveManager.OnCloudDataLoaded(status == SavedGameRequestStatus.Success, Encoding.UTF8.GetString(data));
    }

    #endregion
}
