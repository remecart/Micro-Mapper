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
    public float songSpeed;
    private bool check;
    public bool dontLoadAudio;
    public float audioDelay;
    
    void Start()
    {
        if (dontLoadAudio) {}
        instance = this;
        if (Settings.instance != null ) {
            audioSource.volume = Settings.instance.config.audio.music;
            songSpeed = Settings.instance.config.mapping.songSpeed / 10f;
            audioDelay = Settings.instance.config.audio.audioDelay;
        }
        SetAudioClip(audioSource);
        StopSong();
    }

    void Update()
    {
        if (audioSource.clip&& check == false) { 
            check = true;
            if (LoadMap.instance) LoadMap.instance.Load(audioSource);
        }
    }
    public void SetAudioClip(AudioSource source)
    {
        string songPath = null;
        if (EditMetaData.instance != null) songPath = Path.Combine(EditMetaData.instance.folderPath, EditMetaData.instance.metaData._songFilename);
        else songPath = Path.Combine(ReadMapInfo.instance.folderPath, ReadMapInfo.instance.info._songFilename);
        StartCoroutine(LoadAudioClip(songPath, source, GetAudioTypeFromExtension(songPath)));
    }

    public AudioType GetAudioTypeFromExtension(string path)
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
                return AudioType.UNKNOWN;
        }
    }

    public IEnumerator LoadAudioClip(string path, AudioSource src, AudioType audioType)
    {
        string url = "file:///" + path;

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

                //if (LoadMap.instance != null)
                //{
                //    LoadMap.instance.Load(src);
                //}

                ApplySongSpeed(); // Apply speed settings once the audio is loaded
            }
        }
    }


    public void Offset(float offset)
    {
        if (audioSource.clip)
        {
            if (offset >= 0 && offset <= audioSource.clip.length)
            {
                audioSource.time = offset + audioDelay;
                audioSource.Play();
                audioSource.pitch = songSpeed; // Set the playback speed
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

    private void OnSongSpeedChanged()
    {
        if (audioSource.clip != null)
        {
            // Reload the audio clip with the updated speed settings
            ApplySongSpeed();
        }
    }

    private void ApplySongSpeed()
    {
        audioSource.pitch = songSpeed; // Update the pitch to match the new speed
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.Play();
        }
    }
}
