using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using TMPro;

public class SimpleAsyncLoader : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject loadingScreen;
    public Image loadingBarFill; 

    [Header("Settings")]
    public float smoothSpeed = 0.5f;

    public void LoadLevel(string sceneName)
    {
        StartCoroutine(LoadAsynchronously(sceneName));
    }

 IEnumerator LoadAsynchronously(string sceneName)
{
    loadingScreen.SetActive(true);
    loadingBarFill.fillAmount = 0;

    AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
    
    operation.allowSceneActivation = false;

    float visualProgress = 0;

    while (!operation.isDone)
    {
        float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

        visualProgress = Mathf.MoveTowards(visualProgress, targetProgress, Time.deltaTime * smoothSpeed);
        loadingBarFill.fillAmount = visualProgress;

        if (operation.progress >= 0.9f)
        {
            loadingBarFill.fillAmount = 1f;
            
            operation.allowSceneActivation = true;
        }

        yield return null;
    }
}
}