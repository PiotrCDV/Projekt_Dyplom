using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlackScreen : MonoBehaviour
{
    private Image blackScreen;

    [Tooltip("Ile sekund zajmie rozjaœnianie ekranu?")]
    public float fadeDuration = 3f;

    void Start()
    {
        blackScreen = GetComponent<Image>();
        SetAlpha(1f);
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(1f, 0f, elapsed / fadeDuration));
            yield return null;
        }
        SetAlpha(0f);
        gameObject.SetActive(false);
    }

    private void SetAlpha(float alpha)
    {
        if (blackScreen != null)
        {
            Color c = blackScreen.color;
            c.a = alpha;
            blackScreen.color = c;
        }
    }
}
