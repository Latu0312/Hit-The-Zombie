using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Prefabs (assign prefabs, which should have poolPrefabReference set)")]
    public GameObject zombiePrefab;
    public GameObject fuelPrefab;

    [Header("Spawn points")]
    public List<Transform> zombieSpawnPoints = new List<Transform>();
    public List<Transform> fuelSpawnPoints = new List<Transform>();

    [Header("Spawn config")]
    public float zombieSpawnInterval = 3f;
    public int maxZombiesAtOnce = 10;
    public float fuelSpawnInterval = 10f;
    public int maxFuelAtOnce = 5;

    [Header("Spawn radius settings")]
    [Tooltip("Bán kính spawn quanh điểm spawn")]
    public float spawnRadius = 2f;
    [Tooltip("Khoảng cách tối thiểu giữa các vật thể spawn ra để tránh chồng lên nhau")]
    public float minDistanceBetweenSpawns = 1.5f;

    int currentZombies = 0;
    int currentFuel = 0;
    bool spawning = false;

    Coroutine zombieRoutine;
    Coroutine fuelRoutine;

    private int zombieSpawnIndex = 0;
    private int fuelSpawnIndex = 0;

    private List<Vector3> activeSpawnPositions = new List<Vector3>();

    void Start()
    {
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Prewarm(zombiePrefab, Mathf.Min(10, maxZombiesAtOnce));
            PoolManager.Instance.Prewarm(fuelPrefab, Mathf.Min(5, maxFuelAtOnce));
        }

        StartSpawning();
    }

    public void StartSpawning()
    {
        if (spawning) return;
        spawning = true;
        zombieRoutine = StartCoroutine(ZombieSpawnLoop());
        fuelRoutine = StartCoroutine(FuelSpawnLoop());
    }

    public void StopSpawning()
    {
        spawning = false;
        if (zombieRoutine != null) StopCoroutine(zombieRoutine);
        if (fuelRoutine != null) StopCoroutine(fuelRoutine);
    }

    IEnumerator ZombieSpawnLoop()
    {
        while (spawning)
        {
            if (currentZombies < maxZombiesAtOnce && zombieSpawnPoints.Count > 0)
            {
                Transform sp = zombieSpawnPoints[zombieSpawnIndex];
                zombieSpawnIndex = (zombieSpawnIndex + 1) % zombieSpawnPoints.Count;

                Vector3 spawnPos = GetValidSpawnPosition(sp.position);

                GameObject z = PoolManager.Instance.Spawn(zombiePrefab, spawnPos, Quaternion.identity);
                var zombie = z.GetComponent<Zombie>();
                if (zombie != null && zombie.poolPrefabReference == null)
                    zombie.poolPrefabReference = zombiePrefab;

                activeSpawnPositions.Add(spawnPos);
                currentZombies++;

                var helper = z.GetComponent<SpawnedMarker>();
                if (helper == null) helper = z.AddComponent<SpawnedMarker>();
                helper.onReturned += () =>
                {
                    currentZombies--;
                    activeSpawnPositions.Remove(spawnPos);
                };
            }
            yield return new WaitForSeconds(zombieSpawnInterval);
        }
    }

    IEnumerator FuelSpawnLoop()
    {
        while (spawning)
        {
            if (currentFuel < maxFuelAtOnce && fuelSpawnPoints.Count > 0)
            {
                Transform sp = fuelSpawnPoints[fuelSpawnIndex];
                fuelSpawnIndex = (fuelSpawnIndex + 1) % fuelSpawnPoints.Count;

                Vector3 spawnPos = GetValidSpawnPosition(sp.position);

                // 🧩 Spawn bình xăng
                GameObject f = PoolManager.Instance.Spawn(fuelPrefab, spawnPos, Quaternion.identity);
                var pickup = f.GetComponent<FuelPickup>();
                if (pickup != null && pickup.poolPrefabReference == null)
                    pickup.poolPrefabReference = fuelPrefab;

                // 🧩 Thêm xử lý định hướng và căn độ cao
                AdjustFuelSpawnPosition(f);

                activeSpawnPositions.Add(f.transform.position);
                currentFuel++;

                var helper = f.GetComponent<SpawnedMarker>();
                if (helper == null) helper = f.AddComponent<SpawnedMarker>();
                helper.onReturned += () =>
                {
                    currentFuel--;
                    activeSpawnPositions.Remove(f.transform.position);
                };
            }
            yield return new WaitForSeconds(fuelSpawnInterval);
        }
    }

    /// <summary>
    /// Điều chỉnh rotation và độ cao của fuelPrefab theo địa hình.
    /// </summary>
    private void AdjustFuelSpawnPosition(GameObject fuelObj)
    {
        if (fuelObj == null) return;

        // Xoay nghiêng theo trục X = -90
        fuelObj.transform.rotation = Quaternion.Euler(-90f, fuelObj.transform.rotation.eulerAngles.y, 0f);

        // Raycast tìm mặt đất bên dưới
        RaycastHit hit;
        Vector3 rayOrigin = fuelObj.transform.position + Vector3.up * 10f; // bắn tia từ trên xuống
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 50f))
        {
            // spawn cách mặt đất 1.2 đơn vị
            fuelObj.transform.position = hit.point + Vector3.up * 0.5f;
        }
        else
        {
            // fallback nếu không có terrain (vẫn giữ vị trí gốc)
            fuelObj.transform.position += Vector3.up * 0.5f;
        }
    }

    /// <summary>
    /// Trả về vị trí spawn ngẫu nhiên trong bán kính quanh 1 điểm,
    /// đảm bảo không quá gần các vật thể khác đã spawn.
    /// </summary>
    Vector3 GetValidSpawnPosition(Vector3 center)
    {
        const int maxAttempts = 10;
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 candidate = center + new Vector3(randomOffset.x, 0f, randomOffset.y);

            bool tooClose = false;
            foreach (var pos in activeSpawnPositions)
            {
                if (Vector3.Distance(candidate, pos) < minDistanceBetweenSpawns)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
                return candidate;
        }
        return center;
    }
}

public class SpawnedMarker : MonoBehaviour
{
    public System.Action onReturned;
    void OnDisable()
    {
        onReturned?.Invoke();
    }
}
