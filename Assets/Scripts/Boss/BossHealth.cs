using UnityEngine;
using UnityEngine.AI;
using FMODUnity;
using UnityEngine.UI;
using System.Collections;

public class BossHealth : MonoBehaviour, IDamageable
{
    [Header("UI & AI References")]
    [SerializeField] private GameObject healthBarContainer;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image takenDamageFill;

    [Header("Health Settings")]
    [SerializeField] private float maxHP = 100f;
    private float currentHP;
    private bool isDead = false;

    public bool IsExecutable { get; private set; }

    [Header("Visual Effects & Timing")]
    [SerializeField] private float trailDelayTime = 1.0f;
    [SerializeField] private float trailDrainSpeed = 0.5f;
    [SerializeField] private float executionWindowTime = 5.0f; // Ile sekund gracz ma na wciœniêcie ataku zeby aktywowaæ egzekucjê
    [SerializeField] private float timeBeforeDisable = 5.0f;   // Ile czasu po ostatecznej œmierci boss znika

    private Coroutine trailCoroutine;
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    [SerializeField] private Behaviour behaviourTree;

    [Header("Audio")]
    [SerializeField] private EventReference damageSound;

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

        IsExecutable = true;

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

        StartCoroutine(ExecutionWindowRoutine());
    }

    private IEnumerator ExecutionWindowRoutine()
    {
        yield return new WaitForSeconds(executionWindowTime);

        if (IsExecutable)
        {
            IsExecutable = false;

            if (animator != null)
            {
                animator.SetTrigger("BossDeath");
            }

            StartCoroutine(DisableBossRoutine());
        }
    }

    public void ConfirmExecution()
    {
        IsExecutable = false;

        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(false);
        }

        if (animator != null)
        {
            animator.SetTrigger("BossDeath");
        }

        StartCoroutine(DisableAfterExecutionRoutine());
    }

    private IEnumerator DisableAfterExecutionRoutine()
    {

        yield return new WaitForSeconds(8.0f);
        gameObject.SetActive(false);
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