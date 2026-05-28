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

    [Header("Respawn Settings")]
    [SerializeField] private Transform startingPoint; 
    [SerializeField] private float delayBeforeReload = 3.0f; 

    private static Vector3? activeCheckpointPos;
    private static Quaternion? activeCheckpointRot;

    [Header("Visual Effects")]
    [SerializeField] private float trailDelayTime = 1.0f;
    [SerializeField] private float trailDrainSpeed = 0.5f;
    [SerializeField] private float healFillDuration = 0.25f;

    private Coroutine trailCoroutine;
    private Coroutine healCoroutine;
    private bool isHealing;
    private Animator animator;

    [Header("Audio")]
    [SerializeField] private EventReference damageSound;

    private void Awake()
    {
        currentHP = maxHP;
        animator = GetComponent<Animator>();

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        if (activeCheckpointPos.HasValue)
        {
            transform.position = activeCheckpointPos.Value;
            transform.rotation = activeCheckpointRot.Value;
        }
        else if (startingPoint != null)
        {
            transform.position = startingPoint.position;
            transform.rotation = startingPoint.rotation;
        }

        if (cc != null) cc.enabled = true;

        UpdateHealthBar();
    }

    public static void UpdateCheckpoint(Vector3 pos, Quaternion rot)
    {
        activeCheckpointPos = pos;
        activeCheckpointRot = rot;
    }

    public void TakeDamage(float damage)
    {
        PlayerDodge dodge = GetComponent<PlayerDodge>();
        if (dodge != null && dodge.IsInvincible) return; 
        if (isDead) return;

        if (trailCoroutine != null) StopCoroutine(trailCoroutine);

        float oldHP = currentHP;
        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(damageSound, transform.position);

        UpdateHealthBar();

        if (takenDamageFill != null)
        {
            takenDamageFill.fillAmount = oldHP / maxHP;
            trailCoroutine = StartCoroutine(DrainHealthTrail());
        }

        if (currentHP <= 0) Die();
    }

    private void UpdateHealthBar()
    {
        if (healthFillImage != null)
            healthFillImage.fillAmount = currentHP / maxHP;
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

        if (animator != null) animator.SetTrigger("PlayerDeath");

        if (GetComponent<PlayerMovement>() != null) GetComponent<PlayerMovement>().enabled = false;
        if (GetComponent<PlayerCombat>() != null) GetComponent<PlayerCombat>().enabled = false;

        PlayerDodge dodge = GetComponent<PlayerDodge>();
        if (dodge != null) 
        {
            dodge.enabled = false; 
        }
        
        if (GameMessageUI.Instance != null) GameMessageUI.Instance.ShowDeath();

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

        if (amount <= 0f) return;

        if (trailCoroutine != null)
        {
            StopCoroutine(trailCoroutine);
            trailCoroutine = null;
        }

        InterruptHealing();

        float targetHP = Mathf.Min(currentHP + amount, maxHP);

        if (Mathf.Approximately(currentHP, targetHP))
        {
            UpdateHealthBar();
            if (takenDamageFill != null) takenDamageFill.fillAmount = currentHP / maxHP;
            return;
        }

        if (healFillDuration <= 0f)
        {
            currentHP = targetHP;
            UpdateHealthBar();
            if (takenDamageFill != null) takenDamageFill.fillAmount = currentHP / maxHP;
            return;
        }

        healCoroutine = StartCoroutine(HealRoutine(targetHP));
    }

    private IEnumerator HealRoutine(float targetHP)
    {
        isHealing = true;
        float startHP = currentHP;
        float elapsed = 0f;
        float startFill = startHP / maxHP;
        float targetFill = targetHP / maxHP;

        if (healthFillImage != null)
            healthFillImage.fillAmount = startFill;

        if (takenDamageFill != null)
            takenDamageFill.fillAmount = startFill;

        while (elapsed < healFillDuration)
        {
            if (!isHealing) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / healFillDuration);
            float currentFill = Mathf.Lerp(startFill, targetFill, t);
            currentHP = Mathf.Lerp(startHP, targetHP, t);

            if (healthFillImage != null)
                healthFillImage.fillAmount = currentFill;

            if (takenDamageFill != null)
                takenDamageFill.fillAmount = currentFill;

            yield return null;
        }

        if (healthFillImage != null)
            healthFillImage.fillAmount = targetFill;

        if (takenDamageFill != null)
            takenDamageFill.fillAmount = targetFill;

        currentHP = targetHP;
        isHealing = false;
        healCoroutine = null;
    }

    private void InterruptHealing()
    {
        isHealing = false;

        if (healCoroutine != null)
        {
            StopCoroutine(healCoroutine);
            healCoroutine = null;
        }
    }

}