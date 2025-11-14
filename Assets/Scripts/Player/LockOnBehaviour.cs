using Unity.Cinemachine;
using UnityEngine;

public class LockOnBehaviour : MonoBehaviour
{
    public CinemachineCamera vcamLockOn;
    public float maxLockOnDistance = 20f;
    public LayerMask enemyLayer;
    public Animator animator;

    private Transform currentTarget;
    private Camera mainCamera;
    public bool IsLocked { get; private set; }

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void ToggleLockOn()
    {
        if (IsLocked)
            UnlockTarget();
        else
            TryLockOnTarget();
    }

    public void HandleLockOnState(Vector3 playerPosition)
    {
        if (IsLocked && currentTarget != null)
        {
            float distance = Vector3.Distance(playerPosition, currentTarget.position);
            if (distance > maxLockOnDistance || !currentTarget.gameObject.activeInHierarchy)
                UnlockTarget();
        }
    }

    public Transform GetCurrentTarget() => currentTarget;

    private void TryLockOnTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, maxLockOnDistance, enemyLayer);
        Transform bestTarget = null;
        float minDistanceToScreenCenter = float.MaxValue;

        foreach (Collider collider in colliders)
        {
            Vector3 screenPoint = mainCamera.WorldToViewportPoint(collider.transform.position);
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

        if (bestTarget != null)
            LockOn(bestTarget);
    }

    private void LockOn(Transform target)
    {
        currentTarget = target;
        IsLocked = true;
        if (vcamLockOn != null)
            vcamLockOn.LookAt = currentTarget;
        if (animator != null)
            animator.SetBool("isLockedOn", true);
    }

    private void UnlockTarget()
    {
        currentTarget = null;
        IsLocked = false;
        if (vcamLockOn != null)
            vcamLockOn.LookAt = null;
        if (animator != null)
            animator.SetBool("isLockedOn", false);
    }
}
