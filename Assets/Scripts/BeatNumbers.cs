using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BeatNumbers : MonoBehaviour
{
    public static BeatNumbers instance;
    public GameObject beatObject;
    public Transform parent;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    public void SpawnNumber(int beat)
    {
        if (beat < SpawnObjects.instance.BeatFromRealTime(LoadSong.instance.audioSource.clip.length))
        {
            if (beat >= 0)
            {
                GameObject go = Instantiate(beatObject);
                go.transform.SetParent(parent);
                go.transform.localPosition = new Vector3(0, 0, SpawnObjects.instance.PositionFromBeat(beat) * SpawnObjects.instance.editorScale);
                go.GetComponent<TextMeshPro>().text = beat.ToString();
                go.transform.GetChild(0).localScale = new Vector3(go.transform.GetChild(0).localScale.x, go.transform.GetChild(0).localScale.y, SpawnObjects.instance.editorScale / 75f);
            }
        }
    }
}
