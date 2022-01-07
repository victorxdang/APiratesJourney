/*****************************************************************************************************************
 - UpgradeUI.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Handles upgrading of the player ship's stats.
*****************************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : UIUtilities
{
    #region Constants

    /// <summary>
    /// This is the amount of upgrades that are available to all stats execept for the number of cannons stat.
    /// </summary>
    const int MAX_UPGRADES = 10;

    /// <summary>
    /// This is the amount of upgrades that are available for the number of cannons stat.
    /// </summary>
    const int MAX_UPGRADES_CANNON = 2;

    /// <summary>
    /// The child index of the current stat text. This text is the left hand side of the white arrow on each 
    /// respective upgrade bar.
    /// </summary>
    const int CURRENT_STAT_TEXT_INDEX = 2;

    /// <summary>
    /// The child index of the upgrade stat text. This text is the right hand side of the white arrow on each 
    /// respective upgrade bar.
    /// </summary>
    const int UPGRADE_STAT_TEXT_INDEX = 3;

    /// <summary>
    /// This is the child index of the image that will act as the fill image to indicate the amount of upgrades
    /// left for this stat.
    /// </summary>
    const int FILL_IMAGE_INDEX = 4;

    /// <summary>
    /// The child index of the button that will initiate the upgrade.
    /// </summary>
    const int BUTTON_INDEX = 5;

    #endregion


    #region Fields

    [SerializeField] GameObject upgradeHPBar,
                                upgradeArmorBar,
                                upgradeRepairBar,
                                upgradeCannonBar,
                                upgradeReloadBar,
                                upgradeDamageBar,
                                upgradeAccuracyBar;

    [SerializeField] Button buttonUpgradeHP,
                            buttonUpgradeArmor,
                            buttonUpgradeRepair,
                            buttonUpgradeCannon,
                            buttonUpgradeReload,
                            buttonUpgradeDamage,
                            buttonUpgradeAccuracy;

    [SerializeField] Text textGold;


    // output upgrade
    bool condition;
    int totalCost;

    // check if fully upgraded
    bool fullyUpgraded;

    #endregion


    #region Built-In Methods

    /// <summary>
    /// Displays the current amount of gold, sets up all upgrade buttons and updates the icons so that it will display
    /// correctly whether or not the player can buy the upgrade that the button is representing.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        SaveManager.PlayerSaveData.upgraded_ship = false;
        UpdateAllUpgradeBars();
        UpdateGoldText();
        gameObject.SetActive(false);
    }

    #endregion


    #region Update Stat/Icon

    /// <summary>
    /// Finalizes an upgrade of a particular type. The type parameter is obtained from the tag of the upgrade bar. The method 
    /// will save each time the player upgrades to ensure that the player doesn't lose their upgrades if something happens.
    /// </summary>
    /// <param name="type"></param>
    public void UpdateStat(string type)
    {
        AudioManager.Instance.PlayUIClick();

        SaveManager.PlayerSaveData.upgraded_ship = true;
        SaveManager.PlayerSaveData.player_gold -= (MainMenuUIManager.Instance.UnlimitedMoney) ? 0 : GetCost(type);
        SaveManager.PlayerSaveData.UpgradeAmount[type]++;

        UpdateGoldText();
        UpdateAllUpgradeBars();
        CheckIfFullyUpgraded();
    }

    /// <summary>
    /// Updates the upgrade bar so that it reflect the current stat, the next upgrade stat, the cost of the upgrade and the amount of upgrade left (indicated
    /// by how much the image has filled up so far).
    /// </summary>
    /// <param name="statBar"></param>
    /// <param name="button"></param>
    void UpdateDisplayBar(GameObject statBar, Button button)
    {
        condition = (statBar == upgradeArmorBar || statBar == upgradeRepairBar || statBar == upgradeReloadBar);
        totalCost = GetCost(statBar.tag);

        // fill the image so that it reflect how close the player is to fully upgrading the ship
        statBar.transform.GetChild(FILL_IMAGE_INDEX).GetComponent<Image>().fillAmount = SaveManager.PlayerSaveData.UpgradeAmount[statBar.tag] / (float) Scaling.UpgradeAmountMax[statBar.tag];

        // disable the button if the player has either reached max upgrades or does not have enough money
        button.interactable = SaveManager.PlayerSaveData.UpgradeAmount[statBar.tag] != Scaling.UpgradeAmountMax[statBar.tag] && (SaveManager.PlayerSaveData.player_gold > totalCost || MainMenuUIManager.Instance.UnlimitedMoney);

        // display the amount of gold needed for an upgrade or a "-" if the stat is fully upgraded
        button.transform.GetChild(0).GetComponent<Text>().text = (SaveManager.PlayerSaveData.UpgradeAmount[statBar.tag] == Scaling.UpgradeAmountMax[statBar.tag]) ? "-  " : totalCost.ToString("N0");

        // display what the current stat of the ship is
        statBar.transform.GetChild(CURRENT_STAT_TEXT_INDEX).GetComponent<Text>().text = GetStat(false, statBar.tag).ToString(condition ? "N2" : "N0");

        // display what the next upgraded stat of the ship would be, if fully upgraded, then display "MAX"
        statBar.transform.GetChild(UPGRADE_STAT_TEXT_INDEX).GetComponent<Text>().text = (SaveManager.PlayerSaveData.UpgradeAmount[statBar.tag] == Scaling.UpgradeAmountMax[statBar.tag]) ? "Max" : GetStat(true, statBar.tag).ToString(condition ? "N2" : "N0");
    }

    /// <summary>
    /// Updates the amount of gold displayed on the UI to reflect the amount that the player currently has.
    /// </summary>
    public void UpdateGoldText()
    {
        textGold.text = (MainMenuUIManager.Instance.UnlimitedMoney) ? "Unlimited" : SaveManager.PlayerSaveData.player_gold.ToString("N0");
    }

    #endregion


    #region Utilties

    /// <summary>
    /// Checks to see if all of the stats are fully upgraded or not. This is primarily used to keep
    /// track of the 'Upgrades Complete' achievement.
    /// </summary>
    void CheckIfFullyUpgraded()
    {
        fullyUpgraded = true;

        foreach (string key in SaveManager.PlayerSaveData.UpgradeAmount.Keys)
        {
            if (SaveManager.PlayerSaveData.UpgradeAmount[key] != Scaling.UpgradeAmountMax[key])
                fullyUpgraded = false;
        }

        // unlock Upgrades Complete achivement
        if (fullyUpgraded)
            GooglePlayGamesService.UnlockAchievement(GPGSIds.achievement_upgrades_complete);
    }

    /// <summary>
    /// Returns the stat of the ships, either the current stat or the upgraded stat depending on upgrade.
    /// </summary>
    /// <param name="upgrade"></param>
    /// <param name="type"></param>
    /// <returns> The current or upgraded stat of the ship </returns>
    float GetStat(bool upgrade, string type)
    {
        return Scaling.PlayerStatInitial[type] + ((SaveManager.PlayerSaveData.UpgradeAmount[type] + (upgrade ? 1 : 0)) * Scaling.PlayerStatMultiplier[type]);
    }

    /// <summary>
    /// Returns the cost required to upgrade a particular stat.
    /// </summary>
    /// <param name="upgrade"></param>
    /// <param name="type"></param>
    /// <returns> The cost of the upgrade </returns>
    int GetCost(string type)
    {
        return (int) (Scaling.UpgradeCost[type] + (Scaling.UpgradeCost[type] * Scaling.UpgradeCostMultiplier[type] * SaveManager.PlayerSaveData.UpgradeAmount[type]));
    }

    /// <summary>
    /// Goes back to the start page.
    /// </summary>
    public override void Back()
    {
        base.Back();
    }

    /// <summary>
    /// Closes the upgrade menu.
    /// </summary>
    public void CloseUpgrade()
    {
        AudioManager.Instance.PlayUIClick();
        Back();
    }

    #endregion


    #region Other/Repetitive Stuff

    /// <summary>
    /// Update all of the upgrade bars to reflect the correct stat, upgrade stat and cost of upgrades.
    /// </summary>
    public void UpdateAllUpgradeBars()
    {
        UpdateDisplayBar(upgradeHPBar, buttonUpgradeHP);
        UpdateDisplayBar(upgradeArmorBar, buttonUpgradeArmor);
        UpdateDisplayBar(upgradeRepairBar, buttonUpgradeRepair);
        UpdateDisplayBar(upgradeCannonBar, buttonUpgradeCannon);
        UpdateDisplayBar(upgradeReloadBar, buttonUpgradeReload);
        UpdateDisplayBar(upgradeDamageBar, buttonUpgradeDamage);
        UpdateDisplayBar(upgradeAccuracyBar, buttonUpgradeAccuracy);
    }

    #endregion
}
