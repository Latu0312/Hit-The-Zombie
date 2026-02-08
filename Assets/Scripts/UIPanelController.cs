using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý tất cả panel trong UI (mở, đóng, pause game, chuyển canvas).
/// Gắn script này lên 1 GameObject duy nhất (ví dụ: UIManager).
/// </summary>
[System.Serializable]
public class PanelData
{
    [Header("Panel Settings")]
    public string panelName;
    public GameObject panelObject;

    [Header("Button Controls")]
    public Button openButton;
    public Button closeButton;

    [Header("Pause Behavior")]
    public bool pauseOnOpen = true;
    public bool unpauseOnClose = true;
}

public class UIPanelController : MonoBehaviour
{
    [Header("Canvas Control")]
    public GameObject mainMenuCanvas;
    public GameObject gameplayCanvas;
    public GameObject gameOverPanel;

    [Header("Panel Configuration")]
    public List<PanelData> panels = new List<PanelData>();

    // 🧩 Thêm phần gameplay control
    [Header("Gameplay References")]
    public FuelSystem fuelSystem;     // Kéo FuelSystem trong scene vào
    public Spawner spawner;           // Kéo Spawner vào
    public GameObject carObject;      // Kéo xe (hoặc prefab xe trong scene)
    public Transform carSpawnPoint;   // Kéo điểm spawn ô tô vào Inspector

    private void Start()
    {
        // Gắn event cho các nút mở/đóng panel
        foreach (var panel in panels)
        {
            if (panel.panelObject != null)
                panel.panelObject.SetActive(false); // tắt panel lúc đầu

            if (panel.openButton != null)
                panel.openButton.onClick.AddListener(() => TogglePanel(panel, true));

            if (panel.closeButton != null)
                panel.closeButton.onClick.AddListener(() => TogglePanel(panel, false));
        }

        // Hiển thị menu chính khi bắt đầu
        ShowMainMenu();
    }

    // ================= CANVAS CONTROL =================
    public void ShowMainMenu()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        if (gameplayCanvas != null) gameplayCanvas.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        Time.timeScale = 0f; // dừng game ở menu
    }

    public void StartGameplay()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (gameplayCanvas != null) gameplayCanvas.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        Time.timeScale = 1f;

        // 🧩 Reset toàn bộ gameplay state khi bắt đầu ván mới
        ResetGameplay();
    }

    public void ShowGameOver()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (gameplayCanvas != null) gameplayCanvas.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // ================= PANEL CONTROL =================
    public void TogglePanel(PanelData panel, bool open)
    {
        if (panel.panelObject == null) return;

        panel.panelObject.SetActive(open);

        if (open && panel.pauseOnOpen)
            Time.timeScale = 0f;
        else if (!open && panel.unpauseOnClose)
            Time.timeScale = 1f;
    }

    // ================= HELPER =================
    public void CloseAllPanels()
    {
        foreach (var p in panels)
        {
            if (p.panelObject != null)
                p.panelObject.SetActive(false);
        }
    }

    // ================= GAMEPLAY RESET =================
    private void ResetGameplay()
    {
        // 1️⃣ Reset fuel
        if (fuelSystem != null)
        {
            fuelSystem.enabled = true;
            fuelSystem.currentFuel = fuelSystem.GetMaxFuel(); // set lại đầy xăng
            fuelSystem.OnFuelChanged?.Invoke(fuelSystem.currentFuel);
        }

        // 2️⃣ Respawn lại zombie/fuel
        if (spawner != null)
        {
            spawner.StopSpawning(); // dừng cũ (nếu còn)
            spawner.StartSpawning(); // bắt đầu lại
        }

        // 3️⃣ Đặt lại vị trí xe về điểm spawn
        if (carObject != null && carSpawnPoint != null)
        {
            Rigidbody rb = carObject.GetComponent<Rigidbody>();
            var carController = carObject.GetComponent<ArcadeCarController_WheelCollider>();

            // Tạm thời disable điều khiển để tránh xe tự di chuyển khi reset
            if (carController != null)
                carController.enabled = false;

            // Reset vị trí & hướng
            carObject.transform.SetPositionAndRotation(carSpawnPoint.position, carSpawnPoint.rotation);

            // Reset hoàn toàn vận tốc vật lý
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Bật lại điều khiển sau khi reset xong
            if (carController != null)
                carController.enabled = true;
        }
    }

}
