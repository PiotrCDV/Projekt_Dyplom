using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDodge : MonoBehaviour
{
    [Header("Ustawienia Uniku")]
    public float dodgeSlideDuration = 0.3f;
    public float dodgeSpeed = 12f;
    public float dodgeStaminaCost = 20f;

    public bool IsDodging { get; private set; } = false;

    private InputSystem_Actions inputActions;
    private Animator animator;
    private CharacterController controller;
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    private PlayerStamina stamina;

    private Vector3 savedDodgeDirection;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
        stamina = GetComponent<PlayerStamina>();

        inputActions = new InputSystem_Actions();

        inputActions.Player.Dodge.performed += ctx =>
        {
            if (ctx.control.device is Keyboard)
            {
                PrepareDodge();
            }
        };
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    public void PrepareDodge()
    {
        if (IsDodging) return;
        if (playerCombat != null && playerCombat.IsAttacking) return;
        if (stamina != null && !stamina.CanPerformAction()) return;

        if (stamina != null) stamina.UseStamina(dodgeStaminaCost);

        IsDodging = true;

        if (playerMovement != null) playerMovement.SetMovementEnabled(false);

        CalculateDodgeDirection();

        if (savedDodgeDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(savedDodgeDirection);
        }

        if (animator != null) animator.SetTrigger("Dodge");
    }

    public void StartDodge()
    {
        if (!IsDodging) return;
        StartCoroutine(DodgeSlideRoutine());
    }

    public void FinishDodge()
    {
        if (playerMovement != null) playerMovement.SetMovementEnabled(true);
        IsDodging = false;
        StopAllCoroutines();
    }

    private IEnumerator DodgeSlideRoutine()
    {
        float timer = 0f;
        while (timer < dodgeSlideDuration)
        {
            controller.Move(savedDodgeDirection * dodgeSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void CalculateDodgeDirection()
    {
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();

        if (input.magnitude > 0.1f)
        {
            Transform camTransform = Camera.main.transform;
            Vector3 camForward = camTransform.forward;
            Vector3 camRight = camTransform.right;

            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            savedDodgeDirection = camForward * input.y + camRight * input.x;
            savedDodgeDirection.Normalize();
        }
        else
        {
            savedDodgeDirection = transform.forward;
        }
    }
}