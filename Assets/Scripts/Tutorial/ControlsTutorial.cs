using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class ControlsTutorial : MonoBehaviour
{
    [Header("UI References")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI keyboardText;
    public TextMeshProUGUI gamepadText;

    [Header("Settings")]
    public float displayDuration = 30f;
    [Tooltip("Minimalne wychylenie analoga (0-1), aby wykryæ pada. Zapobiega przypadkowemu prze³¹czaniu.")]
    public float analogThreshold = 0.2f;

    private bool isUsingGamepad = false;

    private void Start()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
        UpdateUI();
        StartCoroutine(HidePanelAfterTime());
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
        if (change == InputActionChange.ActionPerformed)
        {
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
                    UpdateUI();
                }
            }
            else if (lastDevice is Keyboard || lastDevice is Mouse)
            {
                if (isUsingGamepad)
                {
                    isUsingGamepad = false;
                    UpdateUI();
                }
            }
        }
    }

    private void UpdateUI()
    {
        if (keyboardText == null || gamepadText == null) return;
        keyboardText.gameObject.SetActive(!isUsingGamepad);
        gamepadText.gameObject.SetActive(isUsingGamepad);
    }

    private IEnumerator HidePanelAfterTime()
    {
        yield return new WaitForSeconds(displayDuration);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        this.enabled = false;
    }
}