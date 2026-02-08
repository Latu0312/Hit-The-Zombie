using UnityEngine;

/// <summary>
/// Gắn script này vào object xe (cùng object có Rigidbody & Collider).
/// Khi va chạm với GameObject có component Zombie, sẽ xử lý knockback và cộng tiền.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CarCrashHandler : MonoBehaviour
{
    [Header("Crash reward")]
    public int baseMinReward = 5;
    public int baseMaxReward = 10;

    [Header("Knockback")]
    public float forceMultiplier = 1f;

    private PlayerUpgrades upgrades;

    void Start()
    {
        upgrades = FindObjectOfType<PlayerUpgrades>();
        if (upgrades == null) upgrades = gameObject.AddComponent<PlayerUpgrades>(); // fallback
    }

    void OnCollisionEnter(Collision collision)
    {
        // Try get Zombie component on collided object
        var zombie = collision.gameObject.GetComponent<Zombie>();
        if (zombie != null)
        {
            Vector3 contactPoint = collision.contacts[0].point;
            // direction from car to zombie
            Vector3 dir = (collision.transform.position - transform.position).normalized;
            // Apply knockback
            zombie.ApplyKnockback(dir, forceMultiplier);

            // Give player random money based on upgrades
            int minR = baseMinReward;
            int maxR = baseMaxReward;

            // Upgrade that increases reward range: each level increases both min and max by increment
            int rewardUpgradeLevel = upgrades.crashRewardUpgradeLevel;
            int incrementPerLevel = upgrades.crashRewardIncrementPerLevel; // defined in PlayerUpgrades
            minR += rewardUpgradeLevel * incrementPerLevel;
            maxR += rewardUpgradeLevel * incrementPerLevel;

            int reward = Random.Range(minR, maxR + 1);
            CurrencyManager.Instance.AddCoins(reward);
            // optionally give feedback (UI popup, sound)
        }
    }
}
