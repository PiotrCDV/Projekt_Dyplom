using Unity.Cinemachine;
using UnityEngine;
using System;

public class LockOnBehaviour : MonoBehaviour
{
    [Header("Ustawienia Namierzania")]
    public CinemachineCamera vcamLockOn;
    public float maxLockOnDistance = 20f;
    public LayerMask enemyLayer;
    public Animator animator;

    [Header("UI Celu")]
    public RectTransform targetDotUI;
    [Range(1f, 50f)]
    public float uiSmoothSpeed = 20f;

    [Header("Skalowanie UI")]
    public bool useScale = true;
    public float minScale = 0.6f;
    public float maxScale = 1.2f;

    private Transform currentTarget;
    private Camera mainCamera;
    public bool IsLocked { get; private set; }
    public bool JustSwitched { get; private set; }

    private float switchTimer = 0f;
    private const float switchCooldown = 0.2f;

    public event Action OnUnlock;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (targetDotUI != null)
            targetDotUI.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (JustSwitched)
        {
            switchTimer -= Time.deltaTime;
            if (switchTimer <= 0f)
                JustSwitched = false;
        }
    }

    private void LateUpdate()
    {
        if (IsLocked && currentTarget != null && targetDotUI != null)
        {
            Vector3 targetScreenPos = mainCamera.WorldToScreenPoint(currentTarget.position);

            if (targetScreenPos.z > 0)
            {
                if (!targetDotUI.gameObject.activeSelf)
                    targetDotUI.gameObject.SetActive(true);

                targetDotUI.position = Vector3.Lerp(targetDotUI.position, targetScreenPos, Time.deltaTime * uiSmoothSpeed);

                if (useScale)
                {
                    float dist = Vector3.Distance(transform.position, currentTarget.position);
                    float t = Mathf.InverseLerp(2f, 15f, dist);
                    float scale = Mathf.Lerp(minScale, maxScale, t);
                    targetDotUI.localScale = Vector3.one * scale;
                }
            }
            else
            {
                targetDotUI.gameObject.SetActive(false);
            }
        }
    }

    public void ToggleLockOn()
    {
        if (IsLocked)
            UnlockTarget();
        else
            TryLockOnTarget();

        JustSwitched = true;
        switchTimer = switchCooldown;
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
            LockOnTarget targetScript = collider.GetComponentInParent<LockOnTarget>();
            if (targetScript == null) continue;

            Transform actualTargetTransform = targetScript.lookAtPoint;
            Vector3 screenPoint = mainCamera.WorldToViewportPoint(actualTargetTransform.position);

            if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
            {
                float distanceToCenter = Vector2.Distance(new Vector2(screenPoint.x, screenPoint.y), new Vector2(0.5f, 0.5f));
                if (distanceToCenter < minDistanceToScreenCenter)
                {
                    minDistanceToScreenCenter = distanceToCenter;
                    bestTarget = actualTargetTransform;
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

        if (targetDotUI != null)
        {
            Vector3 startPos = mainCamera.WorldToScreenPoint(target.position);
            targetDotUI.position = startPos;
            targetDotUI.gameObject.SetActive(true);
        }
    }

    private void UnlockTarget()
    {
        currentTarget = null;
        IsLocked = false;
        if (vcamLockOn != null)
            vcamLockOn.LookAt = null;
        if (animator != null)
            animator.SetBool("isLockedOn", false);

        if (targetDotUI != null)
            targetDotUI.gameObject.SetActive(false);

        OnUnlock?.Invoke();
    }
}