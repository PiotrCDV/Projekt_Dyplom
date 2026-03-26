using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image staminaFillImage;

    [Header("Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float regenRate = 15f;
    [SerializeField] private float regenDelay = 1.2f;

    private float currentStamina;
    private float regenTimer;

    public float CurrentStamina => currentStamina;

    private void Awake()
    {
        currentStamina = maxStamina;
        UpdateUI();
    }

    private void Update()
    {
        if (regenTimer > 0)
        {
            regenTimer -= Time.deltaTime;
        }
        else if (currentStamina < maxStamina)
        {
            currentStamina += regenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
            UpdateUI();
        }
    }

    public bool CanPerformAction()
    {
        return currentStamina > 0;
    }

    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Max(currentStamina, 0);
        regenTimer = regenDelay;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (staminaFillImage != null)
        {
            staminaFillImage.fillAmount = currentStamina / maxStamina;
        }
    }
}