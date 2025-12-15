using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class LockOnBehaviour : MonoBehaviour
{
    private Vector3 rootOffset;

    [Header("Ustawienia Namierzania")]
    public CinemachineCamera vcamLockOn;
    public float maxLockOnDistance = 20f;
    public LayerMask enemyLayer;
    public Animator animator;

    [Header("Dynamiczna Kamera (DeadZone + Damping)")]
    public float farDistance = 10f;
    public float closeDistance = 2f;

    [Space]
    public float farDeadZone = 0.15f;
    public float closeDeadZone = 0.6f;

    [Space]
    public float farDamping = 0.2f;
    public float closeDamping = 0f;

    [Header("UI Celu")]
    public RectTransform targetDotUI;
    [Range(1f, 50f)]
    public float uiSmoothSpeed = 20f;

    [Header("Skalowanie UI")]
    public bool useScale = true;
    public float minScale = 0.6f;
    public float maxScale = 1.2f;

    [Header("Camera Fix (Virtual Pivot)")]
    public Transform cameraRoot;

    private Transform currentTarget;
    private Camera mainCamera;

    private CinemachineRotationComposer rotationComposer;

    public bool IsLocked { get; private set; }
    public bool JustSwitched { get; private set; }

    private float switchTimer = 0f;
    private const float switchCooldown = 0.2f;

    public event Action OnUnlock;

    public CinemachineTargetGroup targetGroup;
    public Transform playerTransform;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (targetDotUI != null)
            targetDotUI.gameObject.SetActive(false);

        if (vcamLockOn != null)
        {
            rotationComposer = vcamLockOn.GetComponent<CinemachineRotationComposer>();
        }
        if (cameraRoot != null)
        {
            rootOffset = cameraRoot.position - transform.position;
            cameraRoot.transform.SetParent(null);
        }
    }

    private void Update()
    {
        if (JustSwitched)
        {
            switchTimer -= Time.deltaTime;
            if (switchTimer <= 0f)
                JustSwitched = false;
        }

        if (IsLocked && currentTarget != null && rotationComposer != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.position);
            float t = Mathf.InverseLerp(farDistance, closeDistance, distance);

            float newDeadZoneWidth = Mathf.Lerp(farDeadZone, closeDeadZone, t);

            var composition = rotationComposer.Composition;
            var deadZone = composition.DeadZone;
            deadZone.Size = new Vector2(newDeadZoneWidth, deadZone.Size.y);
            composition.DeadZone = deadZone;
            rotationComposer.Composition = composition;

            float newDamping = Mathf.Lerp(farDamping, closeDamping, t);

            rotationComposer.Damping.x = newDamping;
            rotationComposer.Damping.y = newDamping;
        }
    }

    private void LateUpdate()
    {
        HandleCameraRootRotation();

        if (IsLocked && currentTarget != null && targetDotUI != null)
        {
            Vector3 targetScreenPos = mainCamera.WorldToScreenPoint(currentTarget.position);

            if (targetScreenPos.z > 0)
            {
                if (!targetDotUI.gameObject.activeSelf)
                    targetDotUI.gameObject.SetActive(true);

                targetDotUI.position = Vector3.Lerp(targetDotUI.position, targetScreenPos, 1);

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

        if (targetGroup != null)
        {
            var targets = new List<CinemachineTargetGroup.Target>();

            targets.Add(new CinemachineTargetGroup.Target { Object = playerTransform, Weight = 1, Radius = 3 });

            targets.Add(new CinemachineTargetGroup.Target { Object = currentTarget, Weight = 3, Radius = 1.5f });

            targetGroup.Targets = targets;
        }
        if (vcamLockOn != null)
        {
            vcamLockOn.LookAt = targetGroup.transform;
        }

        if (animator != null) animator.SetBool("isLockedOn", true);

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

        if (targetGroup != null)
        {
            var targets = new List<CinemachineTargetGroup.Target>();
            targets.Add(new CinemachineTargetGroup.Target { Object = playerTransform, Weight = 1, Radius = 1 });
            targetGroup.Targets = targets;
        }
    }

    private void HandleCameraRootRotation()
    {
        if (cameraRoot == null) return;


        cameraRoot.position = transform.position + rootOffset;

        if (IsLocked && targetGroup != null)
        {
            Vector3 direction = targetGroup.transform.position - cameraRoot.position;
            direction.y = 0; 

            if (direction != Vector3.zero)
            {

                cameraRoot.rotation = Quaternion.LookRotation(direction);
            }
        }
        else
        {
            cameraRoot.rotation = Quaternion.RotateTowards(cameraRoot.rotation, transform.rotation, 720f * Time.deltaTime);
        }
    }
    private void OnDestroy()
    {
        if (cameraRoot != null) Destroy(cameraRoot.gameObject);
    }
}