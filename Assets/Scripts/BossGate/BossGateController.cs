using UnityEngine;
using System.Collections;

public class BossGateController : MonoBehaviour
{
    [Header("Referencje")]
    public Renderer gateRenderer;
    public Transform playerTransform;
    public Collider blockingCollider;

    [Header("Ustawienia Interakcji")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.F;
    public string dissolveParam = "_DissolveAmount";

    [Header("Ustawienia Animacji")]
    public float fadeDuration = 1.5f;

    private Material gateMaterial;
    private bool isOpened = false;
    private bool isPlayerInside = false;

    void Start()
    {
        if (gateRenderer != null)
            gateMaterial = gateRenderer.material;
        gateMaterial.SetFloat(dissolveParam, 0f);
        if (blockingCollider != null) blockingCollider.enabled = true;
    }

    void Update()
    {
        if (isOpened || isPlayerInside) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist <= interactionDistance)
        {
            if (Input.GetKeyDown(interactKey))
            {
                OpenGate();
            }
        }
    }

    public void OpenGate()
    {
        isOpened = true;
        StartCoroutine(FadeDissolve(0f, 1f));
        if (blockingCollider != null) blockingCollider.enabled = false;
    }

    public void CloseGate()
    {
        isPlayerInside = true;
        isOpened = false;
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
}