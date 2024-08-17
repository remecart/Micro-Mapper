using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitSoundManager : MonoBehaviour
{
    public static HitSoundManager instance;
    public List<AudioClip> hitsounds;
    public int hitsoundIndex;
    private float time;
    private bool playing;
    private bool cache;
    private float nextNote;
    public float volume;
    public float length;
    public AudioSource audioSource;

    void Start()
    {
        instance = this;
    }

    void Update()
    {
        playing = SpawnObjects.instance.playing;
        time = SpawnObjects.instance.currentBeat;

        if (length == 0)
        {
            if (LoadSong.instance.audioSource.clip) length = LoadSong.instance.audioSource.clip.length;
        }

        if (playing)
        {
            if (cache == false)
            {
                GetNextNote();
            }
            if (time >= nextNote && cache == true)
            {
                audioSource.PlayOneShot(hitsounds[hitsoundIndex]);
                audioSource.volume = volume;
                cache = false;
            }
        }
        else cache = false;
    }

    // if (playing)
    //     {
    //         if (cache == false)
    //         {
    //             GetNextNote();
    //         }

    //         if (time > nextNote && cache == true)
    //         {
    //             GameObject go = Instantiate(hitsound);
    //             go.transform.SetParent(content);
    //             AudioSource audioSource = go.AddComponent<AudioSource>();
    //             audioSource.clip = hitsounds[hitsoundIndex];
    //             audioSource.volume = volume;
    //             audioSource.Play();
    //             Destroy(go, hitsounds[hitsoundIndex].length + 0.05f);
    //             GetNextNote();
    //         }
    //     }
    //     else cache = false;

    void GetNextNote()
    {
        for (int i = 0; i < LoadMap.instance.beats.Count - Mathf.FloorToInt(time); i++)
        {
            if (time + i <= SpawnObjects.instance.BeatFromRealTime(length) && time + i >= 0)
            {
                List<colorNotes> notes = LoadMap.instance.beats[Mathf.FloorToInt(time + i)].colorNotes;
                foreach (var noteData in notes)
                {
                    if (noteData.b >= time)
                    {
                        nextNote = noteData.b;
                        cache = true;
                        return;
                    }
                }
            }
        }
    }
}