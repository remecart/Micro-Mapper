using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public void SongVolumeChange(Slider slider)
    {
        GetComponent<AudioSource>().volume = slider.value;
        Settings.instance.config.audio.music = slider.value;
    }

    public void HitsoundVolumeChange(Slider slider)
    {
        HitSoundManager.instance.volume = slider.value;
        Settings.instance.config.audio.hitsound = slider.value;
    }
}
