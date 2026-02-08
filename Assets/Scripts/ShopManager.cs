using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public enum UpgradeType { FuelCapacity, FuelPickup, CrashReward }

    [System.Serializable]
    public class ShopItem
    {
        public string displayName;
        public UpgradeType type;
        public int price = 100;
        public Button buyButton;
        public Text priceText;
        public int maxLevel = 3;
    }

    [Header("Shop Items")]
    public List<ShopItem> items = new List<ShopItem>();

    [Header("UI References")]
    public TMP_Text totalCoinsTextTMP; // nếu bạn dùng TMP_Text
    public Text totalCoinsText;         // nếu bạn dùng Text thường

    private PlayerUpgrades upgrades;
    private CurrencyManager currency;

    void Start()
    {
        upgrades = FindObjectOfType<PlayerUpgrades>();
        currency = CurrencyManager.Instance;

        SetupButtons();
        UpdateCoinsUI(currency != null ? currency.coins : 0);

        // Đăng ký sự kiện cập nhật tiền tệ
        if (currency != null)
            currency.OnCurrencyChanged.AddListener(UpdateCoinsUI);
    }

    void OnDestroy()
    {
        if (currency != null)
            currency.OnCurrencyChanged.RemoveListener(UpdateCoinsUI);
    }

    void SetupButtons()
    {
        foreach (var item in items)
        {
            if (item.buyButton == null) continue;
            if (item.priceText != null)
                item.priceText.text = item.price.ToString();

            item.buyButton.onClick.AddListener(() => TryBuy(item));
        }
    }

    void TryBuy(ShopItem item)
    {
        if (currency == null) return;

        if (!currency.CanAfford(item.price))
        {
            Debug.Log("❌ Không đủ tiền để mua.");
            return;
        }

        int currentLevel = GetLevelFor(item.type);
        if (currentLevel >= item.maxLevel)
        {
            Debug.Log("⚠️ Đã đạt cấp tối đa cho món này.");
            return;
        }

        bool spent = currency.Spend(item.price);
        if (!spent) return;

        // ✅ Tăng cấp và lưu vào PlayerPrefs
        IncreaseLevelFor(item.type);
        upgrades.SaveUpgrades();

        // Cập nhật FuelSystem nếu cần
        var fuelSystem = FindObjectOfType<FuelSystem>();
        if (fuelSystem != null)
            fuelSystem.RefreshMaxFuel();

        Debug.Log($"✅ Mua thành công {item.displayName}. Cấp mới: {GetLevelFor(item.type)}");
    }


    int GetLevelFor(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.FuelCapacity: return upgrades.fuelCapacityUpgradeLevel;
            case UpgradeType.FuelPickup: return upgrades.fuelPickupUpgradeLevel;
            case UpgradeType.CrashReward: return upgrades.crashRewardUpgradeLevel;
        }
        return 0;
    }

    void IncreaseLevelFor(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.FuelCapacity:
                upgrades.fuelCapacityUpgradeLevel++;
                break;
            case UpgradeType.FuelPickup:
                upgrades.fuelPickupUpgradeLevel++;
                break;
            case UpgradeType.CrashReward:
                upgrades.crashRewardUpgradeLevel++;
                break;
        }
    }

    // 🔁 Hàm cập nhật UI khi tiền thay đổi
    void UpdateCoinsUI(int amount)
    {
        if (totalCoinsTextTMP != null)
            totalCoinsTextTMP.text = $"Coins: {amount}";
        if (totalCoinsText != null)
            totalCoinsText.text = $"Coins: {amount}";
    }
}
