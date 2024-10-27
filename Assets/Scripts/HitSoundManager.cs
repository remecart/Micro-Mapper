using SFB;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using static Audio;

public class HitSoundManager : MonoBehaviour
{
    public static HitSoundManager instance;
    public List<AudioClip> hitsounds;
    public int hitsoundIndex;
    private float time;
    private bool playing;
    private bool cache;
    private float nextNote;
    public float length;
    public AudioSource audioSource;
    public GameObject hitsound;
    
    void Start()
    {
        instance = this;
        audioSource.volume = Settings.instance.config.audio.hitsound;
        hitsoundIndex = Settings.instance.config.audio.hitsounds switch
        {
            Hitsound.MicroMapper => 0,
            Hitsound.Rabbit => 1,
            Hitsound.osu => 2,
            _ => 3
        };

        AudioType type = LoadSong.instance.GetAudioTypeFromExtension(Settings.instance.config.audio.customSoundPath);
        StartCoroutine(LoadAudioFile(Settings.instance.config.audio.customSoundPath, type));
    }

    public void OpenFilePicker()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select a File", "", "ogg", false);
        string path = paths[0];

        if (!string.IsNullOrEmpty(path))
        {
            AudioType type = LoadSong.instance.GetAudioTypeFromExtension(path);
            StartCoroutine(LoadAudioFile(path, type));
        }
    }

    public IEnumerator LoadAudioFile(string path, AudioType audioType)
    {
        if (string.IsNullOrEmpty(path))
        {
            yield break;
        }

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, audioType))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.ConnectionError || www.result != UnityWebRequest.Result.ProtocolError)
            {
                hitsounds[3] = DownloadHandlerAudioClip.GetContent(www);
                Settings.instance.config.audio.customSoundPath = path;
            }
        }
    }

    void Update()
    {
        playing = SpawnObjects.instance.playing;

        //time = SpawnObjects.instance.currentBeat;
        time = SpawnObjects.instance.BeatFromRealTime(SpawnObjects.instance.GetRealTimeFromBeat(SpawnObjects.instance.currentBeat) + 0.185f);

        if (length == 0)
        {
            if (LoadSong.instance.audioSource.clip) length = LoadSong.instance.audioSource.clip.length;
        }

        if (playing)
        {
            if (cache == false )
            {
                GetNextNote();
            }
            if (time >= nextNote && cache == true)
            {
                //audioSource.PlayOneShot(hitsounds[hitsoundIndex]);
                GameObject go = Instantiate(hitsound);
                go.transform.parent = this.transform;
                go.GetComponent<AudioSource>().clip = hitsounds[hitsoundIndex];
                go.GetComponent<AudioSource>().volume = audioSource.volume;
                go.GetComponent<AudioSource>().Play();
                go.GetComponent<DestroyInTime>().time = hitsounds[hitsoundIndex].length + 0.125f;
                cache = false;
            }
        }
        else {
            cache = false;
            audioSource.Stop();
        }
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
        for (int i = 0; i < 4; i++)
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