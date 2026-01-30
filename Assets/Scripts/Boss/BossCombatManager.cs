using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class BossCombatManager : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

    [Header("Attacks Configuration")]
    public List<BossAttack> availableAttacks;

    [Header("Rotation Settings")]
    public float rotationSpeed = 10f;

    private float lastAttackTime = -999f;
    private bool isAttacking = false;
    private BossAttack currentAttack;
    private GameObject currentTarget;

    private void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {

        if (currentTarget != null)
        {
            SmoothRotateTowardsTarget();
        }

        if (isAttacking && agent != null && agent.enabled)
        {
            agent.velocity = Vector3.zero;
        }
    }

    private void SmoothRotateTowardsTarget()
    {
        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    public void TryAttack(int attackIndex, GameObject target)
    {
        currentTarget = target;

        if (isAttacking) return;
        if (attackIndex < 0 || attackIndex >= availableAttacks.Count) return;

        BossAttack attackToPerform = availableAttacks[attackIndex];

        if (Time.time - lastAttackTime < attackToPerform.cooldown) return;

        PerformAttackLogic(attackToPerform);
    }

    private void PerformAttackLogic(BossAttack attack)
    {
        lastAttackTime = Time.time;
        isAttacking = true;
        currentAttack = attack;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true; 
            agent.velocity = Vector3.zero; 
        }

        if (animator != null)
        {
            animator.SetTrigger(attack.animationTrigger);
        }
    }


    public void EnableCurrentHitbox()
    {
        if (currentAttack != null && currentAttack.hitbox != null)
        {
            currentAttack.hitbox.SetDamage(currentAttack.damage);
            currentAttack.hitbox.EnableDamage();
        }
    }

    public void DisableCurrentHitbox()
    {
        if (currentAttack != null && currentAttack.hitbox != null)
        {
            currentAttack.hitbox.DisableDamage();
        }
    }

    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
        currentAttack = null;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = false; 
        }
    }
}
[System.Serializable]
public class BossAttack
{
    public string attackName; 
    public string animationTrigger; 
    public int damage;              
    public float cooldown;          
    public float attackRange;       
    public BossDamage hitbox; 
}