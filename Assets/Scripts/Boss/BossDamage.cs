using UnityEngine;
using System.Collections.Generic;

public class BossDamage : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask targetLayer;

    [Header("Visual Effects")]
    [SerializeField] private GameObject bloodParticlePrefab;

    private int currentDamage;
    private bool isDamageEnabled = false;
    private List<GameObject> hitTargets = new List<GameObject>();

    public void SetDamage(int amount)
    {
        currentDamage = amount;
    }

    public void EnableDamage()
    {
        hitTargets.Clear();
        isDamageEnabled = true;
    }

    public void DisableDamage()
    {
        isDamageEnabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isDamageEnabled) return;
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            if (!hitTargets.Contains(other.gameObject))
            {
                var playerHealth = other.GetComponent<PlayerHealth>();

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(currentDamage);

                    // --- SYSTEM KRWI GRACZA ---
                    if (bloodParticlePrefab != null)
                    {
                        Vector3 hitPoint = other.ClosestPoint(transform.position);
                        Vector3 hitDirection = (transform.position - hitPoint).normalized;
                        if (hitDirection == Vector3.zero) hitDirection = Vector3.up;
                        Quaternion bloodRotation = Quaternion.LookRotation(hitDirection);
                        GameObject blood = Instantiate(bloodParticlePrefab, hitPoint, bloodRotation);
                        Destroy(blood, 2f);
                    }
                }

                hitTargets.Add(other.gameObject);
            }
        }
    }
}