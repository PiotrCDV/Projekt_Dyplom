using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    private Animator animator;
    private PlayerMovement playerMovement;
    private LockOnBehaviour lockOnBehaviour;
    private SwordDamage weaponScript;
    private PlayerDodge playerDodge;
    private PlayerStamina stamina;

    [Header("Combat Settings")]
    public float attackStaminaCost = 15f;
    public float sprintAttackStaminaCost = 20f;

    [Header("Combat State")]
    private int comboStep = 0;
    private bool isAttacking = false;
    private bool inputQueued = false;
    private bool allowInputQueuing = false;

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
        inputActions.Player.Attack.performed += ctx => HandleAttackInput();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void HandleAttackInput()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isInTransition = animator.IsInTransition(0);
        if (isInTransition) stateInfo = animator.GetNextAnimatorStateInfo(0);

        if (stateInfo.IsTag("NoCombat")) return;

        bool isLocked = lockOnBehaviour != null && lockOnBehaviour.IsLocked;
        bool isSprintingInput = playerMovement != null && playerMovement.IsSprinting;

        if (!isLocked && !isSprintingInput) return;
        if (playerDodge != null && playerDodge.IsDodging) return;
        if (stamina != null && !stamina.CanPerformAction()) return;

        if (isAttacking)
        {
            if (allowInputQueuing) inputQueued = true;
            return;
        }

        PerformAttack();
    }

    private void PerformAttack()
    {
        animator.ResetTrigger("Recovery");
        animator.ResetTrigger("RecoveryStop");
        animator.ResetTrigger("FightSprintAttack");
        animator.ResetTrigger("SprintAttack");

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isSprintingInAnimator = stateInfo.IsName("Sprint") || stateInfo.IsName("Fight_Sprint");
        bool isSprintingInput = playerMovement != null && playerMovement.IsSprinting;
        bool isLocked = lockOnBehaviour != null && lockOnBehaviour.IsLocked;

        bool shouldDoSprintAttack = isSprintingInput && isSprintingInAnimator;

        if (!isLocked && !shouldDoSprintAttack)
        {
            if (playerMovement != null) playerMovement.SetMovementEnabled(true);
            return;
        }

        isAttacking = true;
        inputQueued = false;
        allowInputQueuing = false;

        if (playerMovement != null) playerMovement.SetMovementEnabled(false);

        float currentCost = shouldDoSprintAttack ? sprintAttackStaminaCost : attackStaminaCost;
        if (stamina != null) stamina.UseStamina(currentCost);

        if (shouldDoSprintAttack)
        {
            animator.SetBool("SprintAttackDelay", true); 
            if (isLocked) animator.SetTrigger("FightSprintAttack");
            else animator.SetTrigger("SprintAttack");
            comboStep = 0;
            return;
        }

        comboStep++;
        if (comboStep > 3) comboStep = 1;

        if (stateInfo.IsName("Recovery")) animator.SetTrigger("RecoveryStop");
        else animator.SetTrigger("Attack" + comboStep);
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
        allowInputQueuing = false;
        DisableWeaponHitbox();
        animator.SetBool("SprintAttackDelay", false);

        if (inputQueued) PerformAttack();
        else
        {
            AnimatorStateInfo currentInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool isSprintAttack = currentInfo.IsName("Fight_Sprint_Light_Attack") || currentInfo.IsName("Sprint_Light_Attack");
            if (isSprintAttack)
            {
                if (playerMovement != null) playerMovement.SetMovementEnabled(true);
            }
            else
            {
                animator.SetTrigger("Recovery");
                comboStep = 0;
                if (playerMovement != null) playerMovement.SetMovementEnabled(true);
            }
        }
    }

    public void EnableAttackQueue() => allowInputQueuing = true;
    public void EnableWeaponHitbox() { if (weaponScript != null) weaponScript.EnableDamage(); }
    public void DisableWeaponHitbox() { if (weaponScript != null) weaponScript.DisableDamage(); }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
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
                        Debug.LogWarning("Znalaz³em cel LockOn, ale nie znalaz³em na nim Animatora!");
                    }
                }
            }
        }
    }
}