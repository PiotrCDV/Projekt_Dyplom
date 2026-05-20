using UnityEngine;

public class BoundaryFogWall : MonoBehaviour
{
    [Header("Ustawienia Mg³y (Tryb Linear)")]
    [Tooltip("Wartoœæ 'End' dla normalnej mg³y na mapie (np. 80)")]
    public float normalEndDistance = 80f;
    [Tooltip("Wartoœæ 'End' dla œciany mg³y (np. 15 - mg³a 100% zablokuje widok bardzo blisko)")]
    public float thickEndDistance = 15f;

    [Tooltip("Jak szybko œciana mg³y przybli¿a siê do gracza?")]
    public float fadeToThickSpeed = 2f;
    [Tooltip("Jak szybko mg³a cofa siê po wyjœciu?")]
    public float fadeToNormalSpeed = 4f;

    [Header("Wiadomoœæ Narratora")]
    [TextArea(2, 4)]
    public string message = "Zbyt gêsta mg³a... Zgubiê drogê, jeœli pójdê dalej. Muszê zawróciæ.";
    public float messageDuration = 4f;
    public float messageCooldown = 8f;

    private bool isPlayerInside = false;
    private float nextMessageTime = 0f;

    void Update()
    {
        float targetEndDistance = isPlayerInside ? thickEndDistance : normalEndDistance;
        float currentSpeed = isPlayerInside ? fadeToThickSpeed : fadeToNormalSpeed;

        RenderSettings.fogEndDistance = Mathf.Lerp(RenderSettings.fogEndDistance, targetEndDistance, Time.deltaTime * currentSpeed);

        if (isPlayerInside && Time.time >= nextMessageTime)
        {
            if (SubtitleManager.Instance != null)
            {
                SubtitleManager.Instance.ShowSubtitle(message, messageDuration);
                nextMessageTime = Time.time + messageCooldown;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            nextMessageTime = 0f;
        }
    }
}