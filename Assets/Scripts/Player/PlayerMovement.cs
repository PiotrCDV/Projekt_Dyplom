using Unity.Cinemachine;
using Commands;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public CharacterController controller;

    [Header("Animation")]
    public Animator animator;

    // Zmienne prywatne
    private Vector2 moveInput;
    private InputSystem_Actions inputActions;
    private Camera mainCamera;
    private Vector3 camForward;
    private Vector3 camRight;

    private LockOnBehaviour lockOnBehaviour;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        lockOnBehaviour = GetComponent<LockOnBehaviour>();
        inputActions.Player.LockOn.performed += ctx => lockOnBehaviour.ToggleLockOn();
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Start()
    {
        CommandManager.Instance.RegisterInstance(this);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleCamera();
        lockOnBehaviour.HandleLockOnState(transform.position);
        HandleMovementAndAnimation(); // G³ówna logika ruchu
    }

    private void HandleCamera()
    {
        Transform camTransform = mainCamera.transform;

        camForward = camTransform.forward;
        camRight = camTransform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();
    }

    // --- ZMIANA DLA TESTU "BEZ STRAFE" ---
    private void HandleMovementAndAnimation()
    {
        Vector3 move = camForward * moveInput.y + camRight * moveInput.x;
        controller.Move(move * moveSpeed * Time.deltaTime);

        if (lockOnBehaviour.IsLocked && lockOnBehaviour.GetCurrentTarget() != null)
        {
            // --- TRYB LOCK-ON ---
            Vector3 lookDir = lockOnBehaviour.GetCurrentTarget().position - transform.position;
            lookDir.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(lookDir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
        else
        {
            // --- TRYB FREELOOK ---
            if (move.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(move.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }

        if (animator != null)
            animator.SetFloat("Speed", move.magnitude);
    }

    [Command("setspeed", "speed")]
    public void SetSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Max(0f, newSpeed);
    }
}