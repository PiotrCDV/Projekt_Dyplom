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
    public string playerTrackName = "PlayerTrack";
    public string enemyTrackName = "EnemyTrack";

    // Zmienne do usypiania skryptów
    private CharacterController playerCC;
    private PlayerMovement playerMovementScript;

    // Cache TYLKO dla gracza, bo tylko jemu modyfikujemy offsety
    private AnimationTrack cachedPlayerTrack;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void StartExecution(Animator playerAnim, Animator werewolfAnim)
    {
        // 1. KNEBLUJEMY SKRYPTY RUCHU GRACZA
        playerCC = playerAnim.GetComponent<CharacterController>();
        if (playerCC != null) playerCC.enabled = false;

        playerMovementScript = playerAnim.GetComponent<PlayerMovement>();
        if (playerMovementScript != null) playerMovementScript.enabled = false;

        // 2. TEPAMY CAŁY OBIEKT TIMELINE DO WILKOŁAKA
        // Kamera leci za nim, bo jest jego dzieckiem
        executionDirector.transform.position = werewolfAnim.transform.position;
        executionDirector.transform.rotation = werewolfAnim.transform.rotation;

        // 3. PRZYPISUJEMY ŚCIEŻKI
        foreach (var track in executionTimelineAsset.GetOutputTracks())
        {
            if (track.name == playerTrackName)
            {
                executionDirector.SetGenericBinding(track, playerAnim);
                if (track is AnimationTrack animTrack)
                {
                    cachedPlayerTrack = animTrack;
                    // Wymuszamy pozycję TYLKO dla Humanoida (gracza)
                    animTrack.trackOffset = TrackOffset.ApplyTransformOffsets;
                    animTrack.position = werewolfAnim.transform.position;
                    animTrack.rotation = werewolfAnim.transform.rotation * Quaternion.Euler(0, 180f, 0);
                }
            }
            else if (track.name == enemyTrackName)
            {
                // Wilkołak dostaje tylko powiązanie. Żadnego grzebania w offsetach!
                executionDirector.SetGenericBinding(track, werewolfAnim);
            }
        }

        // Twardy reset Animatora gracza
        playerAnim.Rebind();
        playerAnim.Update(0f);

        executionDirector.RebuildGraph();

        executionDirector.time = 0;
        executionDirector.Evaluate();

        executionDirector.stopped += OnExecutionFinished;
        executionDirector.Play();
    }

    private void OnExecutionFinished(PlayableDirector director)
    {
        executionDirector.stopped -= OnExecutionFinished;

        // 4. BUDZIMY GRACZA PO EGZEKUCJI
        if (playerCC != null) playerCC.enabled = true;
        if (playerMovementScript != null) playerMovementScript.enabled = true;

        // 5. CZYŚCIMY OFFSETY GRACZA
        if (cachedPlayerTrack != null)
        {
            cachedPlayerTrack.position = Vector3.zero;
            cachedPlayerTrack.rotation = Quaternion.identity;
        }

        Debug.Log("Egzekucja Zakończona!");
    }
}