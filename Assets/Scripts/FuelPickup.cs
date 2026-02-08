using UnityEngine;
using System.Collections;

public class FuelPickup : MonoBehaviour, IPooledObject
{
    public GameObject poolPrefabReference;
    private Collider col;
    private bool activeState = false;

    [Header("Floating Rotation Effect")]
    [Tooltip("Tốc độ xoay quanh trục Y (độ/giây)")]
    public float rotationSpeed = 50f;
    [Tooltip("Biên độ di chuyển lơ lửng lên xuống")]
    public float floatAmplitude = 0.25f;
    [Tooltip("Tốc độ dao động lơ lửng")]
    public float floatFrequency = 2f;

    private Vector3 startPos;

    void Awake()
    {
        col = GetComponent<Collider>();
        startPos = transform.localPosition;
    }

    void Update()
    {
        if (!activeState) return;

        // 🌀 Xoay quanh trục Y
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);

        // 🌫️ Lơ lửng nhẹ lên xuống theo sin
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        Vector3 pos = transform.position;
        pos.y = newY;
        transform.position = pos;
    }

    public void OnSpawned()
    {
        activeState = true;
        if (col != null) col.enabled = true;
        gameObject.SetActive(true);
        startPos = transform.position; // lưu vị trí ban đầu khi spawn
    }

    public void OnDespawned()
    {
        activeState = false;
        if (col != null) col.enabled = false;
        gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🔥 Trigger Enter with: {other.name}");
        if (!activeState) return;
        if (!other.CompareTag("Car")) return;

        Debug.Log($"Fuel collided with: {other.name}");

        var upgrades = FindObjectOfType<PlayerUpgrades>();
        var fuelSys = other.GetComponent<FuelSystem>()
                    ?? other.GetComponentInParent<FuelSystem>()
                    ?? other.GetComponentInChildren<FuelSystem>();

        if (fuelSys == null)
        {
            Debug.LogWarning("⚠️ FuelSystem not found on Car!");
            return;
        }

        float amount = upgrades != null ? upgrades.GetFuelPickupAmount() : 25f;
        Debug.Log($"Before fuel: {fuelSys.currentFuel}");
        fuelSys.AddFuel(amount);
        Debug.Log($"After fuel: {fuelSys.currentFuel}");

        StartCoroutine(DespawnAfterFrame());
    }

    IEnumerator DespawnAfterFrame()
    {
        yield return null;
        PoolManager.Instance.Despawn(gameObject, poolPrefabReference);
    }
}
