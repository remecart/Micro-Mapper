using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Clock : MonoBehaviour
{
    public LoadSong loadSong;
    public SpawnObjects spawnObjects;
    public GameObject minute;
    public GameObject seconds;
    public TextMeshProUGUI text;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (loadSong.audioSource.clip != null)
        {
            float currentTime = spawnObjects.GetRealTimeFromBeat(spawnObjects.currentBeat);
            seconds.transform.localEulerAngles = new Vector3(0, 0, -currentTime * 360 / 60);
            minute.transform.localEulerAngles = new Vector3(0, 0, -currentTime * 0.5f);
            text.text = $"{Mathf.FloorToInt(currentTime / 60f):D2}:{Mathf.FloorToInt(currentTime)%60:D2}.{Mathf.FloorToInt((currentTime - Mathf.FloorToInt(currentTime)) * 100):D2}";
        }
    }
}
