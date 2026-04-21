using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using System.Collections;
using UnityEngine.SceneManagement; 

public class PlayerHealth : MonoBehaviour
{
    [Header("UI & AI References")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image takenDamageFill;

    [Header("Health Settings")]
    [SerializeField] private float maxHP = 100f;
    private float currentHP;
    private bool isDead = false;

    [Header("Visual Effects")]
    [SerializeField] private float trailDelayTime = 1.0f;
    [SerializeField] private float trailDrainSpeed = 0.5f;
    [SerializeField] private float delayBeforeReload = 3.0f; 

    private Coroutine trailCoroutine;
    private Animator animator;

    [Header("Audio")]
    [SerializeField] private EventReference damageSound; // Zamiast AudioClip

    private void Awake()
    {
        currentHP = maxHP;
        animator = GetComponent<Animator>(); 
        UpdateHealthBar();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        if (trailCoroutine != null)
        {
            StopCoroutine(trailCoroutine);
        }

        float oldHP = currentHP;
        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(damageSound, transform.position);
        }

        UpdateHealthBar();

        if (takenDamageFill != null)
        {
            takenDamageFill.fillAmount = oldHP / maxHP;
            trailCoroutine = StartCoroutine(DrainHealthTrail());
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthFillImage != null)
        {
            float fillRatio = currentHP / maxHP;
            healthFillImage.fillAmount = fillRatio;
        }
    }

    private IEnumerator DrainHealthTrail()
    {
        yield return new WaitForSeconds(trailDelayTime);
        float targetFill = currentHP / maxHP;

        while (takenDamageFill.fillAmount > targetFill)
        {
            takenDamageFill.fillAmount -= trailDrainSpeed * Time.deltaTime;
            takenDamageFill.fillAmount = Mathf.Max(takenDamageFill.fillAmount, targetFill);
            yield return null;
        }
        trailCoroutine = null;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator != null)
        {
            animator.SetTrigger("PlayerDeath");
        }

        if (GetComponent<PlayerMovement>() != null) GetComponent<PlayerMovement>().enabled = false;
        if (GetComponent<PlayerCombat>() != null) GetComponent<PlayerCombat>().enabled = false;

        StartCoroutine(ReloadSceneRoutine());
    }

    private IEnumerator ReloadSceneRoutine()
    {
        yield return new WaitForSeconds(delayBeforeReload);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHP += amount;
        currentHP = Mathf.Min(currentHP, maxHP); 

        UpdateHealthBar();

    
        if (takenDamageFill != null)
        {
            takenDamageFill.fillAmount = currentHP / maxHP;
        }
    }
}