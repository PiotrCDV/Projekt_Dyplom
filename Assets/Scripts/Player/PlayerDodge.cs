using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDodge : MonoBehaviour
{
    [Header("Ustawienia Uniku")]
    public float dodgeSlideDuration = 0.3f;
    public float dodgeSpeed = 12f;

    public bool IsDodging { get; private set; } = false;

    private InputSystem_Actions inputActions;
    private Animator animator;
    private CharacterController controller;
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;

    private Vector3 savedDodgeDirection;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();

        inputActions = new InputSystem_Actions();

        // OBS£UGA KLAWIATURY (Spacja)
        inputActions.Player.Dodge.performed += ctx =>
        {
            // Jeœli to klawiatura, wykonujemy unik natychmiast po klikniêciu Spacji
            if (ctx.control.device is Keyboard)
            {
                PrepareDodge();
            }
            // Pad jest ignorowany tutaj, bo jego logikê (Tap/Hold) obs³uguje PlayerMovement
        };
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    // Metoda wywo³ywana przez klawiaturê LUB przez PlayerMovement (dla pada)
    public void PrepareDodge()
    {
        if (IsDodging) return;

        // Blokada uniku, jeœli postaæ w³aœnie atakuje
        if (playerCombat != null && playerCombat.IsAttacking) return;

        IsDodging = true;

        // Wy³¹czamy standardowy ruch na czas uniku
        if (playerMovement != null) playerMovement.SetMovementEnabled(false);

        // Obliczamy kierunek (gdzie gracz wychyla analog/klawisze)
        CalculateDodgeDirection();

        // Obracamy postaæ natychmiast w stronê uniku
        if (savedDodgeDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(savedDodgeDirection);
        }

        if (animator != null) animator.SetTrigger("Dodge");
    }

    // --- Metody wywo³ywane przez Animation Events ---

    public void StartDodge()
    {
        if (!IsDodging) return;
        StartCoroutine(DodgeSlideRoutine());
    }

    public void FinishDodge()
    {
        if (playerMovement != null) playerMovement.SetMovementEnabled(true);
        IsDodging = false;
        StopAllCoroutines();
    }

    // --- Logika Ruchu ---

    private IEnumerator DodgeSlideRoutine()
    {
        float timer = 0f;

        while (timer < dodgeSlideDuration)
        {
            // Przesuwamy postaæ w zapisanym kierunku
            controller.Move(savedDodgeDirection * dodgeSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void CalculateDodgeDirection()
    {
        // Czytamy wejœcie z osi ruchu (WASD / Analog)
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();

        if (input.magnitude > 0.1f)
        {
            // Pobieramy wektory kamery, aby unik lecia³ tam, gdzie gracz widzi
            Transform camTransform = Camera.main.transform;
            Vector3 camForward = camTransform.forward;
            Vector3 camRight = camTransform.right;

            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            savedDodgeDirection = camForward * input.y + camRight * input.x;
            savedDodgeDirection.Normalize();
        }
        else
        {
            // Jeœli gracz nie trzyma ¿adnego kierunku, unik leci "do przodu" postaci
            savedDodgeDirection = transform.forward;
        }
    }
}