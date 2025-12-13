using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class BossHealth : MonoBehaviour, IDamageable
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

    private Coroutine trailCoroutine;
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    [SerializeField]private Behaviour behaviourTree;

    private void Awake()
    {
        currentHP = maxHP;
        UpdateHealthBar();
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))       
        {
        //    TakeDamage(20f);
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

        UpdateHealthBar();

        if (takenDamageFill != null)
        {
            takenDamageFill.fillAmount = oldHP / maxHP;

            trailCoroutine = StartCoroutine(DrainHealthTrail());

        }
        if (currentHP <= 0)
        {
            Die();
            return;
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
        isDead = true;

        if (behaviourTree != null)
            behaviourTree.enabled = false; 

        if (navMeshAgent != null)
            navMeshAgent.enabled = false;

   
    }

}