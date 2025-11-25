using Unity.Cinemachine;
using Commands;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public CharacterController controller;
    public CinemachineCamera vcamFreeLook;

    [Header("Animation")]
    public Animator animator;

    // Zmienne prywatne
    private Vector2 moveInput;
    private InputSystem_Actions inputActions;
    private Camera mainCamera;
    private Vector3 camForward;
    private Vector3 camRight;
    private bool canMove = true;

    private LockOnBehaviour lockOnBehaviour;
    private CinemachineOrbitalFollow orbitalFollow;

    // Flagi do opóŸnienia przejœcia z lock-on na freelook
    private bool keepLockOnRotation = false;
    private float keepLockOnTimer = 0f;
    private const float keepLockOnDuration = 0.4f; // czas opóŸnienia w sekundach

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        orbitalFollow = vcamFreeLook.GetComponent<CinemachineOrbitalFollow>();

        lockOnBehaviour = GetComponent<LockOnBehaviour>();
        inputActions.Player.LockOn.performed += ctx =>
        {
            bool wasLocked = lockOnBehaviour.IsLocked;
            lockOnBehaviour.ToggleLockOn();
            // OpóŸnienie przy rêcznym wy³¹czeniu
            if (wasLocked && !lockOnBehaviour.IsLocked)
            {
                keepLockOnRotation = true;
                keepLockOnTimer = keepLockOnDuration;
            }
        };

        // OpóŸnienie przy automatycznym wy³¹czeniu (np. wyjœcie z zasiêgu)
        lockOnBehaviour.OnUnlock += () =>
        {
            keepLockOnRotation = true;
            keepLockOnTimer = keepLockOnDuration;
        };
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

        // Obs³uga opóŸnienia rotacji po wy³¹czeniu lock-on
        if (keepLockOnRotation)
        {
            keepLockOnTimer -= Time.deltaTime;
            if (keepLockOnTimer <= 0f)
            {
                keepLockOnRotation = false;
            }
        }

        HandleMovementAndAnimation();
    }

    private void LateUpdate()
    {
        var lockOn = lockOnBehaviour.vcamLockOn;

        if (lockOnBehaviour.IsLocked && lockOn != null && orbitalFollow != null)
        {
            float targetYaw = lockOn.transform.eulerAngles.y;
            orbitalFollow.HorizontalAxis.Value = targetYaw;

            orbitalFollow.VerticalAxis.Value = Mathf.Clamp(10f, -10f, 45f);
        }
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

    public void SetMovementEnabled(bool state)
    {
        canMove = state;
        if (!canMove)
        {

            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
                animator.SetFloat("MoveX", 0f);
                animator.SetFloat("MoveY", 0f);
            }
        }
    }
    private void HandleMovementAndAnimation()
    {
        if (!canMove) return;

        Vector3 move = camForward * moveInput.y + camRight * moveInput.x;

        controller.Move(move * moveSpeed * Time.deltaTime);

        bool isLocked = lockOnBehaviour.IsLocked && lockOnBehaviour.GetCurrentTarget() != null;

        if (isLocked || keepLockOnRotation)
        {
            Transform target = lockOnBehaviour.GetCurrentTarget();
            if (target != null)
            {
                Vector3 lookDir = target.position - transform.position;
                lookDir.y = 0;
                if (lookDir != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDir.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);
                }
            }
        }
        else
        {
            if (move.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(move.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }

        if (animator != null)
        {
         //   animator.SetBool("isLocked", isLocked);

            if (isLocked)
            {
    
                Vector3 localMove = transform.InverseTransformDirection(move);

                float strafeSpeed = moveSpeed > 0 ? moveSpeed : 1f;

                animator.SetFloat("MoveX", localMove.x, 0.1f, Time.deltaTime);
                animator.SetFloat("MoveY", localMove.z, 0.1f, Time.deltaTime); 
            }
            else
            {
                animator.SetFloat("Speed", move.magnitude, 0.1f, Time.deltaTime);

                animator.SetFloat("MoveX", 0f);
                animator.SetFloat("MoveY", 0f);
            }
        }
    }
    [Command("setspeed", "speed")]
    public void SetSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Max(0f, newSpeed);
    }
}