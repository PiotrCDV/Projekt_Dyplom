using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPotion : MonoBehaviour
{
    [Header("Potion Settings")]
    [SerializeField] private float healAmount = 40f;
    [SerializeField] private GameObject potionModel;

    [Header("References")]
    private Animator animator;
    private LockOnBehaviour lockOnBehaviour;
    private PlayerCombat playerCombat;
    private PlayerHealth playerHealth;

    private InputSystem_Actions inputActions;
    private bool isDrinking;
    public bool IsDrinking => isDrinking;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        lockOnBehaviour = GetComponent<LockOnBehaviour>();
        playerCombat = GetComponent<PlayerCombat>();
        playerHealth = GetComponent<PlayerHealth>();

        inputActions = new InputSystem_Actions();
        inputActions.Player.UsePotion.performed += ctx => TryDrinkPotion();
        
        if (potionModel != null) potionModel.SetActive(false);
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    public void TryDrinkPotion()
    {
        if (isDrinking) return;
        if (playerCombat != null && playerCombat.IsAttacking) return;
        if (GetComponent<PlayerDodge>().IsDodging) return;

        ExecutePotionAnimation();
    }

    private void ExecutePotionAnimation()
    {
        if (animator == null) return;

        isDrinking = true;

        if (potionModel != null) potionModel.SetActive(true);

        bool isLocked = lockOnBehaviour != null && lockOnBehaviour.IsLocked;
        animator.SetBool("isLockedOn", isLocked);
        animator.SetTrigger("DrinkPotion");
    }

    public void ApplyHealing()
    {
        if (playerHealth != null)
        {
            playerHealth.Heal(healAmount);
        }
    }

    public void OnDrinkFinished()
    {
        isDrinking = false;

        if (potionModel != null) potionModel.SetActive(false);
    }
}