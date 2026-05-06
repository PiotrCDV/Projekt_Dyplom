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
        // 1. Pobieramy pozycjê stopy WPROST Z ANIMACJI (zanim nadpisze j¹ IK)
        Vector3 animFootPos = animator.GetIKPosition(foot);
        Quaternion animFootRot = animator.GetIKRotation(foot);

        // 2. MAGIA: Dynamiczna waga IK
        // Obliczamy jak wysoko nad ziemi¹ znajduje siê stopa w oryginalnej animacji
        float heightFromRoot = animFootPos.y - transform.position.y;

        // Funkcja InverseLerp robi tu genialn¹ robotê:
        // Jeœli stopa jest wysoko (0.25) -> dynamicWeight = 0 (IK przestaje dzia³aæ, noga mo¿e swobodnie lecieæ w powietrzu)
        // Jeœli stopa opada (0.05) -> dynamicWeight = 1 (IK znowu przykleja j¹ do ziemi)
        // Wartoœci pomiêdzy s¹ p³ynnie skalowane, co zapobiega "szarpaniu".
        float dynamicWeight = Mathf.InverseLerp(footLiftHeight, footGroundedHeight, heightFromRoot);
        float finalWeight = masterWeight * dynamicWeight;

        animator.SetIKPositionWeight(foot, finalWeight);
        animator.SetIKRotationWeight(foot, finalWeight);

        // Jeœli finalna waga wynosi 0 (stopa jest w powietrzu), przerywamy dalsze liczenie dla optymalizacji
        if (finalWeight <= 0f) return;

        // 3. Wystrzelenie lasera (Raycast)
        RaycastHit hit;
        Vector3 rayOrigin = animFootPos + Vector3.up * 1f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 1.5f, groundMask))
        {
            // --- POZYCJA ---
            Vector3 newFootPos = animFootPos;
            newFootPos.y = hit.point.y + footOffset;
            animator.SetIKPosition(foot, newFootPos);

            // --- ROTACJA (Z limitem ³amania koœci) ---
            Quaternion surfaceRot = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Quaternion targetFootRot = surfaceRot * animFootRot;

            // Ograniczamy brutalne wyginanie kostki za pomoc¹ RotateTowards
            // Stopa wygnie siê tylko o maksymalnie 35 stopni, nie ³ami¹c siê na ostrych krawêdziach
            Quaternion limitedRot = Quaternion.RotateTowards(animFootRot, targetFootRot, maxAnkleAngle);
            animator.SetIKRotation(foot, limitedRot);
        }
    }
}