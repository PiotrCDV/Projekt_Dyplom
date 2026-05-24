        inputActions.Player.Attack.performed += ctx => HandleAttackInput(false);
        inputActions.Player.HeavyAttack.performed += ctx => HandleAttackInput(true);
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    private Animator animator;
    private PlayerMovement playerMovement;
    private LockOnBehaviour lockOnBehaviour;
    private SwordDamage weaponScript;
    private PlayerDodge playerDodge;
    private PlayerStamina stamina;
    private PlayerPotion playerPotion;

    public float lightAttackStaminaCost = 15f;
    public float heavyAttackStaminaCost = 25f;
    public float sprintAttackStaminaCost = 20f;

    private int comboStep = 0;
    private bool isAttacking = false;
    private bool inputQueued = false;
    private bool allowInputQueuing = false;
    private bool nextAttackIsHeavy = false;
    private bool currentComboIsHeavy = false;

    private bool hasBufferedUnlockSprintAttack;
    private bool bufferedUnlockSprintAttackHeavy;
    private float bufferedUnlockSprintAttackExpireAt;
    private const float unlockSprintAttackBufferDuration = 0.22f;
    private const float sprintAttackMinSpeed = 1.4f;

    private bool currentAttackWasSprintAttack;

    private Coroutine attackFailsafe;
    private InputSystem_Actions inputActions;
    public bool IsAttacking => isAttacking;
    public bool IsPerformingSprintAttack => isAttacking && currentAttackWasSprintAttack;
    
    [Header("Movement Settings")]
    [SerializeField] private float movementReEnableDelay = 0.5f; 
    private Coroutine movementDelayCoroutine; 
    
    [Header("Execution Settings")]
    public float maxExecutionDistance = 3.0f; 

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        lockOnBehaviour = GetComponent<LockOnBehaviour>();
        weaponScript = GetComponentInChildren<SwordDamage>();
        playerDodge = GetComponent<PlayerDodge>();
        stamina = GetComponent<PlayerStamina>();
        playerPotion = GetComponent<PlayerPotion>();

        inputActions = new InputSystem_Actions();
        inputActions.Player.Attack.performed += ctx => HandleAttackInput(false);
        inputActions.Player.HeavyAttack.performed += ctx => HandleAttackInput(true);

        if (lockOnBehaviour != null) lockOnBehaviour.OnUnlock += HandleUnlockDuringAttack;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void OnDestroy()
    {
        if (lockOnBehaviour != null) lockOnBehaviour.OnUnlock -= HandleUnlockDuringAttack;
    }

    private void HandleAttackInput(bool heavy)
    {
        if (playerPotion != null && playerPotion.IsDrinking) return;
        if (playerDodge != null && playerDodge.IsDodging) return;
        if (stamina != null && !stamina.CanPerformAction()) return;

        if (!heavy && TryExecuteBoss())
        {
            return;
        }

        if (isAttacking)
        {
            if (allowInputQueuing)
            {
                inputQueued = true;
                nextAttackIsHeavy = heavy;
            }
            return;
        }

        nextAttackIsHeavy = heavy;
        PerformAttack();
    }

    private void PerformAttack()
    {
        if (movementDelayCoroutine != null) StopCoroutine(movementDelayCoroutine);
        
        bool isLocked = lockOnBehaviour != null && lockOnBehaviour.IsLocked;
        bool isSprintingInput = playerMovement != null && playerMovement.IsSprinting;
        bool hasSprintSpeed = HasSprintAttackSpeed();
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isInFightSprint = stateInfo.IsName("Fight_Sprint");
        bool isInFreeSprint = stateInfo.IsName("Sprint");
        bool transitioningToFightSprint = animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).IsName("Fight_Sprint");
        bool transitioningToFreeSprint = animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).IsName("Sprint");


        if (!isLocked && isSprintingInput && isInFightSprint && !transitioningToFreeSprint)
        {
            BufferUnlockSprintAttack(nextAttackIsHeavy);
            return;
        }

        bool shouldDoSprintAttack;
        if (isLocked)
        {
            bool sprintIntentByParams = animator.GetBool("FightSprint");
            bool isSprintingInAnimator = isInFightSprint || isInFreeSprint;
            shouldDoSprintAttack = hasSprintSpeed && (isSprintingInAnimator || (isSprintingInput && (transitioningToFightSprint || sprintIntentByParams)));
        }
        else
        {
            shouldDoSprintAttack = hasSprintSpeed && isSprintingInput && (isInFreeSprint || transitioningToFreeSprint);
        }

        if (!isLocked && !shouldDoSprintAttack)
        {
            ResetCombatState();
            return;
        }

        isAttacking = true;
        inputQueued = false;
        allowInputQueuing = false;

        if (playerMovement != null) playerMovement.SetMovementEnabled(false);

        if (attackFailsafe != null) StopCoroutine(attackFailsafe);
        attackFailsafe = StartCoroutine(AttackFailsafeRoutine());

        float currentCost = shouldDoSprintAttack ? sprintAttackStaminaCost : (nextAttackIsHeavy ? heavyAttackStaminaCost : lightAttackStaminaCost);
        if (stamina != null) stamina.UseStamina(currentCost);

        if (shouldDoSprintAttack)
        {
            animator.SetBool("SprintAttackDelay", true);
            string sprintTrigger = ResolveSprintAttackTrigger(stateInfo, isLocked);
            ResetConflictingTriggers(sprintTrigger);
            animator.SetTrigger(sprintTrigger);
            comboStep = 0;
            currentComboIsHeavy = false;
            currentAttackWasSprintAttack = true;
        }
        else
        {
            animator.SetBool("SprintAttackDelay", false);
            string triggerToFire = "";

            if (nextAttackIsHeavy)
            {
                if (comboStep == 1 && !currentComboIsHeavy) triggerToFire = "HAttack2";
                else if (comboStep == 2 && !currentComboIsHeavy) triggerToFire = "HAttack2";
                else if (comboStep == 3 && !currentComboIsHeavy) triggerToFire = "HAttack1";
                else if (comboStep == 1 && currentComboIsHeavy) triggerToFire = "HAttack2";
                else triggerToFire = "HAttack1";

                comboStep = (triggerToFire == "HAttack1") ? 1 : 2;
                currentComboIsHeavy = true;
            }
            else
            {
                if (comboStep == 1 && !currentComboIsHeavy) triggerToFire = "Attack2";
                else if (comboStep == 2 && !currentComboIsHeavy) triggerToFire = "Attack3";
                else if (comboStep == 1 && currentComboIsHeavy) triggerToFire = "Attack3";
                else triggerToFire = "Attack1";

                if (triggerToFire == "Attack1") comboStep = 1;
                else if (triggerToFire == "Attack2") comboStep = 2;
                else comboStep = 3;

                currentComboIsHeavy = false;
            }

            if (stateInfo.IsName("Recovery")) animator.SetTrigger("RecoveryStop");
            ResetConflictingTriggers(triggerToFire);
            animator.SetTrigger(triggerToFire);
            currentAttackWasSprintAttack = false;
        }
    }

    private string ResolveSprintAttackTrigger(AnimatorStateInfo stateInfo, bool isLocked)
    {
        if (stateInfo.IsName("Fight_Sprint")) return "FightSprintAttack";
        if (stateInfo.IsName("Sprint")) return "SprintAttack";
        return isLocked ? "FightSprintAttack" : "SprintAttack";
    }


    private void BufferUnlockSprintAttack(bool heavy)
    {
        hasBufferedUnlockSprintAttack = true;
        bufferedUnlockSprintAttackHeavy = heavy;
        bufferedUnlockSprintAttackExpireAt = Time.time + unlockSprintAttackBufferDuration;
    }

    private bool HasSprintAttackSpeed()
    {
        return animator.GetFloat("Speed") > sprintAttackMinSpeed;
    }

    private void TryConsumeBufferedUnlockSprintAttack()
    {
        if (!hasBufferedUnlockSprintAttack) return;

        if (Time.time > bufferedUnlockSprintAttackExpireAt)
        {
            hasBufferedUnlockSprintAttack = false;
            return;
        }

        if (isAttacking) return;
        if (playerDodge != null && playerDodge.IsDodging) return;
        if (stamina != null && !stamina.CanPerformAction())
        {
            hasBufferedUnlockSprintAttack = false;
            return;
        }

        bool isLocked = lockOnBehaviour != null && lockOnBehaviour.IsLocked;
        if (isLocked)
        {
            hasBufferedUnlockSprintAttack = false;
            return;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isInFreeSprint = stateInfo.IsName("Sprint")
            || (animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).IsName("Sprint"));

        if (!isInFreeSprint || !HasSprintAttackSpeed()) return;

        nextAttackIsHeavy = bufferedUnlockSprintAttackHeavy;
        hasBufferedUnlockSprintAttack = false;
        PerformAttack();
    }
    private IEnumerator EnableMovementRoutine()
    {
        yield return new WaitForSeconds(movementReEnableDelay);
        if (playerMovement != null) playerMovement.SetMovementEnabled(true);
        movementDelayCoroutine = null;
    }

    public void OnAttackEnd()
    {
        if (attackFailsafe != null) StopCoroutine(attackFailsafe);

        isAttacking = false;
        allowInputQueuing = false;
        DisableWeaponHitbox();
        animator.SetBool("SprintAttackDelay", false);

        if (inputQueued)
        {
            PerformAttack();
        }
        else
        {
            bool isExcludedFromDelay = (comboStep == 3 && !currentComboIsHeavy) || (comboStep == 2 && currentComboIsHeavy);

            if (isExcludedFromDelay)
            {
                if (playerMovement != null) playerMovement.SetMovementEnabled(true);
                if (movementDelayCoroutine != null) StopCoroutine(movementDelayCoroutine);
            }
            else
            {
                if (movementDelayCoroutine != null) StopCoroutine(movementDelayCoroutine);
                movementDelayCoroutine = StartCoroutine(EnableMovementRoutine());
            }

            comboStep = 0;
            currentComboIsHeavy = false;

            bool shouldReturnDirectlyToIdle = currentAttackWasSprintAttack
                                              && (lockOnBehaviour == null || !lockOnBehaviour.IsLocked)
                                              && (playerMovement == null || !playerMovement.IsSprinting)
                                              && !HasSprintAttackSpeed();

            currentAttackWasSprintAttack = false;

            if (shouldReturnDirectlyToIdle)
            {
                ResetConflictingTriggers("SprintToIdle");
                animator.SetTrigger("SprintToIdle");
            }
            else
            {
                animator.SetTrigger("Recovery");
            }
        }
    }

    private void ResetConflictingTriggers(string nextTrigger)
    {
        if (nextTrigger != "Attack1") animator.ResetTrigger("Attack1");
        if (nextTrigger != "Attack2") animator.ResetTrigger("Attack2");
        if (nextTrigger != "Attack3") animator.ResetTrigger("Attack3");
        if (nextTrigger != "HAttack1") animator.ResetTrigger("HAttack1");
        if (nextTrigger != "HAttack2") animator.ResetTrigger("HAttack2");
        if (nextTrigger != "SprintAttack") animator.ResetTrigger("SprintAttack");
        if (nextTrigger != "FightSprintAttack") animator.ResetTrigger("FightSprintAttack");
        if (nextTrigger != "SprintToIdle") animator.ResetTrigger("SprintToIdle");
        if (nextTrigger != "Recovery") animator.ResetTrigger("Recovery");
        if (nextTrigger != "RecoveryStop") animator.ResetTrigger("RecoveryStop");
    }

    private void ResetCombatState()
    {
        isAttacking = false;
        inputQueued = false;
        comboStep = 0;
        currentComboIsHeavy = false;
        hasBufferedUnlockSprintAttack = false;
        currentAttackWasSprintAttack = false;
        if (playerMovement != null) playerMovement.SetMovementEnabled(true);
    }

    private void HandleUnlockDuringAttack()
    {
        if (animator == null) return;

        animator.SetBool("FightSprint", false);
        animator.SetBool("SprintAttackDelay", false);
        animator.ResetTrigger("FightSprintAttack");
        animator.ResetTrigger("SprintAttack");
        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
        animator.ResetTrigger("Attack3");
        animator.ResetTrigger("HAttack1");
        animator.ResetTrigger("HAttack2");
        animator.ResetTrigger("SprintToIdle");
        animator.ResetTrigger("Recovery");
        animator.ResetTrigger("RecoveryStop");

        if (!isAttacking) return;

        animator.CrossFadeInFixedTime("Idle", 0.1f, 0);
    }

    private bool TryExecuteBoss()
    {
        if (lockOnBehaviour != null && lockOnBehaviour.IsLocked)
        {
            Transform targetEnemy = lockOnBehaviour.GetCurrentTarget();
            if (targetEnemy != null)
            {
                BossHealth bossHealth = targetEnemy.GetComponentInParent<BossHealth>();

                if (bossHealth != null && bossHealth.IsExecutable)
                {
                    float distance = Vector3.Distance(transform.position, targetEnemy.position);
                    if (distance <= maxExecutionDistance)
                    {
                        Animator wilkolakAnimator = targetEnemy.GetComponentInParent<Animator>();
                        if (wilkolakAnimator != null)
                        {
                            // Odpalamy egzekucję!
                            bossHealth.ConfirmExecution();
                            ExecutionManager.Instance.StartExecution(animator, wilkolakAnimator);

                            // Czyścimy stan walki, żeby postać nie próbowała w tle machać mieczem
                            ResetCombatState();
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator AttackFailsafeRoutine()
    {
        yield return new WaitForSeconds(2.0f);
        if (isAttacking) OnAttackEnd();
    }

    public void EnableAttackQueue() => allowInputQueuing = true;
    public void EnableWeaponHitbox() { if (weaponScript != null) weaponScript.EnableDamage(); }
    public void DisableWeaponHitbox() { if (weaponScript != null) weaponScript.DisableDamage(); }

    private void Update()
    {
        TryConsumeBufferedUnlockSprintAttack();
    }
}