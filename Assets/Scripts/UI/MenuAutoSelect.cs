using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class MenuAutoSelect : MonoBehaviour
{
    [Tooltip("Przycisk, który ma zostaæ wybrany")]
    public GameObject firstSelectedButton;

    private void OnEnable()
    {
        // Odpala siê automatycznie, gdy panel jest w³¹czany
        ForceSelectButton();
    }

    // Dodana publiczna funkcja, któr¹ mo¿emy podpi¹æ pod inne przyciski!
    public void ForceSelectButton()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(SelectButtonRoutine());
        }
    }

    private IEnumerator SelectButtonRoutine()
    {
        yield return null;

        if (EventSystem.current != null && firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }
}