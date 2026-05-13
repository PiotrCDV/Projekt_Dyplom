using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCam != null)
        {
            transform.rotation = mainCam.transform.rotation;

            // Jeœli po u¿yciu powy¿szego kodu Twój tekst jest ODBITY W LUSTRZE (czytany od ty³u),
            // zablokuj górn¹ linijkê (dodaj // na pocz¹tku) i odblokuj tê doln¹:
            // transform.LookAt(transform.position + mainCam.transform.forward);
        }
    }
}