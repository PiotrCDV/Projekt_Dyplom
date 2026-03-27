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

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void StartExecution(Animator playerAnim, Animator werewolfAnim)
    {
        executionDirector.transform.position = werewolfAnim.transform.position;
        executionDirector.transform.rotation = werewolfAnim.transform.rotation;

        foreach (var track in executionTimelineAsset.GetOutputTracks())
        {
            if (track.name == playerTrackName)
                executionDirector.SetGenericBinding(track, playerAnim);
            else if (track.name == enemyTrackName)
                executionDirector.SetGenericBinding(track, werewolfAnim);
        }

        executionDirector.time = 0;
        executionDirector.Evaluate();

        executionDirector.stopped += OnExecutionFinished;

        executionDirector.Play();
    }

    private void OnExecutionFinished(PlayableDirector director)
    {
        executionDirector.stopped -= OnExecutionFinished;

        Debug.Log("Egzekucja Zakończona na 100%! Timeline posprzątał kamery.");

    }
}