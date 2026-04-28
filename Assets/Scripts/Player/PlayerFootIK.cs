using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerFootIK : MonoBehaviour
{
    private Animator animator;

    [Header("Ustawienia IK")]
    [Range(0, 1)] public float ikWeight = 1f;
    public LayerMask groundMask;

    [Header("Wymiary promieni (Raycast)")]
    public float raycastUpOffset = 1f;
    public float raycastDownDistance = 1.5f;

    [Header("Poprawki wizualne")]
    public float footOffset = 0.1f; // Grubosc buta

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, ikWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, ikWeight);
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, ikWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, ikWeight);

        HandleFootIK(AvatarIKGoal.LeftFoot);
        HandleFootIK(AvatarIKGoal.RightFoot);
    }

    private void HandleFootIK(AvatarIKGoal foot)
    {
        Vector3 footPosition = animator.GetIKPosition(foot);
        Quaternion footRotation = animator.GetIKRotation(foot);
        RaycastHit hit;
        Vector3 rayOrigin = footPosition + Vector3.up * raycastUpOffset;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, raycastDownDistance, groundMask))
        {
            Vector3 newFootPosition = footPosition;
            newFootPosition.y = hit.point.y + footOffset;
            animator.SetIKPosition(foot, newFootPosition);
            Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            animator.SetIKRotation(foot, surfaceRotation * footRotation);
        }
    }
}