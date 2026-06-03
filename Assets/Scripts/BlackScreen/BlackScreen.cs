using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class BlackScreen : MonoBehaviour
{
    public static BlackScreen Instance { get; private set; }

    private Image blackScreen;

    [Header("Ustawienia Startowe")]
    [Tooltip("Ile sekund zajmie rozjaœnianie ekranu na pocz¹tku gry?")]
    public float fadeDuration = 3f;

    public GameObject backToMenuButton;
    public string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        blackScreen = GetComponent<Image>();

        if (backToMenuButton != null)
            backToMenuButton.SetActive(false);
    }

    void Start()
    {
        SetAlpha(1f);
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(1f, 0f, elapsed / fadeDuration));
            yield return null;
        }
        SetAlpha(0f);

        if (blackScreen != null) blackScreen.raycastTarget = false;
    }

    public void FadeToBlackAndShowButton(float duration)
    {
        if (blackScreen != null) blackScreen.raycastTarget = true;

        Time.timeScale = 0f;

        StartCoroutine(FadeOutRoutine(duration));
    }

    private IEnumerator FadeOutRoutine(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(0f, 1f, elapsed / duration));
            yield return null;
        }
        SetAlpha(1f);

        if (backToMenuButton != null) backToMenuButton.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}