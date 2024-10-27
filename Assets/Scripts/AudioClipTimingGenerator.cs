using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class AudioClipTimingGenerator : MonoBehaviour
{
    public AudioAnalysis audioAnalysis;
    public static AudioClipTimingGenerator instance;
    public AudioSource audioSource;

    // A list to store the timing events (in seconds)
    public List<float> timingEvents;
    public bool generateMap;
    public float threshold;
    public int precision;
    public bool debug;

    void Start()
    {
        instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            GenerateMap(audioSource);
        }
    }

    public void GenerateMap(AudioSource audio)
    {
        if (debug)
        {
            for (int i = 0; i < LoadMap.instance.beats.Count * precision; i++)
            {
                float time = SpawnObjects.instance.GetRealTimeFromBeat((float)i / (float)precision);

                float result = audioAnalysis.GetVolumeAtTime(audio.clip, time) * audioAnalysis.GetHighestFrequencyValueAtTime(audio.clip, time, 2048);
                float resultBeforeThreshold = audioAnalysis.GetVolumeAtTime(audio.clip, time - threshold) * Mathf.Sqrt(audioAnalysis.GetHighestFrequencyValueAtTime(audio.clip, time - threshold, 2048));
                float resultAfterThreshold = audioAnalysis.GetVolumeAtTime(audio.clip, time + threshold) * Mathf.Sqrt(audioAnalysis.GetHighestFrequencyValueAtTime(audio.clip, time + threshold, 2048));

                float averageVolume = (resultBeforeThreshold + resultAfterThreshold) / 1.5f;

                Debug.Log("Beat: " + (float)i / (float)precision + $", vol: {result}, volAverage: {averageVolume}");

                if (result > averageVolume)
                {
                    LoadMap.instance.beats[Mathf.FloorToInt(i / precision)].colorNotes.Add(new colorNotes
                    {
                        b = (float)i / (float)precision,
                        x = 0,
                        y = 0,
                        a = 0,
                        c = 0,
                        d = 1
                    });
                }
            }
        }
    }

    public void GenerateMapAtCurrentBeat(AudioSource audio)
    {
        float time = SpawnObjects.instance.GetRealTimeFromBeat(SpawnObjects.instance.currentBeat);

        float result = audioAnalysis.GetVolumeAtTime(audio.clip, time);
        float resultBeforeThreshold = audioAnalysis.GetVolumeAtTime(audio.clip, time - threshold);
        float resultAfterThreshold = audioAnalysis.GetVolumeAtTime(audio.clip, time + threshold);

        float averageVolume = (resultBeforeThreshold + resultAfterThreshold) / 2;

        if (result > averageVolume)
        {
            LoadMap.instance.beats[Mathf.FloorToInt(SpawnObjects.instance.currentBeat)].colorNotes.Add(new colorNotes
            {
                b = SpawnObjects.instance.currentBeat,
                x = 0,
                y = 0,
                a = 0,
                c = 0,
                d = 1
            });
        }

    }


    // LoadMap.instance.beats[Mathf.FloorToInt(beat)].colorNotes.Add(new colorNotes
    // {
    //     b = beat,
    //     x = 0,
    //     y = 0,
    //     a = 0,
    //     c = 0,
    //     d = 1
    // });

}
