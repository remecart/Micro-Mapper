using SFB;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using static Audio;

public class HitSoundManager : MonoBehaviour
{
    public static HitSoundManager instance;
    public List<AudioClip> hitsounds;
    public int hitsoundIndex;
    private float time;
    private bool playing;
    public bool cache;
    private float nextNote;
    public float length;
    public AudioSource audioSource;
    public GameObject hitsoundPrefab;

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
            Debug.LogError("Invalid path provided.");
            yield break;
        }

        string fileUrl = "file://" + path;

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileUrl, audioType))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            if (clip != null)
            {
                hitsounds[3] = clip;
                Settings.instance.config.audio.customSoundPath = path;
            }

        }
    }


    float cachevolume;

    void Update()
    {
        if (cachevolume != 0 && audioSource.volume == 0)
        {
            for (int i = this.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(this.transform.GetChild(i).gameObject);
            }

            cachevolume = audioSource.volume;
        }
        else if (cachevolume != audioSource.volume)
        {
            for (int i = this.transform.childCount - 1; i >= 0; i--)
            {
                this.transform.GetChild(i).GetComponent<AudioSource>().volume = audioSource.volume;
            }

            cachevolume = audioSource.volume;
        }

        playing = SpawnObjects.instance.playing;

        //time = SpawnObjects.instance.currentBeat;
        time = SpawnObjects.instance.BeatFromRealTime(SpawnObjects.instance.GetRealTimeFromBeat(SpawnObjects.instance.currentBeat) + 0.185f);

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
            if (time >= nextNote && cache)
            {
                GameObject go = Instantiate(hitsoundPrefab, this.transform);
                go.transform.SetParent(this.transform);
                var goAudio = go.GetComponent<AudioSource>();
                goAudio.volume = audioSource.volume;
                goAudio.clip = hitsounds[hitsoundIndex]; 
                goAudio.Play();
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                go.GetComponent<DestroyInTime>().time = hitsounds[hitsoundIndex].length + 0.125f;
                cache = false;
            }
        }
        else
        {
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