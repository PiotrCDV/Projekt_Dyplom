using UnityEngine;
using UnityEngine.AI;
using FMODUnity;
using UnityEngine.UI;
using UnityEngine.InputSystem; // WA�NE: Dodano Input System
using System.Collections;

public class BossHealth : MonoBehaviour, IDamageable
{
    public static BossHealth Instance { get; private set; }

    [Header("UI & AI References")]
    [SerializeField] private GameObject healthBarContainer;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image takenDamageFill;

    [Header("Health Settings")]
    [SerializeField] private float maxHP = 100f;
    private float currentHP;
    public bool isDead { get; private set; } = false;

    public bool IsExecutable { get; private set; }

    [Header("Visual Effects & Timing")]
    [SerializeField] private float trailDelayTime = 1.0f;
    [SerializeField] private float trailDrainSpeed = 0.5f;
    [SerializeField] private float executionWindowTime = 5.0f; // Ile sekund gracz ma na wci�ni�cie ataku zeby aktywowa� egzekucj�
    [SerializeField] private float timeBeforeDisable = 5.0f;   // Ile czasu po ostatecznej �mierci boss znika

    private Coroutine trailCoroutine;
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    [SerializeField] private Behaviour behaviourTree;

    [Header("Audio")]
    [SerializeField] private EventReference damageSound;

    [Header("Execution UI")]
    [SerializeField] private GameObject executionUIParent; // Rodzic ikon (ExecutionUI)
    [SerializeField] private Image buttonPromptImage;      // Obrazek, kt�ry si� zmienia
    [SerializeField] private Sprite mouseIcon;             // PNG dla LPM
    [SerializeField] private Sprite gamepadIcon;           // PNG dla przycisku pada

    private Camera mainCamera;
    public FMODUnity.EventReference levelAmbientMusic;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        currentHP = maxHP;
        UpdateHealthBar();
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        mainCamera = Camera.main;

        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(false);
        }

        if (executionUIParent != null) executionUIParent.SetActive(false);
    }

    public void ShowHealthBar()
    {
        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(true);
        }
    }

    public void HideHealthBar()
    {
        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(false);
        }
    }

    private void Update()
    {
        if (IsExecutable)
        {
            HandleExecutionUI();
        }
    }

    private void HandleExecutionUI()
    {
        if (executionUIParent == null) return;

        if (mainCamera != null)
        {
            executionUIParent.transform.LookAt(executionUIParent.transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
        }

        if (buttonPromptImage != null)
        {
            bool usingGamepad = false;

            if (Gamepad.current != null)
            {
                float lastPadTime = (float)Gamepad.current.lastUpdateTime;
                float lastKbTime = Keyboard.current != null ? (float)Keyboard.current.lastUpdateTime : 0;
                float lastMouseTime = Mouse.current != null ? (float)Mouse.current.lastUpdateTime : 0;

                if (lastPadTime > lastKbTime && lastPadTime > lastMouseTime)
                {
                    usingGamepad = true;
                }
            }

            buttonPromptImage.sprite = usingGamepad ? gamepadIcon : mouseIcon;
        }
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

        if (executionUIParent != null)
        {
            executionUIParent.SetActive(true);
        }

        if (!levelAmbientMusic.IsNull && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(levelAmbientMusic);
        }
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

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

            if (executionUIParent != null)
            {
                executionUIParent.SetActive(false);
            }

            if (animator != null)
            {
                animator.SetTrigger("BossDeath");
            }

            if (GameMessageUI.Instance != null)
            {
                GameMessageUI.Instance.ShowVictory();
            }

            if (LockOnBehaviour.Instance != null && LockOnBehaviour.Instance.IsLocked)
            {
                LockOnBehaviour.Instance.UnlockTarget();
            }

            StartCoroutine(DisableBossRoutine());
        }
    }

    public void ConfirmExecution()
    {
        IsExecutable = false;

        if (executionUIParent != null)
        {
            executionUIParent.SetActive(false);
        }

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

        HideHealthBar();
    }
}