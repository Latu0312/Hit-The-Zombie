using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to Zombie prefab. Require Rigidbody + Collider (non-kinematic).
/// Tag the prefab GameObject with tag "Zombie".
/// Add an AudioSource (set clip in prefab) � will be played when knocked.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Zombie : MonoBehaviour, IPooledObject
{
    [Header("Knockback")]
    public float knockbackForce = 30f;
    public float knockbackDuration = 2f;

    [Header("Return to pool")]
    public GameObject poolPrefabReference; // assign the prefab (used by PoolManager.Despawn)

    Rigidbody rb;
    Collider col;
    AudioSource audioSource;

    bool isKnocked = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
    }

    public void OnSpawned()
    {
        // reset state
        isKnocked = false;
        rb.isKinematic = false;
        col.enabled = true;
        // optionally enable AI scripts here if you have them
    }

    public void OnDespawned()
    {
        // nothing special
    }

    /// <summary>
    /// Called externally (from car) to apply knockback.
    /// </summary>
    public void ApplyKnockback(Vector3 direction, float forceMultiplier = 1f)
    {
        if (isKnocked) return;
        isKnocked = true;
        StartCoroutine(KnockbackRoutine(direction.normalized * knockbackForce * forceMultiplier));
    }

    IEnumerator KnockbackRoutine(Vector3 force)
    {
        // Disable AI here if needed

        // play sound
        if (audioSource != null)
        {
            audioSource.Play();
        }

        // apply impulse
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(force, ForceMode.Impulse);

        // optionally rotate/animation: e.g. play ragdoll

        // wait duration
        yield return new WaitForSeconds(knockbackDuration);

        // After 2 seconds, return to pool (disable)
        rb.linearVelocity = Vector3.zero;
        col.enabled = false;
        // optionally re-enable AI on spawn
        PoolManager.Instance.Despawn(gameObject, poolPrefabReference);
    }

    // Optional: if collided with car directly and you want to call from here
    void OnCollisionEnter(Collision other)
    {
        // nothing here � main flow handled by CarCrashHandler
    }
}
