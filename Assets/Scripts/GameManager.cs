using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý trạng thái game: end when fuel out.
/// Gắn panel Game Over (assign in inspector).
/// </summary>
public class GameManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public Spawner spawner;
    public FuelSystem fuelSystem;

    void Start()
    {
        if (spawner == null) spawner = FindObjectOfType<Spawner>();
        if (fuelSystem == null) fuelSystem = FindObjectOfType<FuelSystem>();

        if (fuelSystem != null)
        {
            fuelSystem.OnOutOfFuel.AddListener(OnOutOfFuel);
        }

        gameOverPanel?.SetActive(false);
    }

    void OnOutOfFuel()
    {
        // show panel and stop spawning, optionally disable player control
        gameOverPanel?.SetActive(true);
        spawner?.StopSpawning();

        // optionally freeze time or disable player input
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        // implement restart: reload scene or reset state
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
