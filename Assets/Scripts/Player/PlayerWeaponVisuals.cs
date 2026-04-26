using UnityEngine;

public class PlayerWeaponVisuals : MonoBehaviour
{
    [Header("Weapon GameObjects")]
    public GameObject swordInHand;
    
    public GameObject swordOnBelt;

    private void Start()
    {
        ShowHolstered();
    }

    public void OnEquipWeapon()
    {
        if (swordInHand != null && swordOnBelt != null)
        {
            swordInHand.SetActive(true);
            swordOnBelt.SetActive(false);
        }
    }

    public void OnHolsterWeapon()
    {
        if (swordInHand != null && swordOnBelt != null)
        {
            swordInHand.SetActive(false);
            swordOnBelt.SetActive(true);
        }
    }

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