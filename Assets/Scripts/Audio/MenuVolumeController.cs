using UnityEngine;
using UnityEngine.UI;

public class MenuVolumeController : MonoBehaviour
{
    [Header("Suwaki UI")]
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        if (AudioManager.Instance != null)
        {
            if (musicSlider != null)
            {
                musicSlider.value = AudioManager.Instance.GetMusicVolume();

                musicSlider.onValueChanged.RemoveAllListeners();
                musicSlider.onValueChanged.AddListener(SetMusicVol);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = AudioManager.Instance.GetSFXVolume();

                sfxSlider.onValueChanged.RemoveAllListeners();
                sfxSlider.onValueChanged.AddListener(SetSFXVol);
            }
        }
    }

    private void SetMusicVol(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
    }

    private void SetSFXVol(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
    }
}
