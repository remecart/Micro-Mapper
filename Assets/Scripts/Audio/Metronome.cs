using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Metronome : MonoBehaviour
{
    public AudioSource audioSource;
    public static Metronome instance;
    public AudioClip clip;

    void Start()
    {
        instance = this;
        audioSource.volume = Settings.instance.config.audio.metronome;
    }

    public void MetronomeSound()
    {
        audioSource.volume = Settings.instance.config.audio.metronome;
        audioSource.PlayOneShot(clip);
    }

    public void ChangeMetronomeVolume(Slider slider)
    {
        Settings.instance.config.audio.metronome = slider.value;
    }
}
