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

        inputActions = new InputSystem_Actions();
        inputActions.Player.Attack.performed += ctx => HandleAttackInput();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void HandleAttackInput()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsTag("Combat")) return;

        if (stateInfo.IsTag("NoCombat")) return;

        if (playerDodge != null && playerDodge.IsDodging) return;

        if (lockOnBehaviour == null || !lockOnBehaviour.IsLocked) return;

        if (isAttacking)
        {
            if (allowInputQueuing)
            {
                inputQueued = true;
            }
            return;
        }

        PerformAttack();
    }

    private void PerformAttack()
    {
        comboStep++;
        if (comboStep > 3) comboStep = 1;

        isAttacking = true;
        inputQueued = false;
        allowInputQueuing = false;

        if (playerMovement != null) playerMovement.SetMovementEnabled(false);

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Recovery"))
        {

            animator.SetTrigger("RecoveryStop");

        }
        else 
        {

            animator.SetTrigger("Attack" + comboStep);
        }
    }

    public void EnableAttackQueue()
    {
        allowInputQueuing = true;
    }

    public void EnableWeaponHitbox()
    {
        if (weaponScript != null) weaponScript.EnableDamage();
    }

    public void DisableWeaponHitbox()
    {
        if (weaponScript != null) weaponScript.DisableDamage();
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
        allowInputQueuing = false;
        DisableWeaponHitbox();

        if (inputQueued)
        {
            PerformAttack();
        }
        else
        {
            animator.SetTrigger("Recovery");
            comboStep = 0;

            if (playerMovement != null) playerMovement.SetMovementEnabled(true);
        }
    }
}