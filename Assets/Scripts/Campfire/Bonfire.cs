using UnityEngine;

public class Bonfire : MonoBehaviour
{
    [Header("Referencje Wizualne")]
    public ParticleSystem fireParticles;
    public Light glowLight;

    [Header("UI Interakcji")]
    public GameObject interactUI;

    [Header("Ustawienia")]
    public KeyCode interactKey = KeyCode.E;
    public Transform spawnPoint;

    private static bool isGlobalBonfireLit = false;
    private bool playerInRange = false;

    void Start()
    {
        if (interactUI != null) interactUI.SetActive(false);

        if (isGlobalBonfireLit)
        {
            ApplyLitState();
        }
        else
        {
            if (fireParticles != null) fireParticles.Stop();
            if (glowLight != null) glowLight.enabled = false;
        }
    }

    void Update()
    {
        if (playerInRange && !isGlobalBonfireLit && Input.GetKeyDown(interactKey))
        {
            LightBonfire();
        }
    }

    private void LightBonfire()
    {
        isGlobalBonfireLit = true;
        ApplyLitState();

        if (interactUI != null) interactUI.SetActive(false);

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;
        
        PlayerHealth.UpdateCheckpoint(pos, rot);
        
        Debug.Log("OGNISKO ZAPALONE NA STAŁE!");
    }

    private void ApplyLitState()
    {
        if (fireParticles != null) fireParticles.Play();
        if (glowLight != null) glowLight.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactUI != null && !isGlobalBonfireLit) 
                interactUI.SetActive(true);
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