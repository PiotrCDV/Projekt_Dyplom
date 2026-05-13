using UnityEngine;

public class Bonfire : MonoBehaviour
{
    [Header("Referencje Wizualne")]
    public ParticleSystem fireParticles;
    public Light glowLight;

    [Header("UI Interakcji")]
    [Tooltip("Przeci¹gnij tutaj obiekt tekstowy z Canvasa (np. napis 'Naciœnij E')")]
    public GameObject interactUI;

    [Header("Ustawienia")]
    public bool isLit = false;
    public KeyCode interactKey = KeyCode.E;

    private bool playerInRange = false;

    void Start()
    {
        // Upewniamy siê, ¿e UI jest wy³¹czone na starcie
        if (interactUI != null) interactUI.SetActive(false);

        if (!isLit)
        {
            if (fireParticles != null) fireParticles.Stop();
            if (glowLight != null) glowLight.enabled = false;
        }
        else
        {
            if (fireParticles != null) fireParticles.Play();
            if (glowLight != null) glowLight.enabled = true;
        }
    }

    void Update()
    {
        // Interakcja dzia³a TYLKO jeœli gracz jest w zasiêgu i ognisko jest zgaszone
        if (playerInRange && !isLit && Input.GetKeyDown(interactKey))
        {
            LightBonfire();
        }
    }

    private void LightBonfire()
    {
        isLit = true;

        // Odpalamy efekty wizualne!
        if (fireParticles != null) fireParticles.Play();
        if (glowLight != null) glowLight.enabled = true;

        // Wy³¹czamy UI interakcji, bo nie ma ju¿ tu nic do roboty
        if (interactUI != null) interactUI.SetActive(false);

        Debug.Log("OGNISKO ROZPALONE!");
    }

    // --- LOGIKA STREFY INTERAKCJI ---

    private void OnTriggerEnter(Collider other)
    {
        // Reagujemy tylko, gdy wejdzie gracz, a ognisko jest wci¹¿ do rozpalenia
        if (other.CompareTag("Player") && !isLit)
        {
            playerInRange = true;
            if (interactUI != null) interactUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            // Zawsze chowamy UI, gdy gracz odejdzie
            if (interactUI != null) interactUI.SetActive(false);
        }
    }
}