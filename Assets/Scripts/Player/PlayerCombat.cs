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

    public float lightAttackStaminaCost = 15f;
    public float heavyAttackStaminaCost = 25f;
    public float sprintAttackStaminaCost = 20f;

    private int comboStep = 0;
    private bool isAttacking = false;
    private bool inputQueued = false;
    private bool allowInputQueuing = false;
    private bool nextAttackIsHeavy = false;
    private bool currentComboIsHeavy = false;

    private Coroutine attackFailsafe;
    private InputSystem_Actions inputActions;
    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        lockOnBehaviour = GetComponent<LockOnBehaviour>();
        weaponScript = GetComponentInChildren<SwordDamage>();
        playerDodge = GetComponent<PlayerDodge>();
        stamina = GetComponent<PlayerStamina>();

        inputActions = new InputSystem_Actions();
        inputActions.Player.Attack.performed += ctx => HandleAttackInput(false);
        inputActions.Player.HeavyAttack.performed += ctx => HandleAttackInput(true);
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void HandleAttackInput(bool heavy)
    {
        if (playerDodge != null && playerDodge.IsDodging) return;
        if (stamina != null && !stamina.CanPerformAction()) return;

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
        bool isLocked = lockOnBehaviour != null && lockOnBehaviour.IsLocked;
        bool isSprintingInput = playerMovement != null && playerMovement.IsSprinting;
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isSprintingInAnimator = stateInfo.IsName("Sprint") || stateInfo.IsName("Fight_Sprint");
        bool shouldDoSprintAttack = isSprintingInput && isSprintingInAnimator;

        if (!isLocked && !shouldDoSprintAttack)
        {
            ResetCombatState();
            return;
        }

        ResetAllAttackTriggers();

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
            if (isLocked) animator.SetTrigger("FightSprintAttack");
            else animator.SetTrigger("SprintAttack");
            comboStep = 0;
            currentComboIsHeavy = false;
        }
        else
        {
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
            animator.SetTrigger(triggerToFire);
        }
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
            comboStep = 0;
            currentComboIsHeavy = false;
            if (playerMovement != null) playerMovement.SetMovementEnabled(true);
            animator.SetTrigger("Recovery");
        }
    }

    private void ResetAllAttackTriggers()
    {
        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
        animator.ResetTrigger("Attack3");
        animator.ResetTrigger("HAttack1");
        animator.ResetTrigger("HAttack2");
        animator.ResetTrigger("SprintAttack");
        animator.ResetTrigger("FightSprintAttack");
        animator.ResetTrigger("Recovery");
        animator.ResetTrigger("RecoveryStop");
    }

    private void ResetCombatState()
    {
        isAttacking = false;
        inputQueued = false;
        comboStep = 0;
        currentComboIsHeavy = false;
        if (playerMovement != null) playerMovement.SetMovementEnabled(true);
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
        if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.JoystickButton4))
        {
            if (lockOnBehaviour != null && lockOnBehaviour.IsLocked)
            {
                Transform targetEnemy = lockOnBehaviour.GetCurrentTarget();

                if (targetEnemy != null)
                {
                    Animator wilkolakAnimator = targetEnemy.GetComponentInParent<Animator>();

                    if (wilkolakAnimator != null)
                    {
                        ExecutionManager.Instance.StartExecution(animator, wilkolakAnimator);
                    }
                    else
                    {
                        Debug.LogWarning("Znalaz�em cel LockOn, ale nie znalaz�em na nim Animatora!");
                    }
                }
            }
        }
    }
}