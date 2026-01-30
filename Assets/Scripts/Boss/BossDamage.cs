using UnityEngine;
using System.Collections.Generic;

public class BossDamage : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask targetLayer;

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
                }

                hitTargets.Add(other.gameObject);
            }
        }
    }
}