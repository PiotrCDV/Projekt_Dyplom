using UnityEngine;

public class MessageTrigger : MonoBehaviour
{
    [Header("Ustawienia Wiadomoœci")]
    [TextArea(3, 5)]
    public string message = "Wpisz tu myœli bohatera...";

    [Tooltip("Ile sekund napis ma wisieæ na ekranie?")]
    public float displayDuration = 4f;

    [Tooltip("Czy wiadomoœæ ma siê pokazaæ tylko raz na ca³¹ grê?")]
    public bool showOnlyOnce = true;

    private bool hasBeenShown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenShown)
        {
            SubtitleManager.Instance.ShowSubtitle(message, displayDuration);
            if (showOnlyOnce)
            {
                hasBeenShown = true;
            }
        }
    }
}