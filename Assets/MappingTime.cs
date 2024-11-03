using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MappingTime : MonoBehaviour
{
    public LoadMap loadMap;
    public GameObject minuteHand;
    public GameObject secondHand;
    public TextMeshProUGUI text;

    // Update is called once per frame
    void FixedUpdate()
    {
        // Mapping loadMap time to seconds
        float currentTime = loadMap.mappingTime * 60;

        // Rotate the second hand based on seconds in the minute
        secondHand.transform.localEulerAngles = new Vector3(0, 0, -((currentTime % 60) * 6));

        // Rotate the minute hand based on total minutes
        minuteHand.transform.localEulerAngles = new Vector3(0, 0, -((currentTime / 60) % 60) * 6);

        // Update text in hours, minutes, and seconds format
        int hours = Mathf.FloorToInt(currentTime / 3600f) % 24;
        int minutes = Mathf.FloorToInt(currentTime / 60f) % 60;
        int seconds = Mathf.FloorToInt(currentTime) % 60;

        text.text = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }
}
