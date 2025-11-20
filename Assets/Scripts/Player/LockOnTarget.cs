using UnityEngine;

public class LockOnTarget : MonoBehaviour
{
    public Transform lookAtPoint;

    private void Reset()
    {
        lookAtPoint = transform;
    }
}
