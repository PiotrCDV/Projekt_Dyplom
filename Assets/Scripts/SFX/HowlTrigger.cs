using UnityEngine;
using FMODUnity;

public class HowlTrigger : MonoBehaviour
{
    [Header("Ustawienia Audio")]
    public EventReference howlEvent;

    public Transform howlSourceLocation;

    [Header("Opcje")]
    public bool playOnlyOnce = true;

    private bool hasPlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playOnlyOnce && hasPlayed) return;

            Vector3 playPosition = howlSourceLocation != null ? howlSourceLocation.position : transform.position;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(howlEvent, playPosition);
                Debug.Log("[HowlTrigger] Aauuuuu! Wilko³ak wyje.");
            }
            else
            {
                Debug.LogWarning("Brak AudioManagera na scenie!");
            }

            hasPlayed = true;
        }
    }
}
