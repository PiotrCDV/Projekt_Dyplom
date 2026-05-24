using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerStamina : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image staminaFillImage;
    [SerializeField] private Image staminaTrailImage;

    [Header("Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float regenRate = 15f;
    [SerializeField] private float regenDelay = 1.2f;

    [Header("Trail Effect")]
    [SerializeField] private float trailDelayTime = 0.5f;
    [SerializeField] private float trailDrainSpeed = 2f;

    private float currentStamina;
    private float regenTimer;
    private Coroutine trailCoroutine;

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
        if (trailCoroutine != null)
        {
            StopCoroutine(trailCoroutine);
        }

        currentStamina -= amount;
        currentStamina = Mathf.Max(currentStamina, 0);
        regenTimer = regenDelay;
        UpdateUI();

        if (staminaTrailImage != null)
        {
            staminaTrailImage.fillAmount = (currentStamina + amount) / maxStamina;
            trailCoroutine = StartCoroutine(DrainStaminaTrail());
        }
    }

    private IEnumerator DrainStaminaTrail()
    {
        yield return new WaitForSeconds(trailDelayTime);
        float targetFill = currentStamina / maxStamina;

        while (staminaTrailImage.fillAmount > targetFill)
        {
            staminaTrailImage.fillAmount -= trailDrainSpeed * Time.deltaTime;
            staminaTrailImage.fillAmount = Mathf.Max(staminaTrailImage.fillAmount, targetFill);
            yield return null;
        }
        trailCoroutine = null;
    }

    private void UpdateUI()
    {
        if (staminaFillImage != null)
        {
            staminaFillImage.fillAmount = currentStamina / maxStamina;
        }
    }
}