using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Quản lý tiền tệ (coins), lưu tổng xu vào PlayerPrefs.
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    public int coins = 0;              // Tổng số xu hiện có (được lưu giữa các lần chơi)
    public int sessionCoins = 0;       // Số xu kiếm được trong ván chơi hiện tại
    public UnityEvent<int> OnCurrencyChanged;

    const string PREF_COINS = "TotalCoins";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCoins();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        sessionCoins += amount;
        SaveCoins();
        OnCurrencyChanged?.Invoke(coins);
        Debug.Log($"🪙 +{amount} coins (total: {coins}, session: {sessionCoins})");
    }

    public bool Spend(int amount)
    {
        if (coins < amount) return false;
        coins -= amount;
        SaveCoins();
        OnCurrencyChanged?.Invoke(coins);
        return true;
    }

    public bool CanAfford(int amount) => coins >= amount;

    public void SaveCoins()
    {
        PlayerPrefs.SetInt(PREF_COINS, coins);
        PlayerPrefs.Save();
    }

    public void LoadCoins()
    {
        coins = PlayerPrefs.GetInt(PREF_COINS, 0);
    }

    public int GetTotalCoins() => coins;
    public int GetSessionCoins() => sessionCoins;

    // Reset session coins khi bắt đầu ván mới
    public void ResetSession()
    {
        sessionCoins = 0;
    }
}
