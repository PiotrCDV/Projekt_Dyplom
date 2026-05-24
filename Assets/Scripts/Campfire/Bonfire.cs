using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Bonfire : MonoBehaviour
{
    [Header("Referencje Wizualne")]
    public ParticleSystem fireParticles;
    public Light glowLight;

    [Header("UI Interakcji")]
    public GameObject interactUI;
    [Tooltip("Ikona dla myszy/klawiatury (LPM)")]
    public Sprite mouseIcon;
    [Tooltip("Ikona dla pada")]
    public Sprite gamepadIcon;
    [Tooltip("Próg wychylenia analoga, powyżej którego uznajemy użycie pada")]
    public float analogThreshold = 0.2f;

    [Header("Ustawienia")]
    public KeyCode interactKey = KeyCode.E;
    public Transform spawnPoint;

    private static bool isGlobalBonfireLit = false;
    private bool playerInRange = false;
    private bool isUsingGamepad = false;

    void Start()
    {
        if (interactUI != null) interactUI.SetActive(false);

        UpdateIconImmediate();

        if (isGlobalBonfireLit)
        {
            ApplyLitState();
        }
        else
        {
            if (fireParticles != null) fireParticles.Stop();
            if (glowLight != null) glowLight.enabled = false;
        }
    }

    private void OnEnable()
    {
        InputSystem.onActionChange += OnActionChange;
    }

    private void OnDisable()
    {
        InputSystem.onActionChange -= OnActionChange;
    }

    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change != InputActionChange.ActionPerformed) return;

        var inputAction = (InputAction)obj;
        var lastDevice = inputAction.activeControl.device;

        if (lastDevice is Gamepad)
        {
            if (inputAction.activeValueType == typeof(Vector2))
            {
                Vector2 stickValue = inputAction.ReadValue<Vector2>();
                if (stickValue.magnitude < analogThreshold) return;
            }

            if (!isUsingGamepad)
            {
                isUsingGamepad = true;
                UpdateIconImmediate();
            }
        }
        else if (lastDevice is Keyboard || lastDevice is Mouse)
        {
            if (isUsingGamepad)
            {
                isUsingGamepad = false;
                UpdateIconImmediate();
            }
        }
    }

    private void UpdateIconImmediate()
    {
        if (interactUI == null) return;

        var image = interactUI.GetComponentInChildren<Image>();
        if (image == null) return;

        image.sprite = isUsingGamepad ? gamepadIcon : mouseIcon;
    }

    void Update()
    {
        if (playerInRange && !isGlobalBonfireLit && Input.GetKeyDown(interactKey))
        {
            LightBonfire();
        }
    }

    private void LightBonfire()
    {
        isGlobalBonfireLit = true;
        ApplyLitState();

        if (interactUI != null) interactUI.SetActive(false);

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;
        
        PlayerHealth.UpdateCheckpoint(pos, rot);
        
        Debug.Log("OGNISKO ZAPALONE NA STAŁE!");
    }

    private void ApplyLitState()
    {
        if (fireParticles != null) fireParticles.Play();
        if (glowLight != null) glowLight.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactUI != null && !isGlobalBonfireLit)
            {
                UpdateIconImmediate();
                interactUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactUI != null) interactUI.SetActive(false);
        }
    }
}