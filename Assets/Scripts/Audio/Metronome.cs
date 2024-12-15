using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Metronome : MonoBehaviour
{
    public AudioSource audioSource;
    public static Metronome instance;
    public AudioClip clip;  
    public float audioDelay;
    
    void Start()
    {
        instance = this;
        audioSource.volume = Settings.instance.config.audio.metronome;
        audioDelay = Settings.instance.config.audio.audioDelay;
    }

    public void MetronomeSound()
    {
        if (SpawnObjects.instance.playing)
        {
            audioSource.volume = Settings.instance.config.audio.metronome;
            audioSource.PlayOneShot(clip);
        }
    }

    public void ChangeMetronomeVolume(Slider slider)
    {
        Settings.instance.config.audio.metronome = slider.value;
    }
}
