using UnityEngine;

public class LevelMusicSetter : MonoBehaviour
{
    public AudioClip levelAmbient;

    void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(levelAmbient);
        }
    }
}
