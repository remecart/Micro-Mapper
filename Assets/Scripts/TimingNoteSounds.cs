using SFB;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using static Audio;

public class TimingNoteManager : MonoBehaviour
{
    public static TimingNoteManager instance;
    public List<AudioClip> timingsounds;
    public int hitsoundIndex;
    private float time;
    private bool playing;
    private bool cache;
    private float nextNote;
    public float length;
    public AudioSource audioSource;
    public GameObject hitsound;
    float cachevolume;

    void Start()
    {
        instance = this;
        audioSource.volume = Settings.instance.config.audio.timing;
        hitsoundIndex = Settings.instance.config.audio.timings switch
        {
            Hitsound.MicroMapper => 0,
            Hitsound.Rabbit => 1,
            Hitsound.osu => 2,
            _ => 3
        };

        AudioType type = LoadSong.instance.GetAudioTypeFromExtension(Settings.instance.config.audio.customTimingSoundPath);
        StartCoroutine(LoadAudioFile(Settings.instance.config.audio.customTimingSoundPath, type));
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
                timingsounds[3] = DownloadHandlerAudioClip.GetContent(www);
                Settings.instance.config.audio.customTimingSoundPath = path;
            }
        }
    }

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
            if (time >= nextNote && cache == true)
            {
                //audioSource.PlayOneShot(hitsounds[hitsoundIndex]);
                GameObject go = Instantiate(hitsound);
                go.transform.parent = this.transform;
                go.GetComponent<AudioSource>().clip = timingsounds[hitsoundIndex];
                go.GetComponent<AudioSource>().volume = audioSource.volume;
                go.GetComponent<AudioSource>().Play();
                go.GetComponent<DestroyInTime>().time = timingsounds[hitsoundIndex].length + 0.125f;
                cache = false;
            }
        }
        else
        {
            cache = false;
            audioSource.Stop();
        }
    }



    void GetNextNote()
    {
        for (int i = 0; i < 4; i++)
        {
            if (time + i <= SpawnObjects.instance.BeatFromRealTime(length) && time + i >= 0)
            {
                List<timings> timings = LoadMap.instance.beats[Mathf.FloorToInt(time + i)].timings;

                foreach (var timingData in timings)
                {
                    if (timingData.b >= time)
                    {
                        nextNote = timingData.b;
                        cache = true;
                        return;
                    }
                }
            }
        }
    }
}