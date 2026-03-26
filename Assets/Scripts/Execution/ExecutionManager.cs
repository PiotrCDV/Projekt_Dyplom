using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections;

public class ExecutionManager : MonoBehaviour
{
    public static ExecutionManager Instance;
    public PlayableDirector executionDirector;
    public TimelineAsset executionTimelineAsset;
    public string playerTrackName = "Executions_Protagonist";
    public string enemyTrackName = "Executions_Werewolf";

    private void Awake() { if (Instance == null) Instance = this; }

    public void StartExecution(Animator playerAnim, Animator werewolfAnim)
    {
        // 1. Re¿yser leci dok³adnie na miejsce wilko³aka
        executionDirector.transform.position = werewolfAnim.transform.position;

        // 2. NA ODWRÓT! Bierzemy rotacjê wilko³aka i odwracamy ca³¹ scenê o 180 stopni.
        // To powinno przeteleportowaæ gracza dok³adnie na drug¹ stronê bossa!
        executionDirector.transform.rotation = werewolfAnim.transform.rotation * Quaternion.Euler(0, 180, 0);

        // 3. Wpinamy aktorów do œcie¿ek
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

        // 4. Kamera, Akcja!
        executionDirector.Play();

        // 5. Czekamy na koniec
        StartCoroutine(WaitForExecutionEnd());
    }

    private IEnumerator WaitForExecutionEnd()
    {
        yield return new WaitForSeconds((float)executionDirector.duration);
        Debug.Log("Egzekucja Zakoñczona!");
    }
}