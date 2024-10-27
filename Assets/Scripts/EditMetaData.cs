using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;

public class EditMetaData : MonoBehaviour
{
    public Info metaData;
    public static EditMetaData instance;
    public string folderPath;
    public List<TMP_InputField> inputs;

    void Start()
    {
        instance = this;
        string infoPath = "info.dat";

        if (!folderPath.Contains("info.dat"))
        {
            infoPath = "Info.dat";
        }

        string rawData = File.ReadAllText(Path.Combine(folderPath, infoPath));
        metaData = JsonUtility.FromJson<Info>(rawData);

        File.WriteAllText(folderPath + "\\info_2.dat", JsonUtility.ToJson(metaData, true));

        // put calculate thing here
        LoadValues();
    }

    void LoadValues()
    {
        inputs[0].text = metaData._songName;
        inputs[1].text = metaData._songSubName;
        inputs[2].text = metaData._songAuthorName;
        inputs[3].text = metaData._levelAuthorName;
        inputs[4].text = metaData._beatsPerMinute.ToString();
        inputs[5].text = metaData._previewStartTime.ToString();
        inputs[6].text = metaData._previewDuration.ToString();
        inputs[7].text = metaData._songFilename;
        inputs[8].text = metaData._coverImageFilename;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
