using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerFootIK : MonoBehaviour
{
    private Animator animator;

    [Header("Ustawienia G³ówne")]
    [Range(0, 1)] public float masterWeight = 1f;
    public LayerMask groundMask;
    public float footOffset = 0.1f;

    [Header("Zabezpieczenia (Anty-Deformacja)")]
    [Tooltip("Wysokoœæ wzglêdem postaci, przy której stopa jest ca³kowicie 'wypuszczona' z IK (podczas kroku)")]
    public float footLiftHeight = 0.25f;

    [Tooltip("Wysokoœæ, przy której stopa wraca na ziemiê i jest w pe³ni obs³ugiwana przez IK")]
    public float footGroundedHeight = 0.05f;

    [Tooltip("Maksymalny dopuszczalny k¹t wygiêcia kostki (zapobiega po³amaniu stóp na stromych górkach)")]
    public float maxAnkleAngle = 35f;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || masterWeight <= 0) return;

        ProcessFoot(AvatarIKGoal.LeftFoot);
        ProcessFoot(AvatarIKGoal.RightFoot);
    }

    private void ProcessFoot(AvatarIKGoal foot)
    {
        Vector3 animFootPos = animator.GetIKPosition(foot);
        Quaternion animFootRot = animator.GetIKRotation(foot);
        float heightFromRoot = animFootPos.y - transform.position.y;
        float dynamicWeight = Mathf.InverseLerp(footLiftHeight, footGroundedHeight, heightFromRoot);
        float finalWeight = masterWeight * dynamicWeight;

        animator.SetIKPositionWeight(foot, finalWeight);
        animator.SetIKRotationWeight(foot, finalWeight);

        if (finalWeight <= 0f) return;

        RaycastHit hit;
        Vector3 rayOrigin = animFootPos + Vector3.up * 1f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 1.5f, groundMask))
        {
            Vector3 newFootPos = animFootPos;
            newFootPos.y = hit.point.y + footOffset;
            animator.SetIKPosition(foot, newFootPos);
            Quaternion surfaceRot = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Quaternion targetFootRot = surfaceRot * animFootRot;
            Quaternion limitedRot = Quaternion.RotateTowards(animFootRot, targetFootRot, maxAnkleAngle);
            animator.SetIKRotation(foot, limitedRot);
        }
    }
}