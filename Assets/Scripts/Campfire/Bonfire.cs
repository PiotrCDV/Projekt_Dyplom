using UnityEngine;

public class Bonfire : MonoBehaviour
{
    [Header("Referencje Wizualne")]
    public ParticleSystem fireParticles;
    public Light glowLight;

    [Header("Ustawienia")]
    public bool isLit = false;
    public KeyCode interactKey = KeyCode.E;

    private bool playerInRange = false;

    void Start()
    {
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
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            if (!isLit)
            {
                LightBonfire();
            }
            else
            {
                RestAtBonfire();
            }
        }
    }

    private void LightBonfire()
    {
        isLit = true;
        if (fireParticles != null) fireParticles.Play();
        if (glowLight != null) glowLight.enabled = true;

        Debug.Log("OGNISKO ROZPALONE! (Bonfire Lit)");
    }

    private void RestAtBonfire()
    {
        Debug.Log("Odpoczywasz przy ognisku...");
        // TODO: Tutaj w przysz³oœci dodasz odnawianie zdrowia, reset przeciwników i menu levelowania
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Naciœnij E, aby wejœæ w interakcjê z ogniskiem");
            // TODO: Pokazaæ UI na ekranie
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            // TODO: Ukryæ UI
        }
    }
}