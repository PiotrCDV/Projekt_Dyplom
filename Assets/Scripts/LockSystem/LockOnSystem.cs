using Unity.Cinemachine; // Musimy to dodaæ, aby mieæ dostêp do kamer Cinemachine
using UnityEngine;

public class LockOnSystem : MonoBehaviour
{
    [Header("Ustawienia Kamery")]
    // Przeci¹gnij tu swoj¹ kamerê "vcam_LockOn"
    public CinemachineCamera vcamLockOn;

    [Header("Ustawienia Namierzania")]
    // Jak daleko system ma szukaæ wrogów
    public float maxLockOnDistance = 20f;
    // Na jakiej warstwie (Layer) znajduj¹ siê wrogowie
    public LayerMask enemyLayer;

    // --- Zmienne prywatne ---
    private Animator animator;
    private Transform currentTarget;
    private bool isLocked = false;
    private Camera mainCamera;

    void Start()
    {
        // Automatycznie pobierz Animator z obiektu gracza
        animator = GetComponent<Animator>();
        // Pobierz g³ówn¹ kamerê
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 1. SprawdŸ, czy gracz wcisn¹³ przycisk namierzania
        // (U¿ywamy "F". Mo¿esz zmieniæ na "Input Manager" lub "Input System")
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isLocked)
            {
                // Jeœli ju¿ namierzamy -> Wy³¹cz namierzanie
                UnlockTarget();
            }
            else
            {
                // Jeœli nie namierzamy -> Spróbuj znaleŸæ cel
                TryLockOnTarget();
            }
        }

        // 2. Automatyczne wy³¹czenie, jeœli cel jest za daleko lub nie ¿yje
        if (isLocked && currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.position);

            // Jeœli cel uciek³ za daleko LUB zosta³ wy³¹czony (np. umar³)
            if (distance > maxLockOnDistance || !currentTarget.gameObject.activeInHierarchy)
            {
                UnlockTarget();
            }
        }
    }

    private void TryLockOnTarget()
    {
        // Wystrzel kulê z fizyki, aby znaleŸæ wszystkich wrogów w zasiêgu
        Collider[] colliders = Physics.OverlapSphere(transform.position, maxLockOnDistance, enemyLayer);

        Transform bestTarget = null;
        float minDistanceToScreenCenter = float.MaxValue;

        foreach (Collider collider in colliders)
        {
            // SprawdŸ, który wróg jest najbli¿ej œrodka ekranu
            Vector3 screenPoint = mainCamera.WorldToViewportPoint(collider.transform.position);

            // SprawdŸ, czy cel jest przed kamer¹ i widoczny na ekranie
            if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
            {
                float distanceToCenter = Vector2.Distance(new Vector2(screenPoint.x, screenPoint.y), new Vector2(0.5f, 0.5f));

                if (distanceToCenter < minDistanceToScreenCenter)
                {
                    minDistanceToScreenCenter = distanceToCenter;
                    bestTarget = collider.transform;
                }
            }
        }

        // Jeœli znaleŸliœmy jakikolwiek pasuj¹cy cel
        if (bestTarget != null)
        {
            LockOn(bestTarget);
        }
    }

    private void LockOn(Transform target)
    {
        currentTarget = target;
        isLocked = true;

        // 1. Powiedz kamerze vcam_LockOn, na co ma patrzeæ
        vcamLockOn.LookAt = currentTarget;

        // 2. Powiedz Animatorowi, ¿eby prze³¹czy³ stan na "isLockedOn"
        // To automatycznie aktywuje vcam_LockOn przez StateDrivenCamera
        animator.SetBool("isLockedOn", true);
    }

    private void UnlockTarget()
    {
        currentTarget = null;
        isLocked = false;

        // 1. Wyczyœæ cel z kamery
        vcamLockOn.LookAt = null;

        // 2. Powiedz Animatorowi, ¿eby wróci³ do stanu FreeLook
        animator.SetBool("isLockedOn", false);
    }
}