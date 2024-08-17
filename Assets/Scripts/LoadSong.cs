using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using UnityEngine;
using UnityEngine.Networking;

public class LoadSong : MonoBehaviour
{
    public static LoadSong instance;
    public AudioSource audioSource;
    public float timeInSeconds;
    public float timeInBeats;

    void Start()
    {
        instance = this;
        SetAudioClip(audioSource);
        StopSong();
    }

    public void SetAudioClip(AudioSource source)
    {
        string songPath = null;
        if (EditMetaData.instance != null) songPath = Path.Combine(EditMetaData.instance.folderPath, EditMetaData.instance.metaData._songFilename);
        else songPath = Path.Combine(ReadMapInfo.instance.folderPath, ReadMapInfo.instance.info._songFilename);
        StartCoroutine(LoadAudioClip(songPath, source, GetAudioTypeFromExtension(songPath)));
    }

    private AudioType GetAudioTypeFromExtension(string path)
    {
        string extension = System.IO.Path.GetExtension(path).ToLower();
        switch (extension)
        {
            case ".ogg":
                return AudioType.OGGVORBIS;
            case ".egg":
                return AudioType.OGGVORBIS;
            case ".wav":
                return AudioType.WAV;
            default:
                Debug.LogWarning("Unknown audio type for extension: " + extension);
                return AudioType.UNKNOWN;
        }
    }

    private IEnumerator LoadAudioClip(string path, AudioSource src, AudioType audioType)
    {
        string url = "file:///" + path;
        Debug.Log("Loading audio from URL: " + url); // Log the URL to debug

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error loading audio: " + www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                src.clip = clip;

                timeInSeconds = src.clip.length;

                if (LoadMap.instance != null)
                {
                    LoadMap.instance.IncreaseBeats(clip);
                    LoadMap.instance.Load(src);
                }
            }
        }
    }

    public void Offset(float offset)
    {
        if (audioSource.clip != null)
        {
            if (offset >= 0 && offset <= audioSource.clip.length)
            {
                audioSource.time = offset;
                audioSource.Play();
            }
        }
        else
        {
            Debug.LogWarning("No audio clip loaded.");
        }
    }

    public void StopSong()
    {
        audioSource.Stop();
        StopCoroutine("AnalyzeSamples");
    }
}
