using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public CharacterController controller;

    [Header("Animation")]
    public Animator animator;

    private Vector2 moveInput;

    private InputSystem_Actions inputActions;

    private Transform camTransform;
    private Vector3 camForward;
    private Vector3 camRight;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
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
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleCamera();
        HandleMovementAndAnimation();
    }

    private void HandleCamera()
    {
        camTransform = Camera.main.transform;

        camForward = camTransform.forward;
        camRight = camTransform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();
    }

    private void HandleMovementAndAnimation()
    {
        Vector3 move = camForward * moveInput.y + camRight * moveInput.x;

        if (move.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        controller.Move(move * moveSpeed * Time.deltaTime);

        if (animator != null)
            animator.SetFloat("Speed", move.magnitude);
    }
}