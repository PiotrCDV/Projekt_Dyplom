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

    [Header("Attack Selection")]
    [SerializeField] private bool useWeightedAttackSelection = true;
    [SerializeField] private bool logSelectedAttackIndex = true;
    [SerializeField] private bool avoidRepeatingSameAttack = true;

    [Header("Animator Attack Lock")]
    [SerializeField] private string locomotionSpeedParam = "SpeedMagnitude";

    [Header("Attack Movement Lock")]
    [SerializeField] private bool unlockMovementOnlyByEvent = true;
    [SerializeField] private bool allowRotateWhileMovementLocked = true;

    private float lastAttackTime = -999f;
    private bool isAttacking = false;
    private BossAttack currentAttack;
    private GameObject currentTarget;
    private int lastSelectedAttackIndex = -1;
    private int locomotionSpeedParamHash;
    private bool hasLocomotionSpeedParam;
    private bool isMovementLockedByAttack;

    private void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (animator != null && !string.IsNullOrEmpty(locomotionSpeedParam))
        {
            locomotionSpeedParamHash = Animator.StringToHash(locomotionSpeedParam);
            hasLocomotionSpeedParam = HasAnimatorParameter(animator, locomotionSpeedParamHash, AnimatorControllerParameterType.Float);
        }
    }

    private void Update()
    {
        if (currentTarget != null && (allowRotateWhileMovementLocked || !isMovementLockedByAttack))
        {
            SmoothRotateTowardsTarget();
        }

        if (isMovementLockedByAttack && agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            if (agent.hasPath) agent.ResetPath();
        }

        if (isMovementLockedByAttack && animator != null && hasLocomotionSpeedParam)
        {
            animator.SetFloat(locomotionSpeedParamHash, 0f);
        }
    }

    private static bool HasAnimatorParameter(Animator anim, int hash, AnimatorControllerParameterType type)
    {
        foreach (var param in anim.parameters)
        {
            if (param.nameHash == hash && param.type == type) return true;
        }
        return false;
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

        if (isAttacking || isMovementLockedByAttack) return;
        if (availableAttacks == null || availableAttacks.Count == 0) return;

        int selectedAttackIndex = ResolveAttackIndex(attackIndex);
        if (selectedAttackIndex < 0 || selectedAttackIndex >= availableAttacks.Count) return;

        BossAttack attackToPerform = availableAttacks[selectedAttackIndex];

        if (logSelectedAttackIndex)
        {
            Debug.Log($"[BossCombatManager] Selected attack: {attackToPerform.attackName}");
        }

        if (Time.time - lastAttackTime < attackToPerform.cooldown) return;

        PerformAttackLogic(attackToPerform);
    }

    private int ResolveAttackIndex(int behaviorGraphIndex)
    {
        List<int> candidateIndices = new List<int>();
        for (int i = 0; i < availableAttacks.Count; i++)
        {
            if (availableAttacks[i] != null) candidateIndices.Add(i);
        }

        if (candidateIndices.Count == 0) return -1;

        if (!useWeightedAttackSelection)
        {
            return (behaviorGraphIndex >= 0 && behaviorGraphIndex < availableAttacks.Count) ? behaviorGraphIndex : -1;
        }

        if (avoidRepeatingSameAttack && candidateIndices.Count > 1)
        {
            candidateIndices.Remove(lastSelectedAttackIndex);
        }

        float totalWeight = 0f;
        foreach (int i in candidateIndices)
        {
            totalWeight += Mathf.Max(0f, availableAttacks[i].selectionChancePercent);
        }

        if (totalWeight <= 0f) return candidateIndices[Random.Range(0, candidateIndices.Count)];

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (int i in candidateIndices)
        {
            cumulative += Mathf.Max(0f, availableAttacks[i].selectionChancePercent);
            if (roll <= cumulative) return i;
        }

        return candidateIndices[candidateIndices.Count - 1];
    }

    private void PerformAttackLogic(BossAttack attack)
    {
        lastAttackTime = Time.time;
        isAttacking = true;
        currentAttack = attack;
        lastSelectedAttackIndex = availableAttacks.IndexOf(attack);
        LockMovementForAttack();

        if (animator != null)
        {
            if (hasLocomotionSpeedParam) animator.SetFloat(locomotionSpeedParamHash, 0f);
            animator.SetTrigger(attack.animationTrigger);
        }
    }


    public void EnableMainHitbox()
    {
        if (currentAttack == null || currentAttack.hitbox == null) return;

        currentAttack.hitbox.SetDamage(currentAttack.damage);
        currentAttack.hitbox.EnableDamage();
    }

    public void EnableSecondHitbox()
    {
        if (currentAttack == null || currentAttack.secondHitbox == null) return;

        currentAttack.secondHitbox.SetDamage(currentAttack.damage);
        currentAttack.secondHitbox.EnableDamage();
    }

    public void DisableAllHitboxes()
    {
        if (currentAttack == null) return;

        if (currentAttack.hitbox != null) currentAttack.hitbox.DisableDamage();
        if (currentAttack.secondHitbox != null) currentAttack.secondHitbox.DisableDamage();
    }

    public void OnAttackAnimationEnd()
    {
        DisableAllHitboxes();
        isAttacking = false;
        currentAttack = null;

        if (!unlockMovementOnlyByEvent)
        {
            UnlockMovementAfterAttack();
        }
    }

    public void OnAttackMovementUnlockEvent()
    {
        UnlockMovementAfterAttack();
    }

    private void LockMovementForAttack()
    {
        isMovementLockedByAttack = true;
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
    }

    private void UnlockMovementAfterAttack()
    {
        isMovementLockedByAttack = false;
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
    [Range(0f, 100f)] public float selectionChancePercent = 100f;
    public int damage;              
    public float cooldown;          
    public float attackRange;       
    public BossDamage hitbox;
    public BossDamage secondHitbox;
}