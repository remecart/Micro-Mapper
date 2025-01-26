using SFB;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    public GameObject hitsoundPrefab;
    private float cacheVolume;
    public float audioDelay;

    void Start()
    {
        Time.fixedDeltaTime = 0.00277777777777778f;
        instance = this;
        audioSource.volume = Settings.instance.config.audio.timing;

        hitsoundIndex = Settings.instance.config.audio.timings switch
        {
            Hitsound.MicroMapper => 0,
            Hitsound.Rabbit => 1,
            Hitsound.osu => 2,
            _ => 3
        };

        string customTimingSoundPath = Settings.instance.config.audio.customTimingSoundPath;
        AudioType type = LoadSong.instance.GetAudioTypeFromExtension(customTimingSoundPath);
        StartCoroutine(LoadAudioFile(customTimingSoundPath, type));

        audioDelay = Settings.instance.config.audio.audioDelay;
    }

    public void OpenFilePicker()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select a File", "", "ogg", false);
        if (paths.Length == 0 || string.IsNullOrEmpty(paths[0])) return;

        string selectedPath = paths[0];
        string destinationFolder = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
            "Micro Mapper",
            "TimingSounds"
        );

        Directory.CreateDirectory(destinationFolder);
        string destinationPath = Path.Combine(destinationFolder, Path.GetFileName(selectedPath));

        try
        {
            File.Copy(selectedPath, destinationPath, overwrite: true);
            Debug.Log($"File copied to: {destinationPath}");

            AudioType type = LoadSong.instance.GetAudioTypeFromExtension(destinationPath);
            StartCoroutine(LoadAudioFile(destinationPath, type));
        }
        catch (IOException ex)
        {
            Debug.LogError($"Failed to copy file: {ex.Message}");
        }
    }

    public IEnumerator LoadAudioFile(string path, AudioType audioType)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Invalid custom timing sound path provided.");
            yield break;
        }

        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, audioType);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            yield break;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
        if (clip != null)
        {
            timingsounds[3] = clip;
            Settings.instance.config.audio.customTimingSoundPath = path;
        }
    }

    void FixedUpdate()
    {
        HandleVolumeChange();
        ClearHitsoundsOnSpace();

        playing = SpawnObjects.instance.playing;
        UpdateTimeAndLength();

        if (playing)
        {
            HandleHitsoundPlayback();
        }
        else
        {
            cache = false;
            audioSource.Stop();
        }
    }

    private void HandleVolumeChange()
    {
        if (cacheVolume != 0 && audioSource.volume == 0)
        {
            ClearAllHitsounds();
            cacheVolume = audioSource.volume;
        }
        else if (cacheVolume != audioSource.volume)
        {
            foreach (Transform child in transform)
            {
                child.GetComponent<AudioSource>().volume = audioSource.volume;
            }
            cacheVolume = audioSource.volume;
        }
    }

    private void ClearHitsoundsOnSpace()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClearAllHitsounds();
        }
    }

    private void ClearAllHitsounds()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void UpdateTimeAndLength()
    {
        time = SpawnObjects.instance.BeatFromRealTime(
            SpawnObjects.instance.GetRealTimeFromBeat(SpawnObjects.instance.currentBeat) + 0.185f + audioDelay
        );

        if (length == 0 && LoadSong.instance.audioSource.clip)
        {
            length = LoadSong.instance.audioSource.clip.length;
        }
    }

    private void HandleHitsoundPlayback()
    {
        if (!cache)
        {
            GetNextNote();
        }

        if (cache && time >= nextNote)
        {
            PlayHitsound();
        }
    }

    private void PlayHitsound()
    {
        GameObject go = Instantiate(hitsoundPrefab, transform);
        var goAudio = go.GetComponent<AudioSource>();
        goAudio.volume = audioSource.volume;
        goAudio.clip = timingsounds[hitsoundIndex];
        goAudio.Play();
        go.GetComponent<DestroyInTime>().time = timingsounds[hitsoundIndex].length;
        cache = false;
    }

    private void GetNextNote()
    {
        for (int i = 0; i < 4; i++)
        {
            float targetBeat = time + i;
            if (targetBeat <= SpawnObjects.instance.BeatFromRealTime(length) && targetBeat >= 0)
            {
                List<timings> timings = LoadMap.instance.beats[Mathf.FloorToInt(targetBeat)].timings;
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
