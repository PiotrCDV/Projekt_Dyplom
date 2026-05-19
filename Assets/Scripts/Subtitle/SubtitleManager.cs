using UnityEngine;
using TMPro;
using System.Collections;

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance;

    [Header("Referencje UI")]
    public TextMeshProUGUI subtitleText;

    [Header("Ustawienia Animacji")]
    [Tooltip("Ile sekund zajmie p³ynne pojawienie siê i znikniêcie napisu?")]
    public float fadeDuration = 1f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (subtitleText != null)
        {
            subtitleText.text = "";
            SetTextAlpha(0f);
        }
    }

    public void ShowSubtitle(string textToShow, float duration)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(DisplayAndFadeRoutine(textToShow, duration));
    }

    private IEnumerator DisplayAndFadeRoutine(string text, float displayDuration)
    {
        subtitleText.text = text;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetTextAlpha(Mathf.Lerp(0f, 1f, elapsed / fadeDuration));
            yield return null;
        }
        SetTextAlpha(1f);

        yield return new WaitForSeconds(displayDuration);

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetTextAlpha(Mathf.Lerp(1f, 0f, elapsed / fadeDuration));
            yield return null;
        }
        SetTextAlpha(0f);

        subtitleText.text = "";
    }

    private void SetTextAlpha(float alpha)
    {
        if (subtitleText != null)
        {
            Color c = subtitleText.color;
            c.a = alpha;
            subtitleText.color = c;
        }
    }
}