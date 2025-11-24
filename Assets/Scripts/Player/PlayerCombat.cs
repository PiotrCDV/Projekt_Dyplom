using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public PlayerMovement playerMovement; 

    [Header("Combat Settings")]
    public float attackCooldown = 0.5f; 
    private float lastAttackTime = -Mathf.Infinity;

    private InputSystem_Actions inputActions;
    private LockOnBehaviour lockOnBehaviour; 

    private void Awake()
    {
        inputActions = new InputSystem_Actions();

        inputActions.Player.Attack.performed += ctx => PerformAttack();

        if (lockOnBehaviour == null) 
        {
            lockOnBehaviour = GetComponent<LockOnBehaviour>();
        }
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void PerformAttack()
    {
        if (lockOnBehaviour == null || !lockOnBehaviour.IsLocked) 
        {
            return;
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            animator.SetTrigger("Attack1");
            lastAttackTime = Time.time;
        }
    }

    public void OnAttackStart()
    {
        if (playerMovement != null) playerMovement.SetMovementEnabled(false);
    }

    public void OnAttackEnd()
    {
        if (playerMovement != null) playerMovement.SetMovementEnabled(true);
    }
}