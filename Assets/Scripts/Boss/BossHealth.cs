using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class BossHealth : MonoBehaviour, IDamageable
{
    [Header("UI & AI References")]
    [SerializeField] private GameObject healthBarContainer; // DODANE: Ca³y kontener paska UI
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image takenDamageFill;

    [Header("Health Settings")]
    [SerializeField] private float maxHP = 100f;
    private float currentHP;
    private bool isDead = false;

    [Header("Visual Effects")]
    [SerializeField] private float trailDelayTime = 1.0f;
    [SerializeField] private float trailDrainSpeed = 0.5f;
    [SerializeField] private float timeBeforeDisable = 5.0f;

    private Coroutine trailCoroutine;
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    [SerializeField] private Behaviour behaviourTree;

    [Header("Audio")]
    [SerializeField] private AudioClip damageSound;

    private void Awake()
    {
        currentHP = maxHP;
        UpdateHealthBar();
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
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
            AudioManager.Instance.PlaySFX(damageSound);

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
            animator.SetTrigger("BossDowned");
        }

     

        if (behaviourTree != null)
            behaviourTree.enabled = false;

        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
        }

        StartCoroutine(DisableBossRoutine());
    }

    private IEnumerator DisableBossRoutine()
    {
        yield return new WaitForSeconds(timeBeforeDisable);
        gameObject.SetActive(false);
        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(false);
        }
    }
}