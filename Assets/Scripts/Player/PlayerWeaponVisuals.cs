using UnityEngine;

public class PlayerWeaponVisuals : MonoBehaviour
{
    [Header("Weapon GameObjects")]
    [Tooltip("Miecz przypięty do kości dłoni (Right Hand)")]
    public GameObject swordInHand;
    
    [Tooltip("Miecz przypięty do kości pasa lub pleców (Belt/Spine)")]
    public GameObject swordOnBelt;

    private void Start()
    {
        // Na starcie upewniamy się, że stan jest poprawny (miecz na pasie)
        ShowHolstered();
    }

    // --- FUNKCJE DLA ANIMATION EVENTS ---
    // Wywołaj to w klatce, gdy dłoń dotyka rękojeści na pasie
    public void OnEquipWeapon()
    {
        if (swordInHand != null && swordOnBelt != null)
        {
            swordInHand.SetActive(true);
            swordOnBelt.SetActive(false);
            // Debug.Log("Miecz wyciągnięty do dłoni");
        }
    }

    // Wywołaj to w klatce, gdy dłoń puszcza miecz na pasie
    public void OnHolsterWeapon()
    {
        if (swordInHand != null && swordOnBelt != null)
        {
            swordInHand.SetActive(false);
            swordOnBelt.SetActive(true);
            // Debug.Log("Miecz schowany na pas");
        }
    }

    // Metody pomocnicze do ręcznego ustawiania stanu (np. na początku gry)
    public void ShowEquipped()
    {
        swordInHand.SetActive(true);
        swordOnBelt.SetActive(false);
    }

    public void ShowHolstered()
    {
        swordInHand.SetActive(false);
        swordOnBelt.SetActive(true);
    }
}