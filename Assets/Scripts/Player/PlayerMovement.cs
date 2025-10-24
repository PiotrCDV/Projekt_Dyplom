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

    [Header("Lock-On System")]
    // Przeci¹gnij tu swoj¹ kamerê vcam_LockOn z Hierarchii
    public CinemachineCamera vcamLockOn;
    public float maxLockOnDistance = 20f;
    public LayerMask enemyLayer;

    // Zmienne prywatne
    private Vector2 moveInput;
    private InputSystem_Actions inputActions;
    private Camera mainCamera;
    private Vector3 camForward;
    private Vector3 camRight;

    private Transform currentTarget;
    private bool isLocked = false;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        inputActions.Player.LockOn.performed += ctx => ToggleLockOn();
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
        HandleLockOnState();
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
        // 1. Oblicz wektor ruchu (tak jak w twoim starym skrypcie)
        Vector3 move = camForward * moveInput.y + camRight * moveInput.x;

        // 2. Zastosuj ruch (tak jak w twoim starym skrypcie)
        controller.Move(move * moveSpeed * Time.deltaTime);

        // 3. Rotacja
        if (isLocked && currentTarget != null)
        {
            // --- TRYB LOCK-ON ---
            // Postaæ zawsze patrzy na cel
            Vector3 lookDir = currentTarget.position - transform.position;
            lookDir.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(lookDir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
        else
        {
            // --- TRYB FREELOOK ---
            // Postaæ obraca siê w kierunku ruchu (tak jak w twoim starym skrypcie)
            if (move.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(move.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }

        // 4. Animacja (tak jak w twoim starym skrypcie)
        // Dzia³a tak samo w obu trybach - u¿ywa parametru "Speed"
        if (animator != null)
            animator.SetFloat("Speed", move.magnitude);
    }

    // --- LOGIKA LOCK-ON (Bez zmian) ---

    private void ToggleLockOn()
    {
        Debug.Log("ToggleLockOn WYWO£ANE!");
        if (isLocked)
        {
            UnlockTarget();
        }
        else
        {
            TryLockOnTarget();
        }
    }

    private void HandleLockOnState()
    {
        if (isLocked && currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.position);

            if (distance > maxLockOnDistance || !currentTarget.gameObject.activeInHierarchy)
            {
                UnlockTarget();
            }
        }
    }

    private void TryLockOnTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, maxLockOnDistance, enemyLayer);
        Transform bestTarget = null;
        float minDistanceToScreenCenter = float.MaxValue;

        foreach (Collider collider in colliders)
        {
            Vector3 screenPoint = mainCamera.WorldToViewportPoint(collider.transform.position);

            if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
            {
                float distanceToCenter = Vector2.Distance(new Vector2(screenPoint.x, screenPoint.y), new Vector2(0.5f, 0.5f));
                if (distanceToCenter < minDistanceToScreenCenter)
                {
                    minDistanceToScreenCenter = distanceToCenter;
                    bestTarget = collider.transform;
                }
            }
        }

        if (bestTarget != null)
        {
            LockOn(bestTarget);
        }
    }

    private void LockOn(Transform target)
    {
        currentTarget = target;
        isLocked = true;

        vcamLockOn.LookAt = currentTarget;
        animator.SetBool("isLockedOn", true);
    }

    private void UnlockTarget()
    {
        currentTarget = null;
        isLocked = false;

        vcamLockOn.LookAt = null;
        animator.SetBool("isLockedOn", false);
    }

    // Twój stary kod konsoli - zostaje bez zmian
    [Command("setspeed", "speed")]
    public void SetSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Max(0f, newSpeed);
    }
}