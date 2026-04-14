using UnityEngine;
using FMODUnity; // Dodane

public class LevelMusicSetter : MonoBehaviour
{
    // Zamiast AudioClip:
    public EventReference levelAmbient;

    void Start()
    {
        if (AudioManager.Instance != null && !levelAmbient.IsNull)
        {
            AudioManager.Instance.PlayMusic(levelAmbient);
        }
    }
}