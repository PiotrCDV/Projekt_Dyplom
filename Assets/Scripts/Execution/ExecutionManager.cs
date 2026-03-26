using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections;

public class ExecutionManager : MonoBehaviour
{
    public static ExecutionManager Instance;

    [Header("Referencje")]
    public PlayableDirector executionDirector;
    public TimelineAsset executionTimelineAsset;

    [Header("Ustawienia Ścieżek")]
    public string playerTrackName = "Executions_Protagonist";
    public string enemyTrackName = "Executions_Werewolf";

    [Header("Dostrajanie Pozycji i Rotacji")]
    [Tooltip("O ile metrów odsunąć punkt startowy od bossa (oś przód-tył)")]
    float distanceOffset = -1111.2f; 
    
    [Tooltip("Korekta obrotu gracza. Jeśli jest tyłem, wpisz 180. Jeśli bokiem, 90 lub -90.")]
    public float rotationOffset = 0f;

    private void Awake() 
    { 
        if (Instance == null) Instance = this; 
    }

    public void StartExecution(Animator playerAnim, Animator werewolfAnim)
    {
        // 1. Obliczamy pozycję: Bierzemy pozycję wilkołaka i odsuwamy Directora 
        // o 'distanceOffset' w kierunku, w którym patrzy wilkołak.
        // Dzięki temu gracz nie zacznie animacji "wewnątrz" modelu bossa.
        Vector3 spawnPosition = werewolfAnim.transform.position + (werewolfAnim.transform.forward * distanceOffset);
        executionDirector.transform.position = spawnPosition;

        // 2. Ustawiamy rotację Directora na rotację wilkołaka + ewentualna poprawka 180 stopni
        executionDirector.transform.rotation = werewolfAnim.transform.rotation * Quaternion.Euler(0, rotationOffset, 0);

        // 3. Wpinamy aktorów do odpowiednich ścieżek w Timeline
        foreach (var track in executionTimelineAsset.GetOutputTracks())
        {
            if (track.name == playerTrackName)
            {
                executionDirector.SetGenericBinding(track, playerAnim);
            }
            else if (track.name == enemyTrackName)
            {
                executionDirector.SetGenericBinding(track, werewolfAnim);
            }
        }

        // 4. KLUCZOWE: Resetujemy czas i wymuszamy aktualizację klatki zero (Evaluate).
        // Dzięki temu postacie zostaną natychmiast teleportowane na właściwe miejsca.
        executionDirector.time = 0;
        executionDirector.Evaluate();

        // 5. Kamera, Akcja!
        executionDirector.Play();

        // 6. Czekamy na koniec animacji
        StartCoroutine(WaitForExecutionEnd());
    }

    private IEnumerator WaitForExecutionEnd()
    {
        // Czekamy tyle sekund, ile trwa cały Asset Timeline
        yield return new WaitForSeconds((float)executionDirector.duration);
        Debug.Log("Egzekucja Zakończona!");
        
        // Tutaj możesz dodać np. przywrócenie sterowania postacią
    }
}