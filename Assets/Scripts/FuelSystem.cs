using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class FuelSystem : MonoBehaviour
{
    [Header("References")]
    public PlayerUpgrades upgrades;
    public Slider fuelSlider;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;      // Kéo panel Game Over vào
    public TMP_Text coinsText;            // Kéo text hiển thị tiền kiếm được

    [Header("Fuel Values")]
    public float currentFuel;
    public UnityEvent<float> OnFuelChanged;
    public UnityEvent OnOutOfFuel;

    private float maxFuelUnits;
    private float timeToEmptySeconds;
    private bool gameOver = false;

    void Start()
    {
        if (upgrades == null)
            upgrades = FindObjectOfType<PlayerUpgrades>();

        RefreshMaxFuel();
        currentFuel = maxFuelUnits;

        if (fuelSlider != null)
        {
            fuelSlider.minValue = 0f;
            fuelSlider.maxValue = maxFuelUnits;
            fuelSlider.value = currentFuel;
        }

        OnFuelChanged.AddListener(UpdateFuelUI);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (gameOver) return; // ❌ Ngừng cập nhật nếu đã hết game

        float rate = maxFuelUnits / timeToEmptySeconds;
        currentFuel -= rate * Time.deltaTime;
        currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuelUnits);
        OnFuelChanged?.Invoke(currentFuel);

        if (currentFuel <= 0f)
        {
            OnOutOfFuel?.Invoke();
            ShowGameOver();
        }
    }

    public void RefreshMaxFuel()
    {
        maxFuelUnits = upgrades.GetMaxFuelUnits();
        timeToEmptySeconds = upgrades.GetTotalTimeToEmpty();
        currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuelUnits);
        OnFuelChanged?.Invoke(currentFuel);

        if (fuelSlider != null)
            fuelSlider.maxValue = maxFuelUnits;
    }

    public void AddFuel(float amount)
    {
        if (gameOver) return; // nếu đã hết game, không cộng xăng nữa

        currentFuel = Mathf.Clamp(currentFuel + amount, 0f, maxFuelUnits);
        OnFuelChanged?.Invoke(currentFuel);
    }

    private void UpdateFuelUI(float current)
    {
        if (fuelSlider != null)
            fuelSlider.value = current;
    }

    private void ShowGameOver()
    {
        gameOver = true;

        // Tắt điều khiển xe
        var carController = FindObjectOfType<ArcadeCarController_WheelCollider>();
        if (carController != null)
            carController.enabled = false;

        // Hiển thị panel Game Over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // Lấy coins của ván chơi và tổng coins tích lũy
            var currency = CurrencyManager.Instance;
            if (currency != null)
            {
                // Lưu tổng tiền vào PlayerPrefs
                currency.SaveCoins();

                int earned = currency.GetSessionCoins();
                int total = currency.GetTotalCoins();

                if (coinsText != null)
                    coinsText.text = $"Earned: {earned}\nTotal: {total}";
            }
            else
            {
                if (coinsText != null)
                    coinsText.text = $"Earned: 0\nTotal: 0";
            }
        }

        // Tạm dừng game
        Time.timeScale = 0f;
    }


    public float GetMaxFuel() => maxFuelUnits;
}
