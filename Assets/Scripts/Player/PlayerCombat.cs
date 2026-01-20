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

    [Header("Combo Settings")]
    public float comboGraceTime = 2.0f;

    private int comboStep = 0;
    private bool isAttacking = false;
    private float lastAttackEndTime = 0f;

    private InputSystem_Actions inputActions;

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
        if (playerDodge != null && playerDodge.IsDodging) return;

        if (lockOnBehaviour == null || !lockOnBehaviour.IsLocked) return;

        if (isAttacking) return;

        PerformAttack();
    }

    private void PerformAttack()
    {
        if (comboStep > 0 && Time.time - lastAttackEndTime <= comboGraceTime)
        {
            comboStep++;
            if (comboStep > 3) comboStep = 1;
        }
        else
        {
            comboStep = 1;
        }

        isAttacking = true;

        if (playerMovement != null) playerMovement.SetMovementEnabled(false);

        animator.SetTrigger("Attack" + comboStep);
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

        if (playerMovement != null) playerMovement.SetMovementEnabled(true);

        DisableWeaponHitbox();

        lastAttackEndTime = Time.time;
    }

    private void Update()
    {
        if (!isAttacking && comboStep > 0)
        {
            if (Time.time - lastAttackEndTime > comboGraceTime)
            {
                comboStep = 0;
            }
        }
    }
}