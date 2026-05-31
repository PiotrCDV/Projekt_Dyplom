using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Œcie¿ki do kana³ów FMOD (do g³oœnoœci)")]
    public string musicBusPath = "bus:/Music";
    public string sfxBusPath = "bus:/SFX";

    private Bus musicBus;
    private Bus sfxBus;

    [Header("Instancja Muzyki")]
    private EventInstance musicInstance;

    private void Start()
    {
        float savedMusicVol = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        float savedSFXVol = PlayerPrefs.GetFloat("SFXVolume", 0.8f);

        SetMusicVolume(savedMusicVol);
        SetSFXVolume(savedSFXVol);
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        musicBus = RuntimeManager.GetBus(musicBusPath);
        sfxBus = RuntimeManager.GetBus(sfxBusPath);
    }

    public void PlayMusic(EventReference musicEvent)
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }

        musicInstance = RuntimeManager.CreateInstance(musicEvent);
        musicInstance.start();
    }

    public void StopMusic()
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }
    }


    public void PlaySFX(EventReference sfxEvent, Vector3 position = new Vector3())
    {
        if (!sfxEvent.IsNull)
        {
            RuntimeManager.PlayOneShot(sfxEvent, position);
        }
    }


    public void SetMusicVolume(float sliderValue)
    {
        musicBus.setVolume(sliderValue);
        PlayerPrefs.SetFloat("MusicVolume", sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        sfxBus.setVolume(sliderValue);
        PlayerPrefs.SetFloat("SFXVolume", sliderValue);
    }

    public float GetMusicVolume() => PlayerPrefs.GetFloat("MusicVolume", 0.8f);
    public float GetSFXVolume() => PlayerPrefs.GetFloat("SFXVolume", 0.8f);
}