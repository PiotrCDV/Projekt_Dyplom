using Unity.Cinemachine;
using Commands;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Stats")]
    public float walkSpeed = 2f;  
    public float runSpeed = 5f;     
    public float sprintSpeed = 8f; 

    [SerializeField] private float currentSpeed;

    public CharacterController controller;
    public CinemachineCamera vcamFreeLook;

    [Header("Animation")]
    public Animator animator;

    private Vector2 moveInput;
    private bool isSprinting;

    private InputSystem_Actions inputActions;
    private Camera mainCamera;
    private Vector3 camForward;
    private Vector3 camRight;
    private bool canMove = true;

    private LockOnBehaviour lockOnBehaviour;
    private CinemachineOrbitalFollow orbitalFollow;

    private bool keepLockOnRotation = false;
    private float keepLockOnTimer = 0f;
    private const float keepLockOnDuration = 0.4f;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Sprint.performed += ctx => isSprinting = true;
        inputActions.Player.Sprint.canceled += ctx => isSprinting = false;

        orbitalFollow = vcamFreeLook.GetComponent<CinemachineOrbitalFollow>();
        lockOnBehaviour = GetComponent<LockOnBehaviour>();

      
        if (lockOnBehaviour != null)
        {
            inputActions.Player.LockOn.performed += ctx => lockOnBehaviour.ToggleLockOn();
        }
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();
    private void Start()
    {
        CommandManager.Instance.RegisterInstance(this);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        mainCamera = Camera.main;
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
        if (!canMove && animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveY", 0f);
        }
    }

    void Update()
    {
        HandleCamera();
        if (lockOnBehaviour) lockOnBehaviour.HandleLockOnState(transform.position);

        if (keepLockOnRotation)
        {
            keepLockOnTimer -= Time.deltaTime;
            if (keepLockOnTimer <= 0f) keepLockOnRotation = false;
        }

        HandleMovementAndAnimation();
    }

private void HandleMovementAndAnimation()
    {
        if (!canMove) return;

        float targetSpeed = 0f;
        float inputMagnitude = moveInput.magnitude;

        if (inputMagnitude > 0.1f)
        {
            if (isSprinting) targetSpeed = sprintSpeed;
            else if (inputMagnitude >= 0.6f) targetSpeed = runSpeed;
            else targetSpeed = walkSpeed;
        }

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, 10f * Time.deltaTime);
        
        Vector3 move = camForward * moveInput.y + camRight * moveInput.x;
        if (move.magnitude > 1f) move.Normalize();

        controller.Move(move * currentSpeed * Time.deltaTime);

        bool isLocked = lockOnBehaviour.IsLocked && lockOnBehaviour.GetCurrentTarget() != null;

        if (animator != null)
        {

            bool fightSprintActive = isLocked && isSprinting && inputMagnitude > 0.1f;
            
            animator.SetBool("FightSprint", fightSprintActive);

            float animValue = 0f;
            if (inputMagnitude > 0.1f)
            {
                if (isSprinting) animValue = 1.5f;      
                else if (inputMagnitude >= 0.6f) animValue = 1.0f;
                else animValue = 0.5f;                
            }

            if (isLocked)
            {
         
                Vector3 localMove = transform.InverseTransformDirection(move);
                
                float multiplier = isSprinting ? 1.5f : 1f;

                animator.SetFloat("MoveX", localMove.x * multiplier, 0.1f, Time.deltaTime);
                animator.SetFloat("MoveY", localMove.z * multiplier, 0.1f, Time.deltaTime);
            }
            else
            {
                animator.SetFloat("Speed", animValue, 0.1f, Time.deltaTime);
                animator.SetFloat("MoveX", 0f);
                animator.SetFloat("MoveY", 0f);
            }
        }

 
        if ((isLocked || keepLockOnRotation) && !isSprinting)
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
        else if (move.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }

}