using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Unity.VisualScripting;

public class ReloadMap : MonoBehaviour
{
    public static ReloadMap instance;

    void Start()
    {
        instance = this;
    }

    public void ReloadEverything()
    {
        Objects();
        AudioGraph();
        Lines();
    }

    public void Objects()
    {
        SpawnObjects.instance.LoadObjectsFromScratch(SpawnObjects.instance.currentBeat, true, true);
    }

    public void AudioGraph()
    {
        InstantiateSpectrogram.instance.Reload();
    }

    public void Lines()
    {
        DrawLines.instance.DrawLinesWhenRequired();
    }
}
