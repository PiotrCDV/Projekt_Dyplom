using UnityEngine;

public class FogCloseTrigger : MonoBehaviour
{
    public BossGateController controller;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            controller.CloseGate();
            gameObject.SetActive(false);
        }
    }
}