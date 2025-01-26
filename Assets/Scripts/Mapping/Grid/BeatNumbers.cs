using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BeatNumbers : MonoBehaviour
{
    public static BeatNumbers instance;
    public GameObject beatObject;
    public GameObject textObject;
    public Transform parent;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    public void SpawnNumber(int beat)
    {
        var max = Int32.MaxValue;
        if (LoadSong.instance.audioSource.clip) max = Mathf.CeilToInt(SpawnObjects.instance.BeatFromRealTime(LoadSong.instance.audioSource.clip.length));
        
        if (beat < max)
        {
            if (beat >= 0)
            {
                GameObject goLine = Instantiate(beatObject);
                goLine.transform.SetParent(parent);
                int width = 4;
                if (Placement.instance.enableME) width = Placement.instance.MEgridSize.x;
                goLine.transform.localScale = new Vector3(width, goLine.transform.localScale.y, SpawnObjects.instance.editorScale / 75f);
                goLine.transform.localPosition = new Vector3(-2.5f, 0, SpawnObjects.instance.PositionFromBeat(beat) * SpawnObjects.instance.editorScale);

                GameObject goText = Instantiate(textObject);
                goText.transform.SetParent(goLine.transform);
                goText.transform.localPosition = new Vector3(-0.55f, goLine.transform.localScale.y, 0);
                goText.GetComponent<TextMeshPro>().text = beat.ToString();

                //GameObject go = Instantiate(beatObject);
                //go.transform.SetParent(parent);
                //go.transform.localPosition = new Vector3(0, 0, SpawnObjects.instance.PositionFromBeat(beat) * SpawnObjects.instance.editorScale);
                //go.GetComponent<TextMeshPro>().text = beat.ToString();
                //go.transform.GetChild(0).localScale = new Vector3(go.transform.GetChild(0).localScale.x, go.transform.GetChild(0).localScale.y, SpawnObjects.instance.editorScale / 75f);
            }
        }
    }
}
