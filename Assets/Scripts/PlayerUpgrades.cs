using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    [Header("Fuel (max time)")]
    public int fuelCapacityUpgradeLevel = 0;

    [Header("Fuel pickup effectiveness")]
    public int fuelPickupUpgradeLevel = 0;

    [Header("Crash reward upgrade")]
    public int crashRewardUpgradeLevel = 0;

    [Header("Config")]
    public int maxUpgradeLevel = 3;
    public int crashRewardIncrementPerLevel = 5;

    public float baseMaxFuelUnits = 100f;
    public float baseTimeToEmptySeconds = 180f;

    const string KEY_FUEL_CAP = "Upgrade_FuelCapacity";
    const string KEY_FUEL_PICKUP = "Upgrade_FuelPickup";
    const string KEY_CRASH_REWARD = "Upgrade_CrashReward";

    void Awake()
    {
        LoadUpgrades();
    }

    public void SaveUpgrades()
    {
        PlayerPrefs.SetInt(KEY_FUEL_CAP, fuelCapacityUpgradeLevel);
        PlayerPrefs.SetInt(KEY_FUEL_PICKUP, fuelPickupUpgradeLevel);
        PlayerPrefs.SetInt(KEY_CRASH_REWARD, crashRewardUpgradeLevel);
        PlayerPrefs.Save();
    }

    public void LoadUpgrades()
    {
        fuelCapacityUpgradeLevel = PlayerPrefs.GetInt(KEY_FUEL_CAP, 0);
        fuelPickupUpgradeLevel = PlayerPrefs.GetInt(KEY_FUEL_PICKUP, 0);
        crashRewardUpgradeLevel = PlayerPrefs.GetInt(KEY_CRASH_REWARD, 0);
    }

    public float GetTotalTimeToEmpty()
    {
        return baseTimeToEmptySeconds + 30f * fuelCapacityUpgradeLevel;
    }

    public float GetMaxFuelUnits()
    {
        float newTime = GetTotalTimeToEmpty();
        return baseMaxFuelUnits * (newTime / baseTimeToEmptySeconds);
    }

    public float GetFuelPickupAmount()
    {
        return 25f + 25f * fuelPickupUpgradeLevel;
    }
}
