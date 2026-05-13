using UnityEngine;
using System.Collections;

public class BossGateController : MonoBehaviour
{
    [Header("Referencje")]
    public Renderer gateRenderer;
    public Collider blockingCollider;

    [Header("UI Interakcji (World Space)")]
    [Tooltip("Przeci¹gnij tutaj obiekt Canvas unocz¹cy siê nad bram¹")]
    public GameObject interactUI;

    [Header("Ustawienia")]
    public KeyCode interactKey = KeyCode.F;
    public string dissolveParam = "_DissolveAmount";
    public float fadeDuration = 1.5f;

    private Material gateMaterial;
    private bool isOpened = false;
    private bool isPlayerInside = false;
    private bool playerInRange = false;

    void Start()
    {
        if (gateRenderer != null)
            gateMaterial = gateRenderer.material;

        gateMaterial.SetFloat(dissolveParam, 0f);

        if (interactUI != null) interactUI.SetActive(false);

        if (blockingCollider != null) blockingCollider.enabled = true;
    }

    void Update()
    {
        if (playerInRange && !isOpened && !isPlayerInside)
        {
            if (Input.GetKeyDown(interactKey) || Input.GetMouseButtonDown(0))
            {
                OpenGate();
            }
        }
    }

    public void OpenGate()
    {
        isOpened = true;
        if (interactUI != null) interactUI.SetActive(false);
        StartCoroutine(FadeDissolve(0f, 1f));
        if (blockingCollider != null) blockingCollider.enabled = false;
    }

    public void CloseGate()
    {
        isPlayerInside = true;
        isOpened = false;
        playerInRange = false;
        if (interactUI != null) interactUI.SetActive(false);
        StartCoroutine(FadeDissolve(1f, 0f));
        if (blockingCollider != null) blockingCollider.enabled = true;
    }

    private IEnumerator FadeDissolve(float start, float end)
    {
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float current = Mathf.Lerp(start, end, elapsed / fadeDuration);
            gateMaterial.SetFloat(dissolveParam, current);
            yield return null;
        }
        gateMaterial.SetFloat(dissolveParam, end);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpened && !isPlayerInside)
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