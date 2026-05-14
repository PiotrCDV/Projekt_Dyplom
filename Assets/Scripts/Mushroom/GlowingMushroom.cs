using UnityEngine;
using System.Collections;

public class GlowingMushroom : MonoBehaviour
{
    public Renderer mushroomRenderer;
    public float targetIntensity = 5f;
    public float fadeSpeed = 2f;
    public string shaderParameter = "_GlowIntensity";

    private Material mushroomMat;
    private float currentIntensity = 0f;
    private bool isPlayerNearby = false;

    void Start()
    {
        mushroomMat = mushroomRenderer.material;
        mushroomMat.SetFloat(shaderParameter, 0f);
    }

    void Update()
    {
        float target = isPlayerNearby ? targetIntensity : 0f;
        currentIntensity = Mathf.MoveTowards(currentIntensity, target, fadeSpeed * Time.deltaTime);

        mushroomMat.SetFloat(shaderParameter, currentIntensity);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNearby = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNearby = false;
    }
}