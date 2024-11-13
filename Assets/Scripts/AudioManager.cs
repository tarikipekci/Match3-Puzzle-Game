using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource[] musicSources, soundEffects;

    public void SetMusicSoundLevel(float soundLevel)
    {
        foreach (var music in musicSources)
        {
            music.volume = soundLevel;
        }
    }

    public float GetMusicSoundLevel()
    {
        return musicSources[0].volume;
    }

    public void SetAmbientSoundLevel(float soundLevel)
    {
        foreach (var sound in soundEffects)
        {
            sound.volume = soundLevel;
        }
    }

    public float GetAmbientSoundLevel()
    {
        return soundEffects[0].volume;
    }

    public void SetGeneralSoundLevel(float soundLevel)
    {
        AudioListener.volume = soundLevel;
    }
}