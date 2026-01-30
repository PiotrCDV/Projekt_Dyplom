using UnityEngine;

public class BossCombatManager : MonoBehaviour
{
    [Header("Hitbox References")]
    public BossDamage rightHandHitbox;

    [Header("Damage Balancing")]
    public int attack1Damage = 20;


    public void EnableRightHandHitbox()
    {
        if (rightHandHitbox != null)
        {
            rightHandHitbox.SetDamage(attack1Damage);
            rightHandHitbox.EnableDamage();
        }
    }


    public void DisableRightHandHitbox()
    {
        if (rightHandHitbox != null)
        {
            rightHandHitbox.DisableDamage();
        }
    }
}