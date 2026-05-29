using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameMessageUI : MonoBehaviour
{
    public static GameMessageUI Instance { get; private set; }

    [Header("Referencje")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image victoryImage;
    [SerializeField] private Image deathImage;

    [Header("Ustawienia Czasu")]
    public float fadeInTime = 1f;
    public float stayTime = 2.5f;
    public float fadeOutTime = 1.5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (victoryImage != null) victoryImage.gameObject.SetActive(false);
        if (deathImage != null) deathImage.gameObject.SetActive(false);
    }

    public void ShowVictory() => StartCoroutine(FadeRoutine(victoryImage));
    public void ShowDeath() => StartCoroutine(FadeRoutine(deathImage));

    private IEnumerator FadeRoutine(Image targetImage)
    {
        if (victoryImage != null) victoryImage.gameObject.SetActive(false);
        if (deathImage != null) deathImage.gameObject.SetActive(false);

        targetImage.gameObject.SetActive(true);

        float elapsed = 0;
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInTime);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(stayTime);

        elapsed = 0;
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutTime);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        targetImage.gameObject.SetActive(false);
    }
}
