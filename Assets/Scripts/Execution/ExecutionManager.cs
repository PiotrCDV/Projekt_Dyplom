using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections;

public class ExecutionManager : MonoBehaviour
{
    public static ExecutionManager Instance;

    [Header("Referencje Timeline")]
    public PlayableDirector executionDirector;
    public TimelineAsset executionTimelineAsset;

    [Header("Ustawienia Ścieżek")]
    public string playerTrackName = "PlayerTrack";
    public string enemyTrackName = "EnemyTrack";

    // --- NOWE: Ustawienia przestrzeni egzekucji ---
    [Header("Omijanie Ścian")]
    [Tooltip("Promień wolnego miejsca, jakiego potrzebuje kamera i animacja (np. 2 metry)")]
    public float executionSafeRadius = 2.0f;
    [Tooltip("Warstwa, na której znajdują się ściany areny")]
    public LayerMask obstacleLayer;
    // ----------------------------------------------

    // Zmienne do usypiania skryptów
    private CharacterController playerCC;
    private PlayerMovement playerMovementScript;
    private LockOnBehaviour playerLockOnScript;

    // Zmienna do kontrolowania przezroczystości kropki
    private CanvasGroup dotCanvasGroup;

    // Cache dla gracza
    private AnimationTrack cachedPlayerTrack;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void StartExecution(Animator playerAnim, Animator werewolfAnim)
    {
        // 1. POBIERAMY KOMPONENTY
        playerCC = playerAnim.GetComponent<CharacterController>();
        playerMovementScript = playerAnim.GetComponent<PlayerMovement>();
        playerLockOnScript = playerAnim.GetComponent<LockOnBehaviour>();

        // 2. MAGICZNE UKRYCIE KROPKI
        if (playerLockOnScript != null && playerLockOnScript.targetDotUI != null)
        {
            dotCanvasGroup = playerLockOnScript.targetDotUI.GetComponent<CanvasGroup>();
            if (dotCanvasGroup == null)
            {
                dotCanvasGroup = playerLockOnScript.targetDotUI.gameObject.AddComponent<CanvasGroup>();
            }
            dotCanvasGroup.alpha = 0f;
        }

        // 3. KNEBLUJEMY TYLKO RUCH GRACZA
        if (playerCC != null) playerCC.enabled = false;
        if (playerMovementScript != null) playerMovementScript.enabled = false;

        // --- NOWE: ODSUWANIE OD ŚCIANY ZANIM TIMELINE PRZEJMIE KONTROLĘ ---
        AdjustPositionsBeforeExecution(playerAnim.transform, werewolfAnim.transform);
        // ------------------------------------------------------------------

        // 4. TELEPORTACJA TIMELINE (Użyje już nowej, bezpiecznej pozycji wilkołaka)
        executionDirector.transform.position = werewolfAnim.transform.position;
        executionDirector.transform.rotation = werewolfAnim.transform.rotation;

        // 5. PRZYPISYWANIE I OFFSETY
        foreach (var track in executionTimelineAsset.GetOutputTracks())
        {
            if (track.name == playerTrackName)
            {
                executionDirector.SetGenericBinding(track, playerAnim);
                if (track is AnimationTrack animTrack)
                {
                    cachedPlayerTrack = animTrack;
                    animTrack.trackOffset = TrackOffset.ApplyTransformOffsets;
                    animTrack.position = werewolfAnim.transform.position;
                    animTrack.rotation = werewolfAnim.transform.rotation * Quaternion.Euler(0, 180f, 0);
                }
            }
            else if (track.name == enemyTrackName)
            {
                executionDirector.SetGenericBinding(track, werewolfAnim);
            }
        }

        playerAnim.Rebind();
        playerAnim.Update(0f);
        executionDirector.RebuildGraph();
        executionDirector.time = 0;
        executionDirector.Evaluate();

        executionDirector.stopped += OnExecutionFinished;
        executionDirector.Play();
    }

    // --- NOWE: FUNKCJA DO ODSUWANIA OD ŚCIAN ---
    private void AdjustPositionsBeforeExecution(Transform player, Transform werewolf)
    {
        // Skanujemy otoczenie wokół wilkołaka
        Collider[] walls = Physics.OverlapSphere(werewolf.position, executionSafeRadius, obstacleLayer);
        Vector3 pushOffset = Vector3.zero;

        foreach (Collider wall in walls)
        {
            Vector3 closestPoint = wall.ClosestPoint(werewolf.position);
            Vector3 directionFromWall = werewolf.position - closestPoint;
            directionFromWall.y = 0; // Ignorujemy oś Y

            float distanceToWall = directionFromWall.magnitude;

            if (distanceToWall < executionSafeRadius && distanceToWall > 0.01f)
            {
                float missingDistance = executionSafeRadius - distanceToWall;
                pushOffset += directionFromWall.normalized * missingDistance;
            }
        }

        // Jeśli trzeba, odsuwamy obie postacie synchronicznie
        if (pushOffset != Vector3.zero)
        {
            player.position += pushOffset;
            werewolf.position += pushOffset;
            Debug.Log($"[Egzekucja] Zbyt blisko ściany! Odsunięto postacie o {pushOffset.magnitude}m.");
        }
    }
    // ---------------------------------------------

    private void OnExecutionFinished(PlayableDirector director)
    {
        executionDirector.stopped -= OnExecutionFinished;

        if (playerLockOnScript != null)
        {
            if (playerLockOnScript.IsLocked)
            {
                playerLockOnScript.UnlockTarget();
            }
        }

        if (dotCanvasGroup != null)
        {
            dotCanvasGroup.alpha = 1f;
            dotCanvasGroup = null;
        }

        if (playerCC != null) playerCC.enabled = true;
        if (playerMovementScript != null) playerMovementScript.enabled = true;

        if (cachedPlayerTrack != null)
        {
            cachedPlayerTrack.position = Vector3.zero;
            cachedPlayerTrack.rotation = Quaternion.identity;
        }

        if (GameMessageUI.Instance != null)
        {
            GameMessageUI.Instance.ShowVictory();
        }

        Debug.Log("Egzekucja Zakończona!");
    }
}