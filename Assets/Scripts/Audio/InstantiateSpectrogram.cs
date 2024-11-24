using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class InstantiateSpectrogram : MonoBehaviour
{
    [Header("Load Settings")]
    public bool waveform;
    public static InstantiateSpectrogram instance;
    public AudioSource audioSource;
    public int seconds;
    public float rawTime;
    public int spawnOffset;
    public int despawnOffset;
    public float length;
    public GameObject spectroChunk;
    public Transform spectroChunkParent;
    public Transform Grid;
    bool started;
    public int scrollCache;

    [Header("Shell Settings")]
    public int layers;
    public float height;
    public GameObject shellLayer;

    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        rawTime = SpawnObjects.instance.GetRealTimeFromBeat(SpawnObjects.instance.currentBeat);

        if (length == 0)
        {
            if (audioSource.clip) length = audioSource.clip.length;
        }
        else if (!started)
        {
            Reload();
            started = true;
            int capacity = Mathf.CeilToInt(audioSource.clip.length);
            for (int i = 0; i < capacity; i++)
            {
                SpectrogramTextures.instance.spectrogram.Add(null);
                SpectrogramTextures.instance.waveform.Add(null);
            }
        }

        if (rawTime > seconds + 1)
        {
            if (scrollCache == -1)
            {
                scrollCache = 1;
                Reload();
            }
            else if (seconds + spawnOffset < length)
            {
                scrollCache = 1;
                seconds++;
                foreach (Transform chunk in spectroChunkParent.transform)
                {
                    var pos = Grid.position.z - (SpawnObjects.instance.BeatFromRealTime(SpawnObjects.instance.PositionFromBeat(2)) * despawnOffset * SpawnObjects.instance.editorScale);
                    if (chunk.transform.position.z < pos) Destroy(chunk.gameObject);
                }
                SpawnSpectroChunk(seconds + spawnOffset);
            }
        }

        else if (rawTime < seconds - 1)
        {
            if (scrollCache == 1)
            {
                scrollCache = -1;
                Reload();
            }
            else if (seconds - despawnOffset > 0)
            {
                scrollCache = -1;
                seconds--;
                foreach (Transform chunk in spectroChunkParent.transform)
                {
                    var pos = Grid.position.z + (SpawnObjects.instance.BeatFromRealTime(SpawnObjects.instance.PositionFromBeat(1)) * spawnOffset * SpawnObjects.instance.editorScale);
                    if (chunk.transform.position.z > pos) Destroy(chunk.gameObject);
                }
                if (seconds - despawnOffset - 2 >= 0 && seconds - despawnOffset - 1 < length) SpawnSpectroChunk(seconds - despawnOffset - 2);
            }
        }
    }

    public void Reload()
    {
        foreach (Transform chunk in spectroChunkParent.transform)
        {
            Destroy(chunk.gameObject);
        }

        for (int i = 0; i < despawnOffset + spawnOffset + 3; i++)
        {
            if (seconds + i - despawnOffset - 2 >= 0 && seconds + i - despawnOffset - 1 < length)
            {
                SpawnSpectroChunk(i + seconds - despawnOffset - 2);
            }
        }
    }

    void SpawnSpectroChunk(int beat)
    {
        GameObject go = Instantiate(spectroChunk);
        go.transform.SetParent(spectroChunkParent);
        var pos = SpawnObjects.instance.BeatFromRealTime(SpawnObjects.instance.PositionFromBeat(beat));
        var scale = SpawnObjects.instance.BeatFromRealTime(SpawnObjects.instance.PositionFromBeat(1));
        go.transform.localPosition = new Vector3(-2.5f, 0, pos * SpawnObjects.instance.editorScale + scale * SpawnObjects.instance.editorScale / 2);
        go.transform.localScale = new Vector3(scale * SpawnObjects.instance.editorScale, -3, 2);
        go.GetComponent<WaveformAndSpectrogram>().startOffset = beat;
    }
}
