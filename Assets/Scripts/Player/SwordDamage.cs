using UnityEngine;
using System.Collections.Generic;

public class SwordDamage : MonoBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] private float damageAmount = 20f;
    public Collider swordCollider;

    [Header("Visual Effects")]
    [SerializeField] private GameObject bloodParticlePrefab;

    private List<IDamageable> hitTargets = new List<IDamageable>();

    private void Awake()
    {
        if (swordCollider == null)
            swordCollider = GetComponent<Collider>();

        swordCollider.enabled = false;
        swordCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable target = other.GetComponent<IDamageable>();

        if (target == null)
        {
            target = other.GetComponentInParent<IDamageable>();
        }

        if (target != null && !hitTargets.Contains(target))
        {
            target.TakeDamage(damageAmount);
            hitTargets.Add(target);

            // --- SYSTEM KRWI ---
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
    }

    public void SetDamage(float amount)
    {
        damageAmount = amount;
    }

    public void EnableDamage()
    {
        hitTargets.Clear();
        swordCollider.enabled = true;
    }

    public void DisableDamage()
    {
        swordCollider.enabled = false;
    }
}