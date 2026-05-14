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
        if (playerInRange && !isLit && Input.GetKeyDown(interactKey))
        {
            LightBonfire();
        }
    }

    private void LightBonfire()
    {
        isLit = true;

        if (fireParticles != null) fireParticles.Play();
        if (glowLight != null) glowLight.enabled = true;

        if (interactUI != null) interactUI.SetActive(false);

        Debug.Log("OGNISKO ROZPALONE!");
    }

    private void OnTriggerEnter(Collider other)
    {
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
            if (interactUI != null) interactUI.SetActive(false);
        }
    }
}