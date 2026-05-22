using UnityEngine;
using FMODUnity; // Wymagane do obs³ugi FMOD
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Œcie¿ki do kana³ów FMOD (do g³oœnoœci)")]
    // W FMOD domyœlnie s¹ takie œcie¿ki. Zmienisz je pod swoje potrzeby póŸniej.
    public string musicBusPath = "bus:/Music";
    public string sfxBusPath = "bus:/SFX";

    private Bus musicBus;
    private Bus sfxBus;

    [Header("Instancja Muzyki")]
    // Muzyka musi byæ zapisana jako "Instancja", ¿ebyœmy mogli j¹ zapêtlaæ i zatrzymywaæ
    private EventInstance musicInstance;

    private void Start()
    {
        // Wczytujemy zapisan¹ g³oœnoœæ przy starcie gry (domyœlnie 0.8 czyli 80%)
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
            return; // Dodane return, ¿eby po zniszczeniu kod nie lecia³ dalej
        }

        // Pobieramy referencje do kana³ów g³oœnoœci z FMOD-a
        musicBus = RuntimeManager.GetBus(musicBusPath);
        sfxBus = RuntimeManager.GetBus(sfxBusPath);
    }

    // Odtwarzanie Muzyki (Loop)
    public void PlayMusic(EventReference musicEvent)
    {
        // Jeœli jakaœ muzyka ju¿ gra, zatrzymaj j¹ p³ynnie (FADEOUT)
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }

        // Stwórz now¹ muzykê i j¹ odpal
        musicInstance = RuntimeManager.CreateInstance(musicEvent);
        musicInstance.start();
    }

    // Odtwarzanie DŸwiêków (Pojedyncze strza³y / uderzenia)
    // Dodano pozycjê, dziêki czemu FMOD wie, z której strony odtworzyæ dŸwiêk 3D!
    public void PlaySFX(EventReference sfxEvent, Vector3 position = new Vector3())
    {
        if (!sfxEvent.IsNull)
        {
            // FMOD sam tworzy dŸwiêk w danym miejscu, gra go i po cichu sprz¹ta z pamiêci
            RuntimeManager.PlayOneShot(sfxEvent, position);
        }
    }

    // FMOD u¿ywa prostej skali g³oœnoœci: 0.0 (cisza) do 1.0 (max)
    // Nie trzeba ju¿ ¿adnej logarytmicznej matematyki!
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