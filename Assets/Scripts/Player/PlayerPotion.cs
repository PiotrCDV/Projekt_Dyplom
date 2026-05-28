using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using FMODUnity;

public class PlayerPotion : MonoBehaviour
{
    [Header("Potion Settings")]
    [SerializeField] private float healAmount = 40f;
    [SerializeField] private int maxPotions = 5;
   private int currentPotions;
    [SerializeField] private GameObject potionModel;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI potionCountText;

    [Header("Audio (FMOD)")]
    public EventReference drinkSound;

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

        currentPotions = maxPotions;
        UpdatePotionUI();

        inputActions = new InputSystem_Actions();
        inputActions.Player.UsePotion.performed += ctx => TryDrinkPotion();
        
        if (potionModel != null) potionModel.SetActive(false);
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    public void TryDrinkPotion()
    {
        if (currentPotions <= 0)
        {
            Debug.Log("Brak mikstur!");
            return;
        }

        if (isDrinking) return;
        if (playerCombat != null && playerCombat.IsAttacking) return;
        if (GetComponent<PlayerDodge>().IsDodging) return;

        ExecutePotionAnimation();
    }

    private void ExecutePotionAnimation()
    {
        if (animator == null) return;

        isDrinking = true;
        
        currentPotions--;
        UpdatePotionUI();

        if (potionModel != null) potionModel.SetActive(true);

        bool isLocked = lockOnBehaviour != null && lockOnBehaviour.IsLocked;
        animator.SetBool("isLockedOn", isLocked);
        animator.SetTrigger("DrinkPotion");
    }

    private void UpdatePotionUI()
    {
        if (potionCountText != null)
        {
            potionCountText.text = currentPotions.ToString();
        }
    }

    public void ApplyHealing()
    {
        if (playerHealth != null)
        {
            playerHealth.Heal(healAmount);
        }

        if (!drinkSound.IsNull)
        {
            FMOD.Studio.EventInstance instance = FMODUnity.RuntimeManager.CreateInstance(drinkSound);
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(instance, gameObject);
            instance.start();
            instance.release();
        }
    }

    public void OnDrinkFinished()
    {
        isDrinking = false;

        if (potionModel != null) potionModel.SetActive(false);
    }

}