using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BossGateController : MonoBehaviour
{
    [Header("Referencje")]
    public Renderer gateRenderer;
    public Collider blockingCollider;

    [Header("Obiekty powiązane z bramą")]
    public GameObject linkedObject;

    [Header("UI Interakcji (World Space)")]
    [Tooltip("Przeci�gnij tutaj obiekt Canvas unocz�cy si� nad bram�")]
    public GameObject interactUI;
    [Tooltip("Ikona dla myszy/klawiatury (LPM)")]
    public Sprite mouseIcon;
    [Tooltip("Ikona dla pada")]
    public Sprite gamepadIcon;
    [Tooltip("Próg wychylenia analoga, powyżej którego uznajemy użycie pada")]
    public float analogThreshold = 0.2f;

    [Header("Ustawienia")]
    public KeyCode interactKey = KeyCode.F;
    public string dissolveParam = "_DissolveAmount";
    public float fadeDuration = 1.5f;

    private Material gateMaterial;
    private bool isOpened = false;
    private bool isPlayerInside = false;
    private bool playerInRange = false;
    private bool isUsingGamepad = false;

    void Start()
    {
        if (gateRenderer != null)
            gateMaterial = gateRenderer.material;

        if (gateMaterial != null)
            gateMaterial.SetFloat(dissolveParam, 0f);

        if (interactUI != null) interactUI.SetActive(false);
        UpdateIconImmediate();

        if (linkedObject != null) linkedObject.SetActive(true);

        if (blockingCollider != null) blockingCollider.enabled = true;
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
        if (playerInRange && !isOpened && !isPlayerInside)
        {
            if (IsInteractPressedThisFrame())
            {
                OpenGate();
            }
        }
    }

    private bool IsInteractPressedThisFrame()
    {
        return Input.GetKeyDown(interactKey)
            || Input.GetMouseButtonDown(0)
            || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
    }

    public void OpenGate()
    {
        isOpened = true;
        if (interactUI != null) interactUI.SetActive(false);
        if (linkedObject != null) linkedObject.SetActive(false);
        StartCoroutine(FadeDissolve(0f, 1f));
        if (blockingCollider != null) blockingCollider.enabled = false;
    }

    public void CloseGate()
    {
        if (!isOpened || isPlayerInside)
            return;

        isPlayerInside = true;
        isOpened = false;
        playerInRange = false;
        if (interactUI != null) interactUI.SetActive(false);
        if (linkedObject != null) linkedObject.SetActive(true);
        StartCoroutine(FadeDissolve(1f, 0f));
        if (blockingCollider != null) blockingCollider.enabled = true;

        if (BossHealth.Instance != null)
        {
            BossHealth.Instance.ShowHealthBar();
        }
    }

    private IEnumerator FadeDissolve(float start, float end)
    {
        if (gateMaterial == null)
            yield break;

        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float current = Mathf.Lerp(start, end, elapsed / fadeDuration);
            gateMaterial.SetFloat(dissolveParam, current);
            yield return null;
        }
        gateMaterial.SetFloat(dissolveParam, end);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpened && !isPlayerInside)
        {
            playerInRange = true;
            if (interactUI != null) interactUI.SetActive(true);
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