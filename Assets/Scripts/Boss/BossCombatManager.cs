using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class BossCombatManager : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

    [Header("Attacks Configuration")]
    public List<BossAttack> availableAttacks;

    [Header("Global Cooldown Settings")]
    [Tooltip("Czas przerwy między zakończeniem jednego ataku a rozpoczęciem kolejnego")]
    public float timeBetweenAttacks = 2f;
    private float nextAllowedAttackTime = -999f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 10f;

    [Header("Attack Selection")]
    [SerializeField] private bool useWeightedAttackSelection = true;
    [SerializeField] private bool logSelection = true;
    [SerializeField] private int maxConsecutiveAttacks = 2;

    [Header("Attack Lock Settings")]
    [SerializeField] private bool unlockMovementOnlyByEvent = false; 
    [SerializeField] private string locomotionSpeedParam = "SpeedMagnitude";

    private bool isAttacking = false;
    private BossAttack currentAttack;
    private GameObject currentTarget;
    
    private int lastSelectedAttackIndex = -1;
    private int consecutiveCount = 0;

    private int locomotionSpeedParamHash;
    private bool hasLocomotionSpeedParam;
    private bool isMovementLockedByAttack = false;
    private bool isDashing = false;
    private Coroutine dashCoroutine;

    private void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (animator != null && !string.IsNullOrEmpty(locomotionSpeedParam))
        {
            locomotionSpeedParamHash = Animator.StringToHash(locomotionSpeedParam);
            hasLocomotionSpeedParam = true;
        }
    }

    private void Update()
    {
        if (currentTarget != null && !isDashing && (!isMovementLockedByAttack))
        {
            SmoothRotateTowardsTarget();
        }

        if (isMovementLockedByAttack && agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (isMovementLockedByAttack && animator != null && hasLocomotionSpeedParam)
        {
            animator.SetFloat(locomotionSpeedParamHash, 0f);
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

    public void TryAttack(int behaviorIndex, GameObject target)
    {
        currentTarget = target;

        if (isAttacking || isMovementLockedByAttack || Time.time < nextAllowedAttackTime) return;

        List<int> readyIndices = new List<int>();
        for (int i = 0; i < availableAttacks.Count; i++)
        {
            if (i != lastSelectedAttackIndex || consecutiveCount < maxConsecutiveAttacks)
            {
                readyIndices.Add(i);
            }
        }

        if (readyIndices.Count == 0)
        {
            for (int i = 0; i < availableAttacks.Count; i++) readyIndices.Add(i);
        }

        int finalIndex = -1;
        if (!useWeightedAttackSelection)
        {
            finalIndex = (behaviorIndex >= 0 && behaviorIndex < availableAttacks.Count) ? behaviorIndex : readyIndices[0];
        }
        else
        {
            finalIndex = RollWeightedAttack(readyIndices);
        }

        if (finalIndex != -1)
        {
            PerformAttackLogic(availableAttacks[finalIndex], finalIndex);
        }
    }

    private int RollWeightedAttack(List<int> candidates)
    {
        float totalWeight = 0;
        foreach (int i in candidates) totalWeight += Mathf.Max(0.1f, availableAttacks[i].selectionChancePercent);

        float roll = Random.Range(0, totalWeight);
        float cumulative = 0;

        foreach (int i in candidates)
        {
            cumulative += Mathf.Max(0.1f, availableAttacks[i].selectionChancePercent);
            if (roll <= cumulative) return i;
        }
        return candidates[0];
    }

    private void PerformAttackLogic(BossAttack attack, int index)
    {
        if (index == lastSelectedAttackIndex) consecutiveCount++;
        else consecutiveCount = 1;

        lastSelectedAttackIndex = index;
        isAttacking = true;
        currentAttack = attack;
        
        LockMovementForAttack();

        if (logSelection) Debug.Log($"[BossAI] Atak: {attack.attackName}");

        if (animator != null)
        {
            animator.SetTrigger(attack.animationTrigger);
        }
    }


    public void OnDashAttackStart()
    {
        if (currentAttack != null && currentAttack.isDashAttack)
        {
            if (dashCoroutine != null) StopCoroutine(dashCoroutine);
            dashCoroutine = StartCoroutine(DashRoutine(currentAttack.dashDistance, currentAttack.dashDuration));
        }
    }

    private IEnumerator DashRoutine(float dist, float dur)
    {
        isDashing = true;
        float elapsed = 0;
        Vector3 dir = transform.forward;
        while (elapsed < dur)
        {
            if (agent != null && agent.enabled) agent.Move(dir * (dist / dur) * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        isDashing = false;
        dashCoroutine = null;
    }

    public void EnableMainHitbox() { if (currentAttack?.hitbox != null) { currentAttack.hitbox.SetDamage(currentAttack.damage); currentAttack.hitbox.EnableDamage(); } }
    public void EnableSecondHitbox() { if (currentAttack?.secondHitbox != null) { currentAttack.secondHitbox.SetDamage(currentAttack.damage); currentAttack.secondHitbox.EnableDamage(); } }
    public void DisableAllHitboxes() { currentAttack?.hitbox?.DisableDamage(); currentAttack?.secondHitbox?.DisableDamage(); }

    public void OnAttackAnimationEnd()
    {
        DisableAllHitboxes();
        isAttacking = false;
        currentAttack = null;
        isDashing = false;

        nextAllowedAttackTime = Time.time + timeBetweenAttacks;

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
        if (agent != null && agent.enabled) agent.isStopped = true; 
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
    [Range(0f, 100f)] public float selectionChancePercent = 50f;
    public int damage;
    public BossDamage hitbox;
    public BossDamage secondHitbox;

    [Header("Dash Settings")]
    public bool isDashAttack;
    public float dashDistance;
    public float dashDuration;
}